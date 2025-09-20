#ifndef KOALA_LANG_TOKEN_H
#define KOALA_LANG_TOKEN_H

#include "Kernel.h"
#include <string>

namespace KoalaLang{
    enum TokenType{
        EndOfFile, Unknown,
        
        Identifier, Number, Float, Keyword,

        LParen, RParen, LBrace, RBrace, Semicolon, Colon
    };

    struct KOALA_LANG_API Token
    {
        TokenType type;
        std::string value;
        size_t column, line;

        Token(const TokenType type, const std::string& value, const size_t col, const size_t line) : type(type), value(value), column(col), line(line)
        {}
    };
    
}

#endif