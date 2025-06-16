#ifndef KOALA_ASM_TOKEN_H
#define KOALA_ASM_TOKEN_H

#include <string>

enum TokenType{
    Keyword, Identifier, Number, EndOfFile,

    Colon,
};

struct Token
{
    Token(const TokenType& t, const std::string& val)
    : type(t), value(val)
    {}

    TokenType type;
    const std::string value;
};


#endif