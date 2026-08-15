#include "vm.h"

#include "opcodes.h"
#include "vm_config.h"
#include <string.h>
#include <stdio.h>

void koalaVMRun(uint8_t* bytecode){
    static void* dispatch_tabel[] = {
        [RET] = &&vm_ret,

        [MOV_IMM16] = &&vm_mov_imm16,
        [MOV_REG] = &&vm_mov_reg,
    };

    uint8_t* pc = &bytecode[0];
    uint64_t registers[KOALA_CORE_VM_REGISTERS_COUNT];
    
    #define DISPATCH() goto *dispatch_tabel[*pc++]

    #define UNPACK_REG(name) uint8_t name = *pc++
    #define UNPACK_IMM16(name)\
        uint16_t name;\
        memcpy(&name, pc, sizeof(uint16_t));\
        pc += sizeof(uint16_t);
    #define BITS_AS_FLOAT(src, out)\
        float out;\
        memcpy(&out, &src, sizeof(out));
    
    DISPATCH();

    vm_ret: {
        //DBG
        for(size_t i = 0; i < KOALA_CORE_VM_REGISTERS_COUNT; ++i){
            BITS_AS_FLOAT(registers[i], f);
            printf("R%.2ld S: %ld | U: %ld | F: %f\n", i, (int64_t)registers[i], registers[i], f);
        }
        /////

        return;
    }

    vm_mov_imm16: {
        UNPACK_REG(dst); UNPACK_IMM16(imm);
        registers[dst] = imm;
        DISPATCH();
    }
    
    vm_mov_reg: {
        UNPACK_REG(dst); UNPACK_REG(src);
        registers[dst] = registers[src];
        DISPATCH();
    }
}