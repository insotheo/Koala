#pragma once

#include <stdint.h>

enum OpCode : uint8_t {
    NONE,

    RET,
    
    MOV_IMM16,
    MOV_REG,
};