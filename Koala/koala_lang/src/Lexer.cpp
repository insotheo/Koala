#include "KoalaLang/Lexer.h"

namespace KoalaLang{

    #define INPUT_CURRENT_CHAR m_input[m_idx]
    void Lexer::Tokenize(){
        while(m_idx < m_input.length()){
            Next();
        }

        m_tokens.push_back(Token(TokenType::EndOfFile, "EOF", m_col, m_line));
        m_success = true;
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