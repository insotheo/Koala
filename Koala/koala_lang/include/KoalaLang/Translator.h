#ifndef KOALA_LANG_TRANSLATOR_H
#define KOALA_LANG_TRANSLATOR_H

#include "Kernel.h"
#include "Parser.h"
#include "ASTNodes.h"
#include "KoalaByte/ByteData.h"
#include <vector>
#include <unordered_map>
#include <set>

namespace KoalaLang{
    class KOALA_LANG_API Translator{
    public:
        Translator(Parser& parser) : m_code(parser.GetAST())
        {}

        void Translate();
    private:
        ASTCodeBlock& m_code;

        std::vector<size_t> m_codes;
        std::set<ByteData_t> m_constants;
        std::unordered_map<std::string, size_t> m_regions_ptrs;

        void VisitCodeBlock(ASTCodeBlock& block);
        void VisitExpression(SHARED_PTR_T(ASTNode) node);
        int VisitConstant(SHARED_PTR_T(ASTNode) constant);
    };
}

#endif