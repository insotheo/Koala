#include "parser/lexer.h"

#include <iostream>
#include <sstream>
#include <cctype>

#define CURRENT_CHAR m_input[m_index]
std::vector<Token> Lexer::parse(){
    std::vector<Token> tokens;
    Token token;

    while (m_index < m_input.size())
    {
        if(std::isalpha(CURRENT_CHAR)){
            next();
            continue;
        }

        //DBG
        panic("Unexpected token!");
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
    oss << "Error on (" << m_line << ";" << (m_index - line_start + 1) << "): " << msg << " : " << err_line;
    m_errors.push_back(oss.str());
    m_is_success = false;
}