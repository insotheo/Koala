#ifndef KOALA_ASM_BYTE_DATA_H
#define KOALA_ASM_BYTE_DATA_H

#include <vector>
#include <variant>
#include <string>
#include <unordered_map>

struct Block
{
    size_t begin;
    size_t end;
};


using OP_ARG_TYPE = std::variant<int>;
struct ByteData{
    std::vector<size_t> code;
    std::unordered_map<std::string, OP_ARG_TYPE> constants;
    std::unordered_map<std::string, Block> blocks;
};

#endif