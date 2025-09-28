#ifndef KOALA_LANG_TRANSLATOR_H
#define KOALA_LANG_TRANSLATOR_H

#include "Kernel.h"
#include "Parser.h"
#include "ASTNodes.h"
#include "KoalaByte/ByteData.h"
#include <vector>
#include <unordered_map>
#include "KoalaByte/Bytecode.h"

namespace KoalaLang{
    class KOALA_LANG_API Translator{
    public:
        explicit Translator(Parser& parser) : m_code(parser.GetAST())
        {}

        void Translate();

        inline KoalaByte::Bytecode GetBytecode() {
            std::vector<size_t> m_regionPointers;

            for (const auto& regPtr : m_regions_ptrs) {
                m_regionPointers.push_back(regPtr.second);
            }

            return KoalaByte::Bytecode(m_codes, m_constants, m_regionPointers);
        }
    private:
        ASTCodeBlock& m_code;

        std::vector<size_t> m_codes;
        std::vector<ByteData_t> m_constants;
        std::unordered_map<std::string, size_t> m_regions_ptrs;

        void VisitCodeBlock(ASTCodeBlock& block);
        void VisitExpression(ASTNode& node);
        size_t VisitConstant(ASTNode& constant);
    };
}

#endif