#include "VM.h"

#include "Registers.h"
#include <cstdint>
#include <iostream>
#include <vector>

namespace Koala{

    void KoalaVM::Run(const VMData& data){
        std::vector<uint8_t> ram(data.MemRequired, 0);

        //registers and pointers
        uint64_t regs[REGISTERS_AMOUNT] = {0};
        // uint64_t reg_sp = data.MemRequired;//stack pointer
        // uint64_t reg_bp = data.MemRequired;//base pointer
        const uint32_t* ip = data.Bytecode.data();
        const uint32_t* bytecode_start = data.Bytecode.data();
        const uint64_t* const_pool = data.ConstantPool.data();
        

        static const void* dispatch_table[] = {
            &&do_ret,
            
            &&do_mov_imm,
            &&do_load_const,
            
        };

        #define DISPATCH() goto *dispatch_table[((*ip++) >> 24) & 0xFF]

        DISPATCH();


        do_ret: {
            //TODO: call
            //DBG
            std::cout << "===[KOALA VM DUMP]===\n";
            for(int i = 0; i < REGISTERS_AMOUNT; ++i){
                std::cout << "R" << i << ": " << regs[i] << "\n";
            }
            std::cout << "==========\n";
            
            return;
        }
        
        do_mov_imm: {
            uint32_t current_ins = *(ip - 1);

            uint8_t reg_dst = (current_ins >> 16) & 0xFF;
            uint16_t imm = current_ins & 0xFFFF;

            regs[reg_dst] = imm;

            DISPATCH();
        }

        do_load_const: {
            uint32_t current_ins = *(ip - 1);
            
            uint8_t reg_dst = (current_ins >> 16) & 0xFF;
            uint16_t const_idx = current_ins & 0xFFFF;

            regs[reg_dst] = const_pool[const_idx];  

            DISPATCH();
        }
    }

}