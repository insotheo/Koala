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
    IREM_IMM16,
    IREM_IMM16_R,
    IREM_REG,
    REM_IMM16,
    REM_IMM16_R,
    REM_REG,
};