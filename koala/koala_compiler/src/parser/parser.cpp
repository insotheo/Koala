#include "parser/parser.hpp"

#include <memory>
#include <format>
#include <iostream>

namespace koalac{

    static const std::unordered_map<std::string, InstrDescriptor> InstrTabel = {
        {"ret", {{ .Op = OpCode::RET, .Format = {} }}},
        {"mov", {
            { .Op = OpCode::MOV_IMM16, .Format = {ArgType::Register, ArgType::Imm16} },
            { .Op = OpCode::MOV_IMM64, .Format = {ArgType::Register, ArgType::Imm64} },
            { .Op = OpCode::MOV_REG, .Format = {ArgType::Register, ArgType::Register} }
        }},

        { "add", {
            { .Op = OpCode::ADD_IMM16, .Format = { ArgType::Register, ArgType::Register, ArgType::Imm16 } },
            { .Op = OpCode::ADD_REG, .Format = { ArgType::Register, ArgType::Register, ArgType::Register } }
        }},
        { "sub", {
            { .Op = OpCode::SUB_IMM16, .Format = { ArgType::Register, ArgType::Register, ArgType::Imm16 } },
            { .Op = OpCode::SUB_IMM16_R, .Format = { ArgType::Register, ArgType::Imm16, ArgType::Register } },
            { .Op = OpCode::SUB_REG, .Format = { ArgType::Register, ArgType::Register, ArgType::Register } }
        }},
        { "mul", {
            { .Op = OpCode::MUL_IMM16, .Format = { ArgType::Register, ArgType::Register, ArgType::Imm16 } },
            { .Op = OpCode::MUL_REG, .Format = { ArgType::Register, ArgType::Register, ArgType::Register } }
        }},
        { "idiv", {
            { .Op = OpCode::IDIV_IMM16, .Format = { ArgType::Register, ArgType::Register, ArgType::Imm16 } },
            { .Op = OpCode::IDIV_IMM16_R, .Format = { ArgType::Register, ArgType::Imm16, ArgType::Register } },
            { .Op = OpCode::IDIV_REG, .Format = { ArgType::Register, ArgType::Register, ArgType::Register } }
        }},
        { "div", {
            { .Op = OpCode::DIV_IMM16, .Format = { ArgType::Register, ArgType::Register, ArgType::Imm16 } },
            { .Op = OpCode::DIV_IMM16_R, .Format = { ArgType::Register, ArgType::Imm16, ArgType::Register } },
            { .Op = OpCode::DIV_REG, .Format = { ArgType::Register, ArgType::Register, ArgType::Register } }
        }},
        { "neg", {
            { .Op = OpCode::NEG_IMM16, .Format = { ArgType::Register,ArgType::Imm16 } },
            { .Op = OpCode::NEG_REG, .Format = { ArgType::Register, ArgType::Register } }
        }},
        { "irem", {
            { .Op = OpCode::IREM_IMM16, .Format = { ArgType::Register, ArgType::Register, ArgType::Imm16 } },
            { .Op = OpCode::IREM_IMM16_R, .Format = { ArgType::Register, ArgType::Imm16, ArgType::Register } },
            { .Op = OpCode::IREM_REG, .Format = { ArgType::Register, ArgType::Register, ArgType::Register } }
        }},
        { "rem", {
            { .Op = OpCode::REM_IMM16, .Format = { ArgType::Register, ArgType::Register, ArgType::Imm16 } },
            { .Op = OpCode::REM_IMM16_R, .Format = { ArgType::Register, ArgType::Imm16, ArgType::Register } },
            { .Op = OpCode::REM_REG, .Format = { ArgType::Register, ArgType::Register, ArgType::Register } }
        }},
        { "and", {
            { .Op = OpCode::AND_IMM16, .Format = { ArgType::Register, ArgType::Register, ArgType::Imm16 } },
            { .Op = OpCode::AND_REG, .Format = { ArgType::Register, ArgType::Register, ArgType::Register } }
        }},
        { "or", {
            { .Op = OpCode::OR_IMM16, .Format = { ArgType::Register, ArgType::Register, ArgType::Imm16 } },
            { .Op = OpCode::OR_REG, .Format = { ArgType::Register, ArgType::Register, ArgType::Register } }
        }},
        { "xor", {
            { .Op = OpCode::XOR_IMM16, .Format = { ArgType::Register, ArgType::Register, ArgType::Imm16 } },
            { .Op = OpCode::XOR_REG, .Format = { ArgType::Register, ArgType::Register, ArgType::Register } }
        }},
        { "not", {
            { .Op = OpCode::NOT_IMM16, .Format = { ArgType::Register,ArgType::Imm16 } },
            { .Op = OpCode::NOT_REG, .Format = { ArgType::Register, ArgType::Register } }
        }},
        { "shl", {
            { .Op = OpCode::SHL_IMM16, .Format = { ArgType::Register, ArgType::Register, ArgType::Imm16 } },
            { .Op = OpCode::SHL_IMM16_R, .Format = { ArgType::Register, ArgType::Imm16, ArgType::Register } },
            { .Op = OpCode::SHL_REG, .Format = { ArgType::Register, ArgType::Register, ArgType::Register } }
        }},
        { "shr", {
            { .Op = OpCode::SHR_IMM16, .Format = { ArgType::Register, ArgType::Register, ArgType::Imm16 } },
            { .Op = OpCode::SHR_IMM16_R, .Format = { ArgType::Register, ArgType::Imm16, ArgType::Register } },
            { .Op = OpCode::SHR_REG, .Format = { ArgType::Register, ArgType::Register, ArgType::Register } }
        }},
        { "sar", {
            { .Op = OpCode::SAR_IMM16, .Format = { ArgType::Register, ArgType::Register, ArgType::Imm16 } },
            { .Op = OpCode::SAR_IMM16_R, .Format = { ArgType::Register, ArgType::Imm16, ArgType::Register } },
            { .Op = OpCode::SAR_REG, .Format = { ArgType::Register, ArgType::Register, ArgType::Register } }
        }},
        {"jmp", {{ .Op = OpCode::_JMP_UNDEFINED, .Format = { ArgType::Label } }}},
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
        bool isLocalLabel = ident.starts_with('.');
        Span startSpan = m_Cur.Span;

        Next();
        if(m_Cur.Type != TokenType::Colon){
            Panic("Unexpected token after label identifier. Expected colon ':'.", m_Cur.Span);
            Sync();
        }
        Next();

        auto it = m_Labels.find(ident);
        if(it != m_Labels.end()){
            Panic(std::format("Label '{}' was declared multiple times", ident), startSpan);
            return;
        }
        
        if(isLocalLabel){
            if(m_CurGlobalLabel.empty()){
                Panic(std::format("Cannot assign local label '{}'. No global label found.", ident), startSpan);
                return;
            }
            ident = m_CurGlobalLabel + ident;
        } else {
            m_CurGlobalLabel = ident;
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
            (m_Cur.Type == TokenType::Register || m_Cur.Type == TokenType::Number || m_Cur.Type == TokenType::Identifier)){

                switch(m_Cur.Type){
                    case TokenType::Register:{
                        uint8_t regVal = static_cast<uint8_t>(std::get<uint64_t>(m_Cur.Val));
                        args.push_back(ParserArg(ArgType::Register, regVal));
                        break;
                    }
                    
                    case TokenType::Number:{
                        uint64_t argVal = std::get<uint64_t>(m_Cur.Val);
                        if(argVal <= UINT16_MAX)
                            args.push_back(ParserArg(ArgType::Imm16, static_cast<uint16_t>(argVal)));
                        else
                            args.push_back(ParserArg(ArgType::Imm64, argVal));
                        break;
                    }
                    
                    case TokenType::Identifier:{
                        std::string labelName = std::get<std::string>(m_Cur.Val);
                        if(labelName.starts_with('.')){ //local label
                            if(m_CurGlobalLabel.empty()){
                                Panic(std::format("No global label found to append '{}'", labelName), m_Cur.Span);
                                break;
                            }
                            labelName = m_CurGlobalLabel + labelName;
                        }
                        args.push_back(ParserArg(ArgType::Label, labelName));
                        break;
                    }

                    default: break;
                }

                Next();
                if(m_Cur.Type == TokenType::Comma)
                    Next();
                else break;
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