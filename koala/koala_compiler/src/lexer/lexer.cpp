#include "lexer/lexer.hpp"
#include "lexer/token.hpp"

#include <vm_config.h>
#include <cctype>
#include <cstdint>
#include <sstream>

#define M_IDX_IS_VALID (m_Idx < m_ContentLength)
#define CUR_CHAR (m_Content[m_Idx])


bool isBin(char c){
    return (c == '0' || c == '1');
}

bool isOct(char c){
    return (c >= '0' && c <= '7');
}

bool isDecimal(char c){
    return (c >= '0' && c <= '9');
}

bool isHex(char c){
    return (c >= '0' && c <= '9') ||
           (c >= 'a' && c <= 'f') ||
           (c >= 'A' && c <= 'F');
}

uint64_t parseRegisterIdx(const std::string& s){
    uint64_t n = 0;
    for(size_t i = 1; i < s.size(); ++i){
        n = n * 10 + (static_cast<unsigned char>(s[i]) - '0');
        if(n >= KOALA_VM_REGISTERS_COUNT) return UINT64_MAX;
    }
    return n;
}

bool isRegister(const std::string& s){
    if(s.size() < 2) return false; //at least r0
    if(s[0] != 'r') return false;

    for(size_t i = 1; i < s.size(); ++i){
        if(!std::isdigit(s[i])) return false;
    }

    if(parseRegisterIdx(s) == UINT64_MAX) return false;

    return true;
}

namespace koalac{

    void Lexer::Next(){
        m_Idx += 1;
        if(M_IDX_IS_VALID){
            if(m_Content[m_Idx] == '\n'){
                m_CurLine += 1;
                m_CurColumn = 0;
            } else {
                m_CurColumn += 1;
            }
        }
    }

    void Lexer::SkipWhitespacesAndComments(){
        while(M_IDX_IS_VALID && (std::isspace(CUR_CHAR) || CUR_CHAR == ';')){
            if(CUR_CHAR == ';') {
                while(M_IDX_IS_VALID && CUR_CHAR != '\n') Next();
            }
            Next();
        }
    }

    Token Lexer::NextToken(){
        SkipWhitespacesAndComments();
        
        Span startSpan = { .Line = m_CurLine, .Column = m_CurColumn };

        if(!M_IDX_IS_VALID)
            return Token(TokenType::EndOfFile, startSpan);

        char c = CUR_CHAR;
        
        if(std::isdigit(c)){ //0..., 0b..., 0o..., 0x...
            int radix = 10;
            bool hasPrefix = false;

            if(m_Idx + 1 < m_ContentLength){
                char nextC = m_Content[m_Idx + 1];
                
                switch (nextC) {
                    case 'b': hasPrefix = true; radix = 2; break;
                    case 'o': hasPrefix = true; radix = 8; break;
                    case 'x': hasPrefix = true; radix = 16; break;
                    default: break;
                }
            }
            if(hasPrefix) m_Idx += 2; //skips prefix

            std::stringstream ss;
            bool isNumberValid = true;

            while(M_IDX_IS_VALID && (std::isdigit(CUR_CHAR) || isHex(CUR_CHAR))){
                char tmp = CUR_CHAR;
                ss << tmp;

                //check is tmp is valid
                switch(radix){
                    case 2: isNumberValid = isNumberValid && isBin(tmp); break;
                    case 8: isNumberValid = isNumberValid && isOct(tmp); break;
                    case 10: isNumberValid = isNumberValid && isDecimal(tmp); break;
                    case 16: isNumberValid = isNumberValid && isHex(tmp); break;
                    default: isNumberValid = false; break;
                }
                
                Next();
            }

            std::string str = ss.str();
            if(str.empty()) isNumberValid = false; //example: 0x

            TokenType type = isNumberValid ? TokenType::Number : TokenType::Unknown;
            uint64_t val = UINT64_MAX;

            if(isNumberValid){
                try{
                    val = std::stoull(str, nullptr, radix);
                } catch(...){
                    type = TokenType::Unknown;
                    val = UINT64_MAX;
                }
            }
            
            return Token(type, startSpan, val);
        }

        if(std::isalpha(c) || c == '_'){
            std::stringstream ss;
            while(M_IDX_IS_VALID && (std::isalnum(CUR_CHAR) || CUR_CHAR == '_')){
                ss << CUR_CHAR;
                Next();
            }
            std::string ident = ss.str();

            if(isRegister(ident)) return Token(TokenType::Register, startSpan, parseRegisterIdx(ident));
            else if(
                ident == "mov" ||
                ident == "ret"
            ) return Token(TokenType::Keyword, startSpan, ident);
            else return Token(TokenType::Identifier, startSpan, ident);
        }

        switch(c) {
            case ':': Next(); return Token(TokenType::Colon, startSpan);
            case ',': Next(); return Token(TokenType::Comma, startSpan);

            default: break;
        }

        return Token(TokenType::Unknown, startSpan);
    }

}