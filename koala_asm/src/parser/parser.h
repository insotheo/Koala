#ifndef KOALA_ASM_PARSER_H
#define KOALA_ASM_PARSER_H

#include <iostream>
#include <vector>

#include "parser/token.h"
#include "parser/instruction.h"

class Parser{
public:
    Parser(std::vector<Token>& tokens): m_tokens(tokens), m_index(0), m_is_success(true)
    {}

    std::vector<CodeBlock> parse();
    inline bool is_success() const {return m_is_success;};
private:
    std::vector<Token>& m_tokens;
    size_t m_index;
    bool m_is_success;

    FullCodeBlock parse_block();
    void expand(std::vector<CodeBlock>& glob, FullCodeBlock& full);

    void next();
    bool expect(const TokenType type);
    void fatal(const TokenType type);
};

#endif