#ifndef KOALA_VM_OP_CODES_H
#define KOALA_VM_OP_CODES_H

#include <cstddef>

enum OpCode: size_t{
    OP_ENTRY = 0x00,
    OP_RET = 0x01,
    OP_END = 0x02,
    M_CONST = 0x03, //indicates that next byte is a constant
};

OpCode get_max_op_code();

#endif