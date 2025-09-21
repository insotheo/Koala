#ifndef KOALA_BYTE_OP_CODES_H
#define KOALA_BYTE_OP_CODES_H

namespace KoalaByte{
    enum class OpCode: unsigned char{
        RET = 0x0,
        PUSH = 0x1
    };
}

#endif