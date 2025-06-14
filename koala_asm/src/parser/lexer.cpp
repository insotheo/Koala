#include "parser/lexer.h"

#include <iostream>
#include <sstream>
#include <cctype>

#define CURRENT_CHAR m_input[m_index]
std::vector<Token> Lexer::parse(){
    std::vector<Token> tokens;

    while (m_index < m_input.size())
    {
        if(std::isspace(CURRENT_CHAR)){
            next();
            continue;
        }

        if(std::isdigit(CURRENT_CHAR)){
            std::string number;
            while (m_index < m_input.size() && std::isdigit(CURRENT_CHAR))
            {
                number += CURRENT_CHAR;
                next();
            }
            tokens.push_back(Token(TokenType::Number, number));
            continue;
        }

        if(std::isalpha(CURRENT_CHAR) || CURRENT_CHAR == '_'){
            std::string identifier;
            while (m_index < m_input.size() && (std::isalpha(CURRENT_CHAR) || CURRENT_CHAR == '_'))
            {
                identifier += CURRENT_CHAR;
                next();
            }

            if(identifier == "RET") { tokens.push_back(Token(TokenType::Keyword, "RET")); continue; }

            tokens.push_back(Token(TokenType::Identifier, identifier));
            continue;
        }

        switch(CURRENT_CHAR){
            case ':': tokens.push_back(Token(TokenType::Colon, "")); next(); continue;

            case ';':{
                //comment
                while(m_index < m_input.size() && CURRENT_CHAR != '\n'){
                    next();
                    continue;
                }
                next();
                continue;
            }
        }

        //DBG
        std::ostringstream oss;
        oss << "Unrecognized token('" << CURRENT_CHAR << "')!";
        panic(oss.str());
        next();
    }

    return tokens;
}

void Lexer::next(){
    m_index += 1;
    if(m_index < m_input.size()){
        if(m_input[m_index] == '\n'){
            m_line += 1;
        }
    }
}

void Lexer::print_errors(){
    for(const std::string& err : m_errors){
        std::cout << err << "\n";
    }
    m_errors.clear();
}

void Lexer::panic(const std::string& msg){
    size_t line_start = m_index;
    while(line_start > 0 && m_input[line_start - 1] != '\n'){
        --line_start;
    }

    size_t line_end = m_index;
    while(line_end < m_input.size() && m_input[line_end] != '\n'){
        ++line_end;
    }

    std::string err_line = m_input.substr(line_start, line_end - line_start);

    std::ostringstream oss;
    oss << "Error on (" << m_line << ";" << (m_index - line_start + 1) << "): " << msg << ": " << err_line;
    m_errors.push_back(oss.str());
    m_is_success = false;
}