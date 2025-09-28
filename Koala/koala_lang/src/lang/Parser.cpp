#include "KoalaLang/Parser.h"
#include "KoalaLang/ASTNodes.h"

#include <format>
#include <iostream>
#include <memory>

namespace KoalaLang{

    #define PARSER_CURRENT_TOKEN m_tokens[m_idx]

    void Parser::Parse(const std::string& globalModuleName){
        if(m_tokens.empty()) return;

        //Unknown tokens panic
        bool success = true;
        while(m_idx < m_tokens.size()){
            if(PARSER_CURRENT_TOKEN.type == TokenType::Unknown){
                Panic(std::format("Unknown token('{}')", std::string(PARSER_CURRENT_TOKEN.value)));
                success = false;
            }
            Next();
        }

        if(!success) std::exit(-1);

        //creating ast tree
        m_idx = 0;
        m_modulesStack.push_back(globalModuleName);
        SHARED_PTR_T(ASTCodeBlock) m_code_p = ParseCodeBlock(false);
        m_code = *m_code_p;
        m_code_p.reset();
    }

    SHARED_PTR_T(ASTFunction) Parser::ParseFunctionDecl(){
        FatalNext(TokenType::Identifier);
        std::string functionName = MakeQualifiedName(PARSER_CURRENT_TOKEN.value);

        FatalNext(TokenType::LParen);
        //TODO: args parsing
        FatalNext(TokenType::RParen);

        FatalNext(TokenType::Colon);
        FatalNext(TokenType::Identifier);
        std::string typeName = PARSER_CURRENT_TOKEN.value;
        
        FatalNext(TokenType::LBrace);
        SHARED_PTR_T(ASTCodeBlock) body = ParseCodeBlock();

        return std::make_shared<ASTFunction>(functionName, typeName, body);
    }

    SHARED_PTR_T(ASTModule) Parser::ParseModuleDecl() {
        FatalNext(TokenType::Identifier);
        std::string name = PARSER_CURRENT_TOKEN.value;
        FatalNext(TokenType::LBrace);

        m_modulesStack.push_back(name);
        SHARED_PTR_T(ASTCodeBlock) body = std::make_shared<ASTCodeBlock>(std::vector<SHARED_PTR_T(ASTNode)>());
        ParseCodeBlock();
        m_modulesStack.pop_back();

        std::string qualifiedName = MakeQualifiedName(name);
        return std::make_shared<ASTModule>(body, qualifiedName);
    }

    SHARED_PTR_T(ASTNode) Parser::ParseExpression() {
        SHARED_PTR_T(ASTNode) result = ParseTerm();
        while (m_idx < m_tokens.size()) {
            if (PARSER_CURRENT_TOKEN.type == TokenType::Plus || PARSER_CURRENT_TOKEN.type == TokenType::Minus) {
                BinOperation op = PARSER_CURRENT_TOKEN.type == TokenType::Plus ? BinOperation::Addition : BinOperation::Subtraction;
                Next();
                SHARED_PTR_T(ASTNode) right = ParseTerm();
                result = std::make_shared<ASTBinaryOperation>(result, op, right);
            }
            else break;
        }
        return result;
    }

    SHARED_PTR_T(ASTNode) Parser::ParseTerm() {
        SHARED_PTR_T(ASTNode) result = ParseFactor();
        while (m_idx < m_tokens.size()) {
            if (PARSER_CURRENT_TOKEN.type == TokenType::Asterisk || PARSER_CURRENT_TOKEN.type == TokenType::Slash || PARSER_CURRENT_TOKEN.type == TokenType::Percent) {
                BinOperation op = PARSER_CURRENT_TOKEN.type == TokenType::Asterisk ? BinOperation::Multiplication : PARSER_CURRENT_TOKEN.type == TokenType::Slash ? BinOperation::Division : BinOperation::ReminderOnDivision;
                Next();
                SHARED_PTR_T(ASTNode) right = ParseFactor();
                result = std::make_shared<ASTBinaryOperation>(result, op, right);
            }
            else break;
        }
        return result;
    }

    SHARED_PTR_T(ASTNode) Parser::ParseFactor() {
        if (PARSER_CURRENT_TOKEN.type == TokenType::Number) {
            long int value = std::stol(PARSER_CURRENT_TOKEN.value);
            Next();
            return std::make_shared<ASTNumberLiteral>(value);
        }
        if (PARSER_CURRENT_TOKEN.type == TokenType::Float) {
            double value = std::stod(PARSER_CURRENT_TOKEN.value);
            Next();
            return std::make_shared<ASTFloatLiteral>(value);
        }
        if (PARSER_CURRENT_TOKEN.type == TokenType::LParen) {
            Next();//skip (
            SHARED_PTR_T(ASTNode) result = ParseExpression();
            FatalThenNext(TokenType::RParen);
            return result;
        }
        if (PARSER_CURRENT_TOKEN.type == TokenType::Minus) {
            Next();
            SHARED_PTR_T(ASTNode) val = ParseFactor();
            return std::make_shared<ASTBinaryOperation>(std::make_shared<ASTNumberLiteral>(-1), BinOperation::Multiplication, val);
        }

        Panic("Unexpected token in expression");
        return nullptr;
    }

    SHARED_PTR_T(ASTCodeBlock) Parser::ParseCodeBlock(bool shift){
        if (shift) Next();
        SHARED_PTR_T(ASTCodeBlock) block = std::make_shared<ASTCodeBlock>(std::vector<SHARED_PTR_T(ASTNode)>());

        while(m_idx < m_tokens.size() && PARSER_CURRENT_TOKEN.type != TokenType::RBrace){

            if(PARSER_CURRENT_TOKEN.type == TokenType::Keyword){
                if(PARSER_CURRENT_TOKEN.value == "fn") block->GetNodes().push_back(ParseFunctionDecl());
                else if(PARSER_CURRENT_TOKEN.value == "module") block->GetNodes().push_back(ParseModuleDecl());

                else if(PARSER_CURRENT_TOKEN.value == "ret"){
                    Next();
                    SHARED_PTR_T(ASTNode) expr = ParseExpression();
                    FatalThenNext(TokenType::Semicolon);
                    block->GetNodes().push_back(std::make_shared<ASTRet>(expr));
                }
            }

            else if(PARSER_CURRENT_TOKEN.type == TokenType::LBrace){ //inner block
                SHARED_PTR_T(ASTCodeBlock) innerBlock = ParseCodeBlock();
                block->GetNodes().push_back(innerBlock);
            }

            Next();
        }

        return block;
    }

    void Parser::Panic(const std::string& msg){
        std::string line;
        {
            size_t lineCounter = 1;
            for(int l = 0; l < m_source.length(); ++l){
                if(m_source[l] == '\n'){
                    lineCounter += 1;
                }
                if(lineCounter == PARSER_CURRENT_TOKEN.line){
                    if(m_source[l] == '\n') continue;
                    line += m_source[l];
                }
                if(lineCounter > PARSER_CURRENT_TOKEN.line){
                    break;
                }
            }
        }
        std::cerr << std::format("Error at ln: {}, col: {}| {}: {}", PARSER_CURRENT_TOKEN.line, PARSER_CURRENT_TOKEN.column, msg, line) << "\n";
    }

    std::string Parser::MakeQualifiedName(const std::string &name) {
        if (m_modulesStack.empty()) return name;
        std::string q;
        for (size_t i = 0; i < m_modulesStack.size(); i++) {
            if (i) q += ".";
            q += m_modulesStack[i];
        }
        q += ".";
        q += name;
        return q;
    }

    void Parser::Next(){
        m_idx += 1;
    }

    void Parser::FatalNext(const TokenType type){
        Next();
        if(m_idx < m_tokens.size()){
            if(PARSER_CURRENT_TOKEN.type == type) return;
        }
        Panic("Unexpected token");
        std::exit(-1);
    }

    void Parser::FatalThenNext(const TokenType type) {
        if (PARSER_CURRENT_TOKEN.type == type) {
            Next();
            return;
        }
        Panic("Unexpected token");
        std::exit(-1);
    }
}
