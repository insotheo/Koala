#ifndef KOALA_BYTE_BYTECODE_H
#define KOALA_BYTE_BYTECODE_H

#include "Kernel.h"
#include "KoalaByte/ByteData.h"
#include <vector>

namespace KoalaByte{
    class KOALA_LANG_API Bytecode{
    public:
        Bytecode(std::vector<size_t> code, std::vector<ByteData_t> constants, std::vector<size_t> m_regions)
        : m_code(std::move(code)), m_constants(std::move(constants)), m_regionsPtrs(std::move(m_regions))
        {}

        inline const std::vector<size_t>& GetCode() const { return m_code; }
        inline const std::vector<size_t>& GetRegions() const { return m_regionsPtrs; }
        inline const std::vector<ByteData_t>& GetConstants() const { return m_constants; }
    private:
        std::vector<size_t> m_code;
        std::vector<size_t> m_regionsPtrs;
        std::vector<ByteData_t> m_constants;
    };
}

#endif