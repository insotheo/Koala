#ifndef KOALA_ASM_TOKEN_H
#define KOALA_ASM_TOKEN_H

#include <string>

enum TokenType{
    Keyword, Identifier, Number,

    Colon,
};

struct Token
{
    TokenType type;
    const std::string value;
};


#endif