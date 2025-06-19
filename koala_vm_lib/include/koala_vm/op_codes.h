#ifndef KOALA_VM_OP_CODES_H
#define KOALA_VM_OP_CODES_H

#include <cstddef>

enum OpCode: size_t{
    OP_ENTRY = 0x00,
    OP_RET = 0x01,
    OP_END = 0x02,
    OP_PUSH = 0x03,
    M_CONST = 0x04, //indicates that next byte is a constant

    //arithmetic
    OP_INC = 0x05,
    OP_DEC = 0x06,
    OP_ADD = 0x07,
    OP_SUB = 0x08,
    OP_MUL = 0x09,
    OP_DIV = 0x0A,
};

OpCode get_max_op_code();

#endif