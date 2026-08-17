#pragma once

#include <stdint.h>

enum OpCode : uint8_t {
    NONE,

    RET,
    
    MOV_IMM16,
    MOV_REG,

    ADD_IMM16,
    ADD_REG,
    
    SUB_IMM16,
    SUB_IMM16_R,
    SUB_REG,
    
    MUL_IMM16,
    MUL_REG,
    
    IDIV_IMM16,
    IDIV_IMM16_R,
    IDIV_REG,
    
    DIV_IMM16,
    DIV_IMM16_R,
    DIV_REG,
    
    NEG_IMM16,
    NEG_REG,

    IREM_IMM16,
    IREM_IMM16_R,
    IREM_REG,

    REM_IMM16,
    REM_IMM16_R,
    REM_REG,

    AND_IMM16,
    AND_REG,

    OR_IMM16,
    OR_REG,

    XOR_IMM16,
    XOR_REG,

    NOT_IMM16,
    NOT_REG,

    SHL_IMM16,
    SHL_IMM16_R,
    SHL_REG,

    SHR_IMM16,
    SHR_IMM16_R,
    SHR_REG,

    SAR_IMM16,
    SAR_IMM16_R,
    SAR_REG,
};