#include "parser/parser.hpp"

#include "lexer/token.hpp"
#include "parser/descriptor.hpp"
#include <memory>
#include <format>
#include <string>
#include <iostream>

namespace koalac{

    static const std::unordered_map<std::string, InstrDescriptor> InstrTabel = {
        {"ret", {{ .Op = OpCode::RET, .Format = {} }}},
        {"mov", {
            { .Op = OpCode::MOV_IMM16, .Format = {ArgType::Register, ArgType::Imm16} },
            { .Op = OpCode::MOV_REG, .Format = {ArgType::Register, ArgType::Register} }
        }}
    };

    IRProgram Parser::MakeProgram(){
        IRNodes nodes;

        while(m_Cur.Type != TokenType::EndOfFile){
            if(m_Cur.Type == TokenType::Identifier) ParseLabel(&nodes);
            else if(m_Cur.Type == TokenType::Keyword) ParseInstruction(&nodes);
            else{
                Panic("Unexpected token. Expected instruction or label.", m_Cur.Span);
                Sync();
            }
        }

        return IRProgram(std::move(nodes));
    }


    void Parser::ParseLabel(IRNodes* nodes){
        std::string ident = std::get<std::string>(m_Cur.Val);
        Span startSpan = m_Cur.Span;
        Next();
        if(m_Cur.Type != TokenType::Colon){
            Panic("Unexpected token after label identifier. Expected colon ':'.", m_Cur.Span);
            Sync();
        }
        Next();

        auto it = m_Labels.find(ident);
        if(it != m_Labels.end()){
            Panic(std::format("Lable '{}' was declared multiple times", ident), startSpan);
            return;
        }
        m_Labels.emplace(ident, startSpan);

        nodes->push_back(std::make_unique<IRLabel>(ident, startSpan));
    }

    void Parser::ParseInstruction(IRNodes* nodes){
        std::string instr = std::get<std::string>(m_Cur.Val);
        Span startSpan = m_Cur.Span;
        Next();

        auto instrIt = InstrTabel.find(instr);
        if(instrIt == InstrTabel.end()){
            Panic(std::format("Unknown instruction '{}'", instr), startSpan);
            Sync();
            return;
        }

        std::vector<ParserArg> args;
        while(m_Cur.Type != TokenType::EndOfFile &&
            (m_Cur.Type == TokenType::Register || m_Cur.Type == TokenType::Number)){
                uint64_t argVal = std::get<uint64_t>(m_Cur.Val);
                
                switch(m_Cur.Type){
                    case TokenType::Register:
                        args.push_back(ParserArg(ArgType::Register, static_cast<uint8_t>(argVal)));
                        break;
                    
                    case TokenType::Number:
                        args.push_back(ParserArg(ArgType::Imm16, static_cast<uint16_t>(argVal)));
                        break;
                    
                    default: break;
                }

                Next();
                if(m_Cur.Type == TokenType::Comma)
                    Next();
        }

        OpCode op = OpCode::NONE;

        for(const InstrVariant& instrVar : instrIt->second){
            bool matches = true;

            if(instrVar.Format.size() != args.size())
                continue;
            size_t argsCount = args.size();

            for(size_t i = 0; i < argsCount; ++i){
                if(args[i].Type != instrVar.Format[i]){
                    matches = false;
                    break;
                }
            }

            if(matches){
                op = instrVar.Op;
                break;
            }
        }

        if(op == OpCode::NONE){
            Panic(std::format("Invalid arguments for '{}'.", instr), startSpan);
            Sync();
            return;
        }
        
        std::vector<IRArg> valArgs;
        for(const auto& arg : args){
            valArgs.push_back(arg.Val);
        }

        nodes->push_back(std::make_unique<IRInstruction>(op, std::move(valArgs), startSpan));
    }
    

    void Parser::Next(){
        m_Cur = m_Next;
        m_Next = m_Lexer->NextToken();
    }

    void Parser::Panic(const std::string& msg, struct Span span){
        m_Errors.push_back(ParserError(msg, span));
    }

    void Parser::Sync(){
        while(m_Cur.Type != TokenType::EndOfFile &&
            !(m_Cur.Type  == TokenType::Keyword ||
            (m_Cur.Type == TokenType::Identifier && m_Next.Type == TokenType::Colon)))
            { Next(); }
    }

    void Parser::PrintErrors(){
        for(const ParserError& err : m_Errors){
            std::cerr << std::format("[ERROR(ln: {}, col: {})] {}\n", err.Span.Line, err.Span.Column, err.Msg);
        }
    }

}