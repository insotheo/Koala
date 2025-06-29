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
            while (m_index < m_input.size() && (std::isdigit(CURRENT_CHAR) || CURRENT_CHAR == '_'))
            {
                if(CURRENT_CHAR == '_'){
                    next();
                    continue;
                }
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

            if (identifier == "INC" || identifier == "inc")    { tokens.push_back(Token(TokenType::Keyword, "INC")); continue; }
            if (identifier == "DEC" || identifier == "dec")    { tokens.push_back(Token(TokenType::Keyword, "DEC")); continue; }
            if (identifier == "ADD" || identifier == "add")    { tokens.push_back(Token(TokenType::Keyword, "ADD")); continue; }
            if (identifier == "SUB" || identifier == "sub")    { tokens.push_back(Token(TokenType::Keyword, "SUB")); continue; }
            if (identifier == "MUL" || identifier == "mul")    { tokens.push_back(Token(TokenType::Keyword, "MUL")); continue; }
            if (identifier == "DIV" || identifier == "div")    { tokens.push_back(Token(TokenType::Keyword, "DIV")); continue; }

            if (identifier == "AND" || identifier == "and")    { tokens.push_back(Token(TokenType::Keyword, "AND")); continue; }
            if (identifier == "OR" || identifier == "or")      { tokens.push_back(Token(TokenType::Keyword, "OR")); continue; }
            if (identifier == "XOR" || identifier == "xor")    { tokens.push_back(Token(TokenType::Keyword, "XOR")); continue; }
            if (identifier == "NOT" || identifier == "not")    { tokens.push_back(Token(TokenType::Keyword, "NOT")); continue; }

            if (identifier == "RET" || identifier == "ret")    { tokens.push_back(Token(TokenType::Keyword, "RET")); continue; }
            if (identifier == "END" || identifier == "end")    { tokens.push_back(Token(TokenType::Keyword, "END")); continue; }
            if (identifier == "CALL" || identifier == "call")  { tokens.push_back(Token(TokenType::Keyword, "CALL")); continue; }
            if (identifier == "JMP" || identifier == "jmp")    { tokens.push_back(Token(TokenType::Keyword, "JMP")); continue; }
            if (identifier == "JEZ" || identifier == "jez")    { tokens.push_back(Token(TokenType::Keyword, "JEZ")); continue; }
            if (identifier == "JNZ" || identifier == "jnz")    { tokens.push_back(Token(TokenType::Keyword, "JNZ")); continue; }
            if (identifier == "PUSH" || identifier == "push")  { tokens.push_back(Token(TokenType::Keyword, "PUSH")); continue; }
            if (identifier == "POP" || identifier == "pop")    { tokens.push_back(Token(TokenType::Keyword, "POP")); continue; }
            if (identifier == "DUP" || identifier == "dup")    { tokens.push_back(Token(TokenType::Keyword, "DUP")); continue; }
            if (identifier == "POP_N" || identifier == "pop_n"){ tokens.push_back(Token(TokenType::Keyword, "POP_N")); continue; }
            if (identifier == "MARK" || identifier == "mark")  { tokens.push_back(Token(TokenType::Keyword, "MARK")); continue; }

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