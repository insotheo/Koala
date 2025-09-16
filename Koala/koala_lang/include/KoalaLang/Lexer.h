#ifndef KOALA_LANG_LEXER_H
#define KOALA_LANG_LEXER_H

#include "Kernel.h"
#include "Token.h"
#include <vector>
#include <string>

namespace KoalaLang{
    class KOALA_LANG_API Lexer{
    public:
        Lexer(const std::string& source)
        : m_input(source), m_idx(0), m_col(1), m_line(1), m_success(false)
        {}

        void Tokenize();

        inline std::vector<Token>& GetTokens() { return m_tokens; }
        inline const bool IsSuccess() const { return m_success; }
    private:
        const std::string& m_input;
        size_t m_idx, m_col, m_line;
        std::vector<Token> m_tokens;
        bool m_success;

        void Next();
    };
}

#endif