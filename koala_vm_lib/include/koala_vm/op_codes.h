#ifndef KOALA_VM_OP_CODES_H
#define KOALA_VM_OP_CODES_H

#include <cstddef>

enum OpCode: size_t{
    OP_ENTRY    = 0x00,
    OP_RET      = 0x01,
    OP_END      = 0x02,
    OP_PUSH     = 0x03,
    OP_POP      = 0x04,
    OP_POP_N    = 0x05,
    M_CONST     = 0x06, //indicates that next byte is a constant

    //arithmetic
    OP_INC = 0x07,
    OP_DEC = 0x08,
    OP_ADD = 0x09,
    OP_SUB = 0x0A,
    OP_MUL = 0x0B,
    OP_DIV = 0x0C,

    //logical
    OP_AND = 0x0D,
    OP_OR  = 0x0E,
    OP_XOR = 0x0F,
    OP_NOT = 0x10,
};

OpCode get_max_op_code();

#endif