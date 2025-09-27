#ifndef KOALA_BYTE_OP_CODES_H
#define KOALA_BYTE_OP_CODES_H

namespace KoalaByte{
    enum class OpCode: unsigned char{
        RET = 0x0,
        PUSH = 0x1,

        ADD = 0x2,
        SUB = 0x3,
        MUL = 0x4,
        DIV = 0x5,
        ROD = 0x6, //Reminder On Division
    };
}

#endif