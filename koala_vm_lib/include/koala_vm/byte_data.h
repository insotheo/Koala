#ifndef KOALA_ASM_BYTE_DATA_H
#define KOALA_ASM_BYTE_DATA_H

#include <vector>
#include <variant>
#include <string>

struct Block
{
    size_t begin;
    size_t end;
};

using Value = std::variant<int, std::string>;
using Bytecode = std::vector<size_t>;
struct ProgramData{
    Bytecode code;

    std::vector<Value> constants;
    std::vector<Block> blocks;
};

#endif