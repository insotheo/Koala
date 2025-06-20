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
            while (m_index < m_input.size() && (std::isalpha(CURRENT_CHAR) || CURRENT_CHAR == '_' || std::isdigit(CURRENT_CHAR)))
            {
                identifier += CURRENT_CHAR;
                next();
            }

            if (identifier == "INC")    { tokens.push_back(Token(TokenType::Keyword, "INC")); continue; }
            if (identifier == "DEC")    { tokens.push_back(Token(TokenType::Keyword, "DEC")); continue; }
            if (identifier == "ADD")    { tokens.push_back(Token(TokenType::Keyword, "ADD")); continue; }
            if (identifier == "SUB")    { tokens.push_back(Token(TokenType::Keyword, "SUB")); continue; }
            if (identifier == "MUL")    { tokens.push_back(Token(TokenType::Keyword, "MUL")); continue; }
            if (identifier == "DIV")    { tokens.push_back(Token(TokenType::Keyword, "DIV")); continue; }

            if (identifier == "AND")    { tokens.push_back(Token(TokenType::Keyword, "AND")); continue; }
            if (identifier == "OR")     { tokens.push_back(Token(TokenType::Keyword, "OR")); continue; }
            if (identifier == "XOR")    { tokens.push_back(Token(TokenType::Keyword, "XOR")); continue; }
            if (identifier == "NOT")    { tokens.push_back(Token(TokenType::Keyword, "NOT")); continue; }

            if (identifier == "RET")    { tokens.push_back(Token(TokenType::Keyword, "RET")); continue; }
            if (identifier == "END")    { tokens.push_back(Token(TokenType::Keyword, "END")); continue; }
            if (identifier == "JMP")    { tokens.push_back(Token(TokenType::Keyword, "JMP")); continue; }
            if (identifier == "JEZ")    { tokens.push_back(Token(TokenType::Keyword, "JEZ")); continue; }
            if (identifier == "JNZ")    { tokens.push_back(Token(TokenType::Keyword, "JNZ")); continue; }
            if (identifier == "PUSH")   { tokens.push_back(Token(TokenType::Keyword, "PUSH")); continue; }
            if (identifier == "POP")    { tokens.push_back(Token(TokenType::Keyword, "POP")); continue; }
            if (identifier == "POP_N")  { tokens.push_back(Token(TokenType::Keyword, "POP_N")); continue; }
            if (identifier == "MARK")   { tokens.push_back(Token(TokenType::Keyword, "MARK")); continue; }

            tokens.push_back(Token(TokenType::Identifier, identifier));
            continue;
        }

        switch(CURRENT_CHAR){
            case ':': tokens.push_back(Token(TokenType::Colon, "")); next(); continue;

            case ';':{
                //comment
                while(m_index < m_input.size() && CURRENT_CHAR != '\n'){
                    next();
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
    tokens.push_back(Token(TokenType::EndOfFile, ""));
    
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
    oss << "Error on (" << static_cast<int>(m_line) << ";" << static_cast<int>(m_index - line_start + 1) << "): " << msg << ": " << err_line;
    m_errors.push_back(oss.str());
    m_is_success = false;
}