#pragma once

#include <vector>
#include <opcodes.h>

namespace koalac{

    enum class ArgType{
        Register,
        Imm16,
        Imm64,
    };

    struct InstrVariant {
        OpCode Op;
        std::vector<ArgType> Format;
    };

    using InstrDescriptor = std::vector<InstrVariant>;
}