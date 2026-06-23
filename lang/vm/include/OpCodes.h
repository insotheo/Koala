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




//INSTRUCTIONS

#define KOALA_ASM_RET() Koala::OpCode::RET

#define KOALA_ASM_MOV_IMM(target_reg, value) KOALA_ASM_I(MOV_IMM, target_reg, value)
#define KOALA_ASM_PUSH_CONST(target_reg, value) KOALA_ASM_I(LOAD_CONST, target_reg, value)

#define KOALA_ASM_ADD(target_reg, src1, src2) KOALA_ASM_R(ADD, target_reg, src1, src2)
#define KOALA_ASM_SUB(target_reg, src1, src2) KOALA_ASM_R(SUB, target_reg, src1, src2)
#define KOALA_ASM_MUL(target_reg, src1, src2) KOALA_ASM_R(MUL, target_reg, src1, src2)
#define KOALA_ASM_DIV_S(target_reg, src1, src2) KOALA_ASM_R(DIV_S, target_reg, src1, src2)
#define KOALA_ASM_DIV_U(target_reg, src1, src2) KOALA_ASM_R(DIV_U, target_reg, src1, src2)
#define KOALA_ASM_MOD_S(target_reg, src1, src2) KOALA_ASM_R(MOD_S, target_reg, src1, src2)
#define KOALA_ASM_MOD_U(target_reg, src1, src2) KOALA_ASM_R(MOD_U, target_reg, src1, src2)

///////////////


#endif