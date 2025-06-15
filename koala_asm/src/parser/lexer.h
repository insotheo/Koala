#ifndef KOALA_ASM_LEXER_H
#define KOALA_ASM_LEXER_H

#include "parser/token.h"
#include <vector>

class Lexer{
public:
    Lexer(const std::string& input) : m_index(0), m_input(input + ' '), m_is_success(true)
    {}

    std::vector<Token> parse();
    void print_errors();
    inline bool is_success() const {return m_is_success;}
    inline void clear_errors_list() {m_errors.clear();}
private:
    size_t m_index = 0;
    size_t m_line = 1;

    std::string m_input;
    std::vector<std::string> m_errors;
    bool m_is_success;

    void next();
    void panic(const std::string& msg);
};

#endif