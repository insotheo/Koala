#pragma once

#include "lexer/lexer.hpp"
#include "lexer/token.hpp"
#include "ir.hpp"
#include "parser/descriptor.hpp"
#include <vector>
#include <unordered_map>

namespace koalac{

    struct ParserError{
        std::string Msg;
        struct Span Span;

        ParserError(const std::string& msg, struct Span span)
        : Msg(msg), Span(span)
        {}

        ~ParserError() = default;
    };

    struct ParserArg{
        ArgType Type;
        IRArg Val;

        ParserArg(ArgType type, IRArg val)
        : Type(type), Val(val)
        {}

        ~ParserArg() = default;
    };

    class Parser{
    public:
        Parser(Lexer* lexer)
        : m_Lexer(lexer), m_Cur(m_Lexer->NextToken()), m_Next(m_Lexer->NextToken())
        {}

        IRProgram MakeProgram();
        inline bool IsSuccess() const { return m_Errors.size() == 0; }
        void PrintErrors();
        
    private:
        Lexer* m_Lexer;
        Token m_Cur;
        Token m_Next;
        std::vector<ParserError> m_Errors;
        std::unordered_map<std::string, Span> m_Labels;

        void ParseLabel(IRNodes* nodes);
        void ParseInstruction(IRNodes* nodes);

        void Next();
        void Panic(const std::string& msg, struct Span span);
        void Sync();
    };

}