#ifndef KOALA_VM_OP_CODES_H
#define KOALA_VM_OP_CODES_H

#define OP_CODE_COUNT 19
enum OpCode: size_t{
    OP_RET      = 0x00,
    OP_END      = 0x01,
    OP_PUSH     = 0x02,
    OP_POP      = 0x03,
    OP_POP_N    = 0x04,
    OP_JMP      = 0x05,
    OP_JEZ      = 0x06,
    OP_JNZ      = 0x07,
    OP_DUP      = 0x08,
    
    //arithmetic
    OP_INC = 0x09,
    OP_DEC = 0x0A,
    OP_ADD = 0x0B,
    OP_SUB = 0x0C,
    OP_MUL = 0x0D,
    OP_DIV = 0x0E,

    //logical
    OP_AND = 0x0F,
    OP_OR  = 0x10,
    OP_XOR = 0x11,
    OP_NOT = 0x12,

    //for parser
    PC_MARK = 0x13,
};

#endif