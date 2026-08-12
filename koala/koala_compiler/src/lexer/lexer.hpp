#pragma once

#include "lexer/token.hpp"
#include <string>

namespace koalac{

    class Lexer {
    public:
        Lexer(const std::string& source)
        : m_Content(source), m_Idx(0), m_ContentLength(m_Content.size()), m_CurLine(1), m_CurColumn(1)
        {}

        ~Lexer() = default;

        Token NextToken();
    private:
        void SkipWhitespacesAndComments();
        void Next();
        
        std::string m_Content;
        size_t m_Idx;
        size_t m_ContentLength;
        size_t m_CurLine;
        size_t m_CurColumn;
    };

}