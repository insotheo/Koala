#include "vm.h"

#include "opcodes.h"
#include "vm_config.h"
#include <string.h>
#include <stdio.h>

void koalaVMRun(uint8_t* bytecode){
    static void* dispatch_table[] = {
        [RET]                           = &&vm_ret,

        [MOV_IMM16]                     = &&vm_mov_imm16,
        [MOV_IMM64]                     = &&vm_mov_imm64,
        [MOV_REG]                       = &&vm_mov_reg,

        [ADD_IMM16]                     = &&vm_add_imm16,
        [ADD_REG]                       = &&vm_add_reg,
        
        [SUB_IMM16]                     = &&vm_sub_imm16,
        [SUB_IMM16_R]                   = &&vm_sub_imm16_r,
        [SUB_REG]                       = &&vm_sub_reg,

        [MUL_IMM16]                     = &&vm_mul_imm16,
        [MUL_REG]                       = &&vm_mul_reg,

        [IDIV_IMM16]                    = &&vm_idiv_imm16,
        [IDIV_IMM16_R]                  = &&vm_idiv_imm16_r,
        [IDIV_REG]                      = &&vm_idiv_reg,

        [DIV_IMM16]                     = &&vm_div_imm16,
        [DIV_IMM16_R]                   = &&vm_div_imm16_r,
        [DIV_REG]                       = &&vm_div_reg,

        [NEG_IMM16]                     = &&vm_neg_imm16,
        [NEG_REG]                       = &&vm_neg_reg,
        
        [IREM_IMM16]                    = &&vm_irem_imm16,
        [IREM_IMM16_R]                  = &&vm_irem_imm16_r,
        [IREM_REG]                      = &&vm_irem_reg,

        [REM_IMM16]                     = &&vm_rem_imm16,
        [REM_IMM16_R]                   = &&vm_rem_imm16_r,
        [REM_REG]                       = &&vm_rem_reg,

        [AND_IMM16]                     = &&vm_and_imm16,
        [AND_REG]                       = &&vm_and_reg,

        [OR_IMM16]                      = &&vm_or_imm16,
        [OR_REG]                        = &&vm_or_reg,

        [XOR_IMM16]                     = &&vm_xor_imm16,
        [XOR_REG]                       = &&vm_xor_reg,

        [NOT_IMM16]                     = &&vm_not_imm16,
        [NOT_REG]                       = &&vm_not_reg,

        [SHL_IMM16]                     = &&vm_shl_imm16,
        [SHL_IMM16_R]                   = &&vm_shl_imm16_r,
        [SHL_REG]                       = &&vm_shl_reg,

        [SHR_IMM16]                     = &&vm_shr_imm16,
        [SHR_IMM16_R]                   = &&vm_shr_imm16_r,
        [SHR_REG]                       = &&vm_shr_reg,

        [SAR_IMM16]                     = &&vm_shr_imm16,
        [SAR_IMM16_R]                   = &&vm_shr_imm16_r,
        [SAR_REG]                       = &&vm_shr_reg,
    };

    uint8_t* pc = &bytecode[0];
    uint64_t registers[KOALA_CORE_VM_REGISTERS_COUNT] = {0};
    
    #define DISPATCH() goto *dispatch_table[*pc++]

    #define READ_REG() (*pc++)
    #define DECODE_REG(name) uint8_t name = READ_REG()
    #define USE_REG(name) registers[name]

    #define READ_IMM_N(type) ({\
        type val;\
        memcpy(&val, pc, sizeof(type));\
        pc += sizeof(type);\
        val;\
    })
    #define DECODE_IMM_N(type, name) type name = READ_IMM_N(type)
    #define USE_IMM_N(type, name) ((type)name)

    #define READ_IMM16() READ_IMM_N(int16_t)
    #define DECODE_IMM16(name) DECODE_IMM_N(int16_t, name)
    #define USE_IMM16(name) USE_IMM_N(int16_t, name)

    #define CAST_TO_SIGNED(val) ((int64_t)val)
    #define CAST_TO_UNSIGNED(val) ((uint64_t)val)

    #define BITS_AS_FLOAT(bits)({\
            double fbits;\
            memcpy(&fbits, &bits, sizeof(fbits));\
            fbits;\
        })
    

    #define VM_BINARY_OP(instr, operation, type1, type2, mod)\
        vm_##instr: {\
            DECODE_REG(dst); DECODE_##type1(op1); DECODE_##type2(op2);\
            USE_REG(dst) = (uint64_t)(CAST_TO_##mod(USE_##type1(op1)) operation CAST_TO_##mod(USE_##type2(op2)));\
            DISPATCH();\
        }
    #define VM_UNARY_OP(instr, operation, type, mod)\
        vm_##instr: {\
            DECODE_REG(dst); DECODE_##type(op);\
            USE_REG(dst) = (uint64_t)(operation CAST_TO_##mod(USE_##type(op)));\
            DISPATCH();\
        }

    DISPATCH();

    vm_ret: {
        //DBG
        for(size_t i = 0; i < KOALA_CORE_VM_REGISTERS_COUNT; ++i){
            double f = BITS_AS_FLOAT(registers[i]);
            printf("R%.2ld S: %ld | U: %lu | F: %f\n", i, (int64_t)registers[i], (uint64_t)registers[i], f);
        }
        /////

        return;
    }

    vm_mov_imm16: {
        DECODE_REG(dst); DECODE_IMM16(imm);
        USE_REG(dst) = USE_IMM16(imm);
        DISPATCH();
    }
    
    vm_mov_imm64: {
        DECODE_REG(dst); DECODE_IMM_N(int64_t, imm);
        USE_REG(dst) = USE_IMM_N(int64_t, imm);
        DISPATCH();
    }

    vm_mov_reg: {
        DECODE_REG(dst); DECODE_REG(src);
        USE_REG(dst) = USE_REG(src);
        DISPATCH();
    }

    VM_BINARY_OP(add_imm16,     +, REG, IMM16, SIGNED)
    VM_BINARY_OP(add_reg,       +, REG, REG, SIGNED)
    
    VM_BINARY_OP(sub_imm16,     -, REG, IMM16, SIGNED)
    VM_BINARY_OP(sub_imm16_r,   -, IMM16, REG, SIGNED)
    VM_BINARY_OP(sub_reg,       -, REG, REG, SIGNED)

    VM_BINARY_OP(mul_imm16,     *, REG, IMM16, SIGNED)
    VM_BINARY_OP(mul_reg,       *, REG, REG, SIGNED)

    VM_BINARY_OP(idiv_imm16,    /, REG, IMM16, SIGNED)
    VM_BINARY_OP(idiv_imm16_r,  /, IMM16, REG, SIGNED)
    VM_BINARY_OP(idiv_reg,      /, REG, REG, SIGNED)

    VM_BINARY_OP(div_imm16,     /, REG, IMM16, UNSIGNED)
    VM_BINARY_OP(div_imm16_r,   /, IMM16, REG, UNSIGNED)
    VM_BINARY_OP(div_reg,       /, REG, REG, UNSIGNED)

    VM_UNARY_OP(neg_imm16,      -, IMM16, UNSIGNED)
    VM_UNARY_OP(neg_reg,        -, REG, UNSIGNED)

    VM_BINARY_OP(irem_imm16,    %, REG, IMM16, SIGNED)
    VM_BINARY_OP(irem_imm16_r,  %, IMM16, REG, SIGNED)
    VM_BINARY_OP(irem_reg,      %, REG, REG, SIGNED)

    VM_BINARY_OP(rem_imm16,     %, REG, IMM16, UNSIGNED)
    VM_BINARY_OP(rem_imm16_r,   %, IMM16, REG, UNSIGNED)
    VM_BINARY_OP(rem_reg,       %, REG, REG, UNSIGNED)

    VM_BINARY_OP(and_imm16,     &, REG, IMM16, SIGNED)
    VM_BINARY_OP(and_reg,       &, REG, REG, SIGNED)

    VM_BINARY_OP(or_imm16,      |, REG, IMM16, SIGNED)
    VM_BINARY_OP(or_reg,        |, REG, REG, SIGNED)

    VM_BINARY_OP(xor_imm16,     ^, REG, IMM16, SIGNED)
    VM_BINARY_OP(xor_reg,       ^, REG, REG, SIGNED)
    
    VM_UNARY_OP(not_imm16,      ~, IMM16, UNSIGNED)
    VM_UNARY_OP(not_reg,        ~, REG, UNSIGNED)

    VM_BINARY_OP(shl_imm16,     <<, REG, IMM16, SIGNED)
    VM_BINARY_OP(shl_imm16_r,   <<, IMM16, REG, SIGNED)
    VM_BINARY_OP(shl_reg,       <<, REG, REG, SIGNED)

    VM_BINARY_OP(shr_imm16,     >>, REG, IMM16, UNSIGNED)
    VM_BINARY_OP(shr_imm16_r,   >>, IMM16, REG, UNSIGNED)
    VM_BINARY_OP(shr_reg,       >>, REG, REG, UNSIGNED)

    VM_BINARY_OP(sar_imm16,     >>, REG, IMM16, SIGNED)
    VM_BINARY_OP(sar_imm16_r,   >>, IMM16, REG, SIGNED)
    VM_BINARY_OP(sar_reg,       >>, REG, REG, SIGNED)
}