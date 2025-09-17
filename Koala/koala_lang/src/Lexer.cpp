#include "KoalaLang/Lexer.h"

namespace KoalaLang{

    #define TOKEN(type, value) Token(TokenType::type, value, m_col, m_line)
    #define INPUT_CURRENT_CHAR m_input[m_idx]

    void Lexer::Tokenize(){
        while(m_idx < m_input.length()){
            if(std::isspace(INPUT_CURRENT_CHAR) || std::iscntrl(INPUT_CURRENT_CHAR)){
                Next();
                continue;
            }

            if(std::isdigit(INPUT_CURRENT_CHAR)){
                std::string number;
                bool hasPoint = false;
                bool hasPointMoreThanOneTime = false;

                while(m_idx < m_input.length() &&
                (std::isdigit(INPUT_CURRENT_CHAR) || INPUT_CURRENT_CHAR == '.')){
                    if(INPUT_CURRENT_CHAR == '.'){
                        if(hasPoint) hasPointMoreThanOneTime = true;
                        else hasPoint = true;
                    }
                    number += INPUT_CURRENT_CHAR;
                    Next();
                }
                
                if(!hasPointMoreThanOneTime) m_tokens.push_back(TOKEN(Number, number));
                else m_tokens.push_back(TOKEN(Unknown, number));

                continue;
            }

            if(std::isalpha(INPUT_CURRENT_CHAR) || INPUT_CURRENT_CHAR == '_'){
                std::string identifier;
                while(m_idx < m_input.length() && 
                (std::isalpha(INPUT_CURRENT_CHAR) || std::isdigit(INPUT_CURRENT_CHAR) || INPUT_CURRENT_CHAR == '_')){
                    identifier += INPUT_CURRENT_CHAR;
                    Next();
                }
                
                if(identifier == "ret") { m_tokens.push_back(TOKEN(Keyword, "ret")); continue; }

                m_tokens.push_back(TOKEN(Identifier, identifier));
                continue;
            }

            switch (INPUT_CURRENT_CHAR)
            {
                case '(': {m_tokens.push_back(TOKEN(LParen, "")); Next(); continue;}
                case ')': {m_tokens.push_back(TOKEN(RParen, "")); Next(); continue;}
                
                case '{': {m_tokens.push_back(TOKEN(LBrace, "")); Next(); continue;}
                case '}': {m_tokens.push_back(TOKEN(RBrace, "")); Next(); continue;}

                case ';': {m_tokens.push_back(TOKEN(Semicolon, "")); Next(); continue;}
            }

            m_tokens.push_back(TOKEN(Unknown, std::to_string(INPUT_CURRENT_CHAR)));
            Next();
        }

        m_tokens.push_back(TOKEN(EndOfFile, "EOF"));
    }

    void Lexer::Next(){
        m_idx += 1;
        m_col += 1;
        if(m_idx < m_input.length()){
            if(INPUT_CURRENT_CHAR == '\n'){
                m_col = 1;
                m_line += 1;
            }
        }
    }

}