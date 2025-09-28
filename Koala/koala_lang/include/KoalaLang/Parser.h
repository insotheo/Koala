#ifndef KOALA_LANG_PARSER_H
#define KOALA_LANG_PARSER_H

#include "Kernel.h"
#include "Token.h"
#include "Lexer.h"
#include "ASTNodes.h"

namespace KoalaLang{
    class KOALA_LANG_API Parser{
    public:
        explicit Parser(Lexer& lexer) : m_tokens(lexer.GetTokens()), m_source(lexer.GetSource()), m_code(std::vector<SHARED_PTR_T(ASTNode)>()), m_idx(0)
        {}

        void Parse(const std::string& globalModuleName);

        inline ASTCodeBlock& GetAST() { return m_code; }
    private:
        std::vector<std::string> m_modulesStack;
        std::vector<Token>& m_tokens;
        const std::string& m_source;
        ASTCodeBlock m_code;
        size_t m_idx;

        void Next();
        void FatalNext(const TokenType type);
        void FatalThenNext(const TokenType type);
        void Panic(const std::string& msg);

        std::string MakeQualifiedName(const std::string& name);

        SHARED_PTR_T(ASTCodeBlock) ParseCodeBlock(bool shift = true);
        SHARED_PTR_T(ASTFunction) ParseFunctionDecl();
        SHARED_PTR_T(ASTModule) ParseModuleDecl();
        SHARED_PTR_T(ASTNode) ParseExpression();
        SHARED_PTR_T(ASTNode) ParseTerm();
        SHARED_PTR_T(ASTNode) ParseFactor();
    };
}

#endif