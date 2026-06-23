#ifndef KOALA_VM_OP_CODES_H
#define KOALA_VM_OP_CODES_H

#include <cstdint>

namespace Koala{
    enum OpCode : uint8_t{
        RET = 0,

        MOV_IMM,
        LOAD_CONST,

        ADD,
        SUB,
        MUL,
        DIV_S,
        DIV_U,
        MOD_S,
        MOD_U,
    };
}

//ASM for dbg

//OpCode(8) | regA(8) | regB(8) | regC(8)
#define KOALA_ASM_R(op, rA, rB, rC)\
    ((static_cast<uint32_t>(Koala::OpCode::op) << 24) | (static_cast<uint32_t>(rA) << 16) | (static_cast<uint32_t>(rB) << 8) | static_cast<uint32_t>(rC))

//OpCode(8) | regA(8) | Const(16)
#define KOALA_ASM_I(op, rA, imm16)\
    ((static_cast<uint32_t>(Koala::OpCode::op) << 24) | (static_cast<uint32_t>(rA) << 16) | (static_cast<uint16_t>(imm16)))

///////////////


#endif