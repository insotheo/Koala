#include "VM.h"

#include "Registers.h"
#include <cstdint>
#include <iostream>
#include <vector>
#include <bit>

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
            
            &&do_mov,
            &&do_mov_imm,
            &&do_load_const,

            &&do_add,
            &&do_sub,
            &&do_mul,
            &&do_div_s,
            &&do_div_u,
            &&do_mod_s,
            &&do_mod_u,

            &&do_add_f,
            &&do_sub_f,
            &&do_mul_f,
            &&do_div_f,
        };

        #define DISPATCH() goto *dispatch_table[((*ip++) >> 24) & 0xFF]

        #define DECODE_I(r_dst, imm16)\
            uint8_t r_dst = (*(ip - 1) >> 16) & 0xFF;\
            uint16_t imm16 = *(ip - 1) & 0xFFFF

        #define DECODE_R(r_dst, r_src1, r_src2)\
            uint8_t r_dst = (*(ip - 1) >> 16) & 0xFF;\
            uint8_t r_src1 = (*(ip - 1) >> 8) & 0xFF;\
            uint8_t r_src2 = *(ip - 1) & 0xFF

        DISPATCH();
        
        do_ret: {
            //TODO: call
            //DBG
            std::cout << "===[KOALA VM DUMP]===\n";
            for(int i = 0; i < REGISTERS_AMOUNT; ++i)
            {   
                double fval;
                std::memcpy(&fval, &regs[i], sizeof(double));
                std::cout << "R" << i << ": U:" << regs[i] << " | S: " << static_cast<int64_t>(regs[i]) << " | F: " << fval << "\n";
            }
            std::cout << "==========\n";
            
            return;
        }
        
        do_mov:{
            DECODE_R(dst, src, _);
            regs[dst] = regs[src];
            DISPATCH();
        }

        do_mov_imm: {
            DECODE_I(dst, imm);
            regs[dst] = imm;
            DISPATCH();
        }

        do_load_const: {
            DECODE_I(dst, const_idx);
            regs[dst] = const_pool[const_idx];  
            DISPATCH();
        }

        do_add: {
            DECODE_R(dst, s1, s2);
            regs[dst] = regs[s1] + regs[s2];
            DISPATCH();
        }

        do_sub: {
            DECODE_R(dst, s1, s2);
            regs[dst] = regs[s1] - regs[s2];
            DISPATCH();   
        }

        do_mul: {
            DECODE_R(dst, s1, s2);
            regs[dst] = regs[s1] * regs[s2];
            DISPATCH();
        }
        
        do_div_s: {
            //TODO: zero devision panic
            DECODE_R(dst, s1, s2);
            int64_t val1 = static_cast<int64_t>(regs[s1]);
            int64_t val2 = static_cast<int64_t>(regs[s2]);
            regs[dst] = static_cast<uint64_t>(val1 / val2);
            DISPATCH();
        }
        
        do_div_u:{
            //TODO: zero devision panic
            DECODE_R(dst, s1, s2);
            regs[dst] = static_cast<uint64_t>(regs[s1] / regs[s2]);
            DISPATCH();
        }

        do_mod_s: {
            //TODO: zero devision panic
            DECODE_R(dst, s1, s2);
            int64_t val1 = static_cast<int64_t>(regs[s1]);
            int64_t val2 = static_cast<int64_t>(regs[s2]);
            regs[dst] = static_cast<uint64_t>(val1 % val2);
            DISPATCH();
        }

        do_mod_u: {
            //TODO: zero devision panic
            DECODE_R(dst, s1, s2);
            regs[dst] = static_cast<uint64_t>(regs[s1] % regs[s2]);
            DISPATCH();
        }

        do_add_f: {
            DECODE_R(dst, s1, s2);
            double res = std::bit_cast<double>(regs[s1]) + std::bit_cast<double>(regs[s2]);
            std::memcpy(&regs[dst], &res, sizeof(double));
            DISPATCH();
        }

        do_sub_f: {
            DECODE_R(dst, s1, s2);
            double res = std::bit_cast<double>(regs[s1]) - std::bit_cast<double>(regs[s2]);
            std::memcpy(&regs[dst], &res, sizeof(double));  
            DISPATCH();
        }

        do_mul_f: {
            DECODE_R(dst, s1, s2);
            double res = std::bit_cast<double>(regs[s1]) * std::bit_cast<double>(regs[s2]);
            std::memcpy(&regs[dst], &res, sizeof(double));
            DISPATCH();
        }

        do_div_f: {
            //TODO: zero devision panic
            DECODE_R(dst, s1, s2);
            double res = std::bit_cast<double>(regs[s1]) / std::bit_cast<double>(regs[s2]);
            std::memcpy(&regs[dst], &res, sizeof(double));
            DISPATCH();
        }
    }

}