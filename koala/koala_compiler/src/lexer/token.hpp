#pragma once

#include <variant>
#include <string>
#include <cstdint>

namespace koalac {
    enum class TokenType{
        Unknown,

        Identifier, //string
        Keyword, //string
        Register, //uint64_t
        Number, //uint64_t

        Colon, Comma, //monostate

        EndOfFile, //whatever
    };

    struct Span {
        size_t Line;
        size_t Column;
    };

    struct Token{
        using TokenValue = std::variant<std::monostate, uint64_t, std::string>;

        TokenType Type;
        TokenValue Val;
        Span Span;

        Token(TokenType type, struct Span span, TokenValue val = std::monostate())
        : Type(type), Val(val), Span(span)
        {}

        ~Token() = default;
    };
}