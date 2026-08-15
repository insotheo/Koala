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

    DISPATCH();

    vm_ret: {
        //DBG
        for(size_t i = 0; i < KOALA_CORE_VM_REGISTERS_COUNT; ++i){
            float f;
            memcpy(&f, &registers[i], sizeof(f));
            printf("R%.2ld S: %ld | U: %ld | F: %f\n", i, (int64_t)registers[i], registers[i], f);
        }

        return;
    }

    vm_mov_imm16: {
        uint8_t dst = *pc++;
        uint16_t imm = 0;
        memcpy(&imm, pc, sizeof(uint16_t));
        pc += 2;

        registers[dst] = imm;
        
        DISPATCH();
    }
    
    vm_mov_reg: {
        uint8_t dst = *pc++;
        uint8_t src = *pc++;
        registers[dst] = registers[src];

        DISPATCH();
    }
}