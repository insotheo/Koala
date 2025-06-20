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
    OP_JMP      = 0x06,
    OP_JEZ      = 0x07,
    OP_JNZ      = 0x08,
    M_CONST     = 0x09, //indicates that next byte is a constant
    M_ID        = 0x0A, //indicates that next byte is an identifier

    //arithmetic
    OP_INC = 0x0B,
    OP_DEC = 0x0C,
    OP_ADD = 0x0D,
    OP_SUB = 0x0E,
    OP_MUL = 0x0F,
    OP_DIV = 0x10,

    //logical
    OP_AND = 0x11,
    OP_OR  = 0x12,
    OP_XOR = 0x13,
    OP_NOT = 0x14,

    //for parser
    PC_MARK = 0x15,
};

#endif