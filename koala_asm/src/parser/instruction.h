#ifndef KOALA_ASM_INSTRUCTION_H
#define KOALA_ASM_INSTRUCTION_H


#include <variant>
#include <vector>
#include <string>

using OP_ARG_TYPE = std::variant<int>;

struct Instruction{
    size_t op_code;
    std::vector<OP_ARG_TYPE> operands;
};

struct CodeBlock{
    std::string label;
    std::vector<Instruction> block_instructions;
};

struct FullCodeBlock {
    std::string label;
    std::vector<Instruction> block_instructions;
    std::vector<FullCodeBlock> inside_blocks;
};

#endif