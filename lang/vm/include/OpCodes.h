#ifndef KOALA_VM_OP_CODES_H
#define KOALA_VM_OP_CODES_H

#include <cstdint>

namespace Koala{
    enum OpCode : uint8_t{
        RET = 0,

        MOV,
        MOV_IMM,
        LOAD_CONST,

        AND,
        OR,
        XOR,
        NOT,
        SHL,
        SHR_S,
        SHR_U,

        ADD,
        SUB,
        MUL,
        DIV_S,
        DIV_U,
        MOD_S,
        MOD_U,

        ADD_F,
        SUB_F, 
        MUL_F,
        DIV_F,

        CONV_SI2F,
        CONV_UI2F,
        CONV_F2SI,
        CONV_F2UI,

        CMP_EQ,
        CMP_NEQ,
        CMP_LT_S,
        CMP_LE_S,
        CMP_LT_U,
        CMP_LE_U,
        CMP_LT_F,
        CMP_LE_F,

        JMP,
        JEZ,
        JNZ,
    };
}

//ASM for dbg

//OpCode(8) | regA(8) | regB(8) | regC(8)
#define KOALA_ASM_R(op, rA, rB, rC)\
    ((static_cast<uint32_t>(Koala::OpCode::op) << 24) | (static_cast<uint32_t>(rA) << 16) | (static_cast<uint32_t>(rB) << 8) | static_cast<uint32_t>(rC))

//OpCode(8) | regA(8) | Const(16)
#define KOALA_ASM_I(op, rA, imm16)\
    ((static_cast<uint32_t>(Koala::OpCode::op) << 24) | (static_cast<uint32_t>(rA) << 16) | (static_cast<uint16_t>(imm16)))

//OpCode(8) | Const(24)
#define KOALA_ASM_IX24(op, imm24)\
    ((static_cast<int32_t>(Koala::OpCode::op) << 24) | imm24)


//INSTRUCTIONS

#define KOALA_ASM_RET() Koala::OpCode::RET

#define KOALA_ASM_MOV(dest_reg, src) KOALA_ASM_R(MOV, dest_reg, src, 0)
#define KOALA_ASM_MOV_IMM(dest_reg, value) KOALA_ASM_I(MOV_IMM, dest_reg, value)
#define KOALA_ASM_LOAD_CONST(dest_reg, value) KOALA_ASM_I(LOAD_CONST, dest_reg, value)

#define KOALA_ASM_AND(dest_reg, src1, src2) KOALA_ASM_R(AND, dest_reg, src1, src2)
#define KOALA_ASM_OR(dest_reg, src1, src2) KOALA_ASM_R(OR, dest_reg, src1, src2)
#define KOALA_ASM_XOR(dest_reg, src1, src2) KOALA_ASM_R(XOR, dest_reg, src1, src2)
#define KOALA_ASM_NOT(dest_reg, src1) KOALA_ASM_R(NOT, dest_reg, src1, 0)
#define KOALA_ASM_SHL(dest_reg, src1, src2) KOALA_ASM_R(SHL, dest_reg, src1, src2)
#define KOALA_ASM_SHR_S(dest_reg, src1, src2) KOALA_ASM_R(SHR_S, dest_reg, src1, src2)
#define KOALA_ASM_SHR_U(dest_reg, src1, src2) KOALA_ASM_R(SHR_U, dest_reg, src1, src2)

#define KOALA_ASM_ADD(dest_reg, src1, src2) KOALA_ASM_R(ADD, dest_reg, src1, src2)
#define KOALA_ASM_SUB(dest_reg, src1, src2) KOALA_ASM_R(SUB, dest_reg, src1, src2)
#define KOALA_ASM_MUL(dest_reg, src1, src2) KOALA_ASM_R(MUL, dest_reg, src1, src2)
#define KOALA_ASM_DIV_S(dest_reg, src1, src2) KOALA_ASM_R(DIV_S, dest_reg, src1, src2)
#define KOALA_ASM_DIV_U(dest_reg, src1, src2) KOALA_ASM_R(DIV_U, dest_reg, src1, src2)
#define KOALA_ASM_MOD_S(dest_reg, src1, src2) KOALA_ASM_R(MOD_S, dest_reg, src1, src2)
#define KOALA_ASM_MOD_U(dest_reg, src1, src2) KOALA_ASM_R(MOD_U, dest_reg, src1, src2)

#define KOALA_ASM_ADD_F(dest_reg, src1, src2) KOALA_ASM_R(ADD_F, dest_reg, src1, src2)
#define KOALA_ASM_SUB_F(dest_reg, src1, src2) KOALA_ASM_R(SUB_F, dest_reg, src1, src2)
#define KOALA_ASM_MUL_F(dest_reg, src1, src2) KOALA_ASM_R(MUL_F, dest_reg, src1, src2)
#define KOALA_ASM_DIV_F(dest_reg, src1, src2) KOALA_ASM_R(DIV_F, dest_reg, src1, src2)

#define KOALA_ASM_CONV_SI2F(dest_reg, src) KOALA_ASM_R(CONV_SI2F, dest_reg, src, 0xFF)
#define KOALA_ASM_CONV_UI2F(dest_reg, src) KOALA_ASM_R(CONV_UI2F, dest_reg, src, 0xFF)
#define KOALA_ASM_CONV_F2SI(dest_reg, src) KOALA_ASM_R(CONV_F2SI, dest_reg, src, 0xFF)
#define KOALA_ASM_CONV_F2UI(dest_reg, src) KOALA_ASM_R(CONV_F2UI, dest_reg, src, 0xFF)

#define KOALA_ASM_CMP_EQ(src1, src2) KOALA_ASM_R(CMP_EQ, 0, src1, src2)
#define KOALA_ASM_CMP_NEQ(src1, src2) KOALA_ASM_R(CMP_NEQ, 0, src1, src2)
#define KOALA_ASM_CMP_LT_S(src1, src2) KOALA_ASM_R(CMP_LT_S, 0, src1, src2)
#define KOALA_ASM_CMP_LE_S(src1, src2) KOALA_ASM_R(CMP_LE_S, 0, src1, src2)
#define KOALA_ASM_CMP_LT_U(src1, src2) KOALA_ASM_R(CMP_LT_U, 0, src1, src2)
#define KOALA_ASM_CMP_LE_U(src1, src2) KOALA_ASM_R(CMP_LE_U, 0, src1, src2)
#define KOALA_ASM_CMP_LT_F(src1, src2) KOALA_ASM_R(CMP_LT_F, 0, src1, src2)
#define KOALA_ASM_CMP_LE_F(src1, src2) KOALA_ASM_R(CMP_LE_F, 0, src1, src2)

#define KOALA_ASM_JMP(offset) KOALA_ASM_IX24(JMP, offset)
#define KOALA_ASM_JEZ(offset) KOALA_ASM_IX24(JEZ, offset)
#define KOALA_ASM_JNZ(offset) KOALA_ASM_IX24(JNZ, offset)

///////////////


#endif