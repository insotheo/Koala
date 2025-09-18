#ifndef KOALA_LANG_PARSER_H
#define KOALA_LANG_PARSER_H

#include "Kernel.h"
#include "Token.h"
#include "Lexer.h"
#include "ASTNodes.h"

namespace KoalaLang{
    class KOALA_LANG_API Parser{
    public:
        Parser(Lexer& lexer) : m_tokens(lexer.GetTokens()), m_source(lexer.GetSource()), m_code(std::vector<SHARED_PTR_T(ASTNode)>()), m_idx(0)
        {}

        void Parse();

        inline ASTCodeBlock& GetAST() { return m_code; }
    private:
        std::vector<Token>& m_tokens;
        const std::string& m_source;
        ASTCodeBlock m_code;
        size_t m_idx;

        void Next();
        void FatalNext(TokenType type);
        void Panic(const std::string& msg);

        SHARED_PTR_T(ASTCodeBlock) ParseCodeBlock();
        void ParseFunctionDecl();
    };
}

#endif