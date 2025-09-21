#include "KoalaLang/Parser.h"
#include "KoalaLang/ASTNodes.h"

#include <format>
#include <iostream>
#include <memory>

namespace KoalaLang{

    #define PARSER_CURRENT_TOKEN m_tokens[m_idx]

    void Parser::Parse(){
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

        //creating ast tree
        m_idx = 0;
        while(m_idx < m_tokens.size() && success){
            if(PARSER_CURRENT_TOKEN.type == TokenType::Keyword){
                if(PARSER_CURRENT_TOKEN.value == "fn") ParseFunctionDecl();
            }
            
            Next();
        }
    }

    void Parser::ParseFunctionDecl(){
        FatalNext(TokenType::Identifier);
        std::string functionName = PARSER_CURRENT_TOKEN.value;

        FatalNext(TokenType::LParen);
        //TODO: args parsing
        FatalNext(TokenType::RParen);

        FatalNext(TokenType::Colon);
        FatalNext(TokenType::Identifier);
        std::string typeName = PARSER_CURRENT_TOKEN.value;
        
        FatalNext(TokenType::LBrace);
        SHARED_PTR_T(ASTCodeBlock) body = ParseCodeBlock();
        
        m_code.GetNodes().push_back(std::make_shared<ASTFunction>(functionName, typeName, body));
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
            if (PARSER_CURRENT_TOKEN.type == TokenType::Asterisk || PARSER_CURRENT_TOKEN.type == TokenType::Slash) {
                BinOperation op = PARSER_CURRENT_TOKEN.type == TokenType::Asterisk ? BinOperation::Multiplication : BinOperation::Division;
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

    SHARED_PTR_T(ASTCodeBlock) Parser::ParseCodeBlock(){
        Next();
        SHARED_PTR_T(ASTCodeBlock) block = std::make_shared<ASTCodeBlock>(std::vector<SHARED_PTR_T(ASTNode)>());

        while(m_idx < m_tokens.size() && PARSER_CURRENT_TOKEN.type != TokenType::RBrace){

            if(PARSER_CURRENT_TOKEN.type == TokenType::Keyword){
                if(PARSER_CURRENT_TOKEN.value == "ret"){
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
