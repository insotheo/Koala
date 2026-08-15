#pragma once

#include "ir.hpp"
#include <vector>
#include <cstdint>

namespace koalac{
    using Bytecode = std::vector<uint8_t>;

    Bytecode translateToBytecode(const IRProgram& program);
}