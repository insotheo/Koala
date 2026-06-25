#include "VM.h"

#include "Registers.h"
#include "OpCodes.h"
#include "VMData.h"
#include <cstdint>
#include <vector>
#include <bit>
#include <iostream>

namespace Koala{

    void KoalaVM::Run(VMData* input, Executable* exec){
        static const void* dispatch_table[] = {
            &&do_ret,
            
            &&do_mov,
            &&do_mov_imm,
            &&do_load_const,

            &&do_and,
            &&do_or,
            &&do_xor,
            &&do_not,
            &&do_shl,
            &&do_shr_s,
            &&do_shr_u,

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

            &&do_conv_si2f,
            &&do_conv_ui2f,
            &&do_conv_f2si,
            &&do_conv_f2ui,

            &&do_cmp_eq,
            &&do_cmp_neq,
            &&do_cmp_lt_s,
            &&do_cmp_le_s,
            &&do_cmp_lt_u,
            &&do_cmp_le_u,
            &&do_cmp_lt_f,
            &&do_cmp_le_f,

            &&do_jmp,
            &&do_jez,
            &&do_jnz,
        };
        
        //MODE 1: Translation
        if(input && exec){
            exec->MemRequired = input->MemRequired;
            exec->ConstantPool = input->ConstantPool;
            exec->Bytecode.resize(input->Bytecode.size());
            
            for(size_t i = 0; i < input->Bytecode.size(); ++i){
                uint32_t raw = input->Bytecode[i];
                uint8_t op = (raw >> 24) & 0xFF;
                exec->Bytecode[i].Handler = dispatch_table[op];
                exec->Bytecode[i].Rx = (raw >> 16) & 0xFF;
                exec->Bytecode[i].Ry = (raw >> 8) & 0xFF;
                exec->Bytecode[i].Rz = raw & 0xFF;
                if(op == OpCode::MOV_IMM || op == OpCode::LOAD_CONST){
                    exec->Bytecode[i].Imm = raw & 0xFFFF;
                }
                else{
                    exec->Bytecode[i].Imm = static_cast<int32_t>((raw & 0xFFFFFF) << 8) >> 8;
                }
            }
            return;
        }
        
        //MODE2: Executing
        // std::vector<uint8_t> ram(exec->MemRequired, 0);
        alignas(64) uint64_t regs[REGISTERS_AMOUNT] = {0};

        bool reg_flags = false;
        const Instruction* ip = exec->Bytecode.data();
        const uint64_t* const_pool = exec->ConstantPool.data();
        
        #define DISPATCH() goto *ip->Handler
        #define DISPATCH_NEXT() goto *(++ip)->Handler

        DISPATCH();
        
        do_ret: {
            //DBG
            for(int i = 0; i < REGISTERS_AMOUNT; ++i){
                std::cout << "R" << i << ": U:" << regs[i] << " | S: " << std::bit_cast<int64_t>(regs[i]) << " | F: " << std::bit_cast<double>(regs[i]) << "\n";
            }

            return;
        }
        
        do_mov:{
            regs[ip->Rx] = regs[ip->Ry];
            DISPATCH_NEXT();
        }

        do_mov_imm: {
            regs[ip->Rx] = ip->Imm;
            DISPATCH_NEXT();
        }

        do_load_const: {
            regs[ip->Rx] = const_pool[ip->Imm];
            DISPATCH_NEXT();
        }

        do_and: {
            regs[ip->Rx] = regs[ip->Ry] & regs[ip->Rz];
            DISPATCH_NEXT();
        }

        do_or: {
            regs[ip->Rx] = regs[ip->Ry] | regs[ip->Rz];
            DISPATCH_NEXT();
        }

        do_xor: {
            regs[ip->Rx] = regs[ip->Ry] ^ regs[ip->Rz];
            DISPATCH_NEXT();
        }

        do_not: {
            regs[ip->Rx] = ~regs[ip->Ry];
            DISPATCH_NEXT();
        }

        do_shl: {
            regs[ip->Rx] = regs[ip->Ry] << regs[ip->Rz];
            DISPATCH_NEXT();
        }

        do_shr_s: {
            regs[ip->Rx] = std::bit_cast<uint64_t>(std::bit_cast<int64_t>(regs[ip->Ry]) >> regs[ip->Rz]);
            DISPATCH_NEXT();
        }

        do_shr_u: {
            regs[ip->Rx] = regs[ip->Ry] >> regs[ip->Rz];
            DISPATCH_NEXT();
        }

        do_add: {
            regs[ip->Rx] = regs[ip->Ry] + regs[ip->Rz];
            DISPATCH_NEXT();
        }

        do_sub: {
            regs[ip->Rx] = regs[ip->Ry] - regs[ip->Rz];
            DISPATCH_NEXT();   
        }

        do_mul: {
            regs[ip->Rx] = regs[ip->Ry] * regs[ip->Rz];
            DISPATCH_NEXT();
        }
        
        do_div_s: {
            //TODO: zero devision panic
            int64_t val1 = std::bit_cast<int64_t>(regs[ip->Ry]);
            int64_t val2 = std::bit_cast<int64_t>(regs[ip->Rz]);
            regs[ip->Rx] = std::bit_cast<uint64_t>(val1 / val2);
            DISPATCH_NEXT();
        }
        
        do_div_u:{
            //TODO: zero devision panic
            regs[ip->Rx] = regs[ip->Ry] / regs[ip->Rz];
            DISPATCH_NEXT();
        }

        do_mod_s: {
            //TODO: zero devision panic
            int64_t val1 = std::bit_cast<int64_t>(regs[ip->Ry]);
            int64_t val2 = std::bit_cast<int64_t>(regs[ip->Rz]);
            regs[ip->Rx] = std::bit_cast<uint64_t>(val1 % val2);
            DISPATCH_NEXT();
        }

        do_mod_u: {
            //TODO: zero devision panic
            regs[ip->Rx] = regs[ip->Ry] % regs[ip->Rz];
            DISPATCH_NEXT();
        }

        do_add_f: {
            double res = std::bit_cast<double>(regs[ip->Ry]) + std::bit_cast<double>(regs[ip->Rz]);
            regs[ip->Rx] = std::bit_cast<uint64_t>(res);
            DISPATCH_NEXT();
        }

        do_sub_f: {
            double res = std::bit_cast<double>(regs[ip->Ry]) - std::bit_cast<double>(regs[ip->Rz]);
            regs[ip->Rx] = std::bit_cast<uint64_t>(res);
            DISPATCH_NEXT();
        }

        do_mul_f: {
            double res = std::bit_cast<double>(regs[ip->Ry]) * std::bit_cast<double>(regs[ip->Rz]);
            regs[ip->Rx] = std::bit_cast<uint64_t>(res);
            DISPATCH_NEXT();
        }

        do_div_f: {
            //TODO: zero devision panic
            double res = std::bit_cast<double>(regs[ip->Ry]) / std::bit_cast<double>(regs[ip->Rz]);
            regs[ip->Rx] = std::bit_cast<uint64_t>(res);
            DISPATCH_NEXT();
        }

        do_conv_si2f: {
            int64_t ival = std::bit_cast<int64_t>(regs[ip->Ry]);
            double fval = static_cast<double>(ival);
            regs[ip->Rx] = std::bit_cast<uint64_t>(fval);
            DISPATCH_NEXT();
        }

        do_conv_ui2f: {
            regs[ip->Rx] = std::bit_cast<uint64_t>(static_cast<double>(regs[ip->Ry]));
            DISPATCH_NEXT();
        }

        do_conv_f2si: {
            double fval = std::bit_cast<double>(regs[ip->Ry]);
            int64_t ival = static_cast<int64_t>(fval);
            regs[ip->Rx] = std::bit_cast<uint64_t>(ival);
            DISPATCH_NEXT();
        }

        do_conv_f2ui: {
            double fval = std::bit_cast<double>(regs[ip->Ry]);
            regs[ip->Rx] = static_cast<uint64_t>(fval);
            DISPATCH_NEXT();
        }

        do_cmp_eq: {
            reg_flags = (regs[ip->Ry] == regs[ip->Rz]);
            DISPATCH_NEXT();
        }

        do_cmp_neq: {
            reg_flags = (regs[ip->Ry] != regs[ip->Rz]);
            DISPATCH_NEXT();
        }

        do_cmp_lt_s: {
            int64_t val1 = std::bit_cast<int64_t>(regs[ip->Ry]);
            int64_t val2 = std::bit_cast<int64_t>(regs[ip->Rz]);
            reg_flags = (val1 < val2);
            DISPATCH_NEXT();
        }

        do_cmp_le_s: {
            int64_t val1 = std::bit_cast<int64_t>(regs[ip->Ry]);
            int64_t val2 = std::bit_cast<int64_t>(regs[ip->Rz]);
            reg_flags = (val1 <= val2);
            DISPATCH_NEXT();
        }

        do_cmp_lt_u: {
            reg_flags = (regs[ip->Ry] < regs[ip->Rz]);
            DISPATCH_NEXT();
        }

        do_cmp_le_u: {
            reg_flags = (regs[ip->Ry] <= regs[ip->Rz]);
            DISPATCH_NEXT();
        }

        do_cmp_lt_f: {
            double val1 = std::bit_cast<double>(regs[ip->Ry]);
            double val2 = std::bit_cast<double>(regs[ip->Rz]);
            reg_flags = (val1 < val2);
            DISPATCH_NEXT();
        }

        do_cmp_le_f: {
            double val1 = std::bit_cast<double>(regs[ip->Ry]);
            double val2 = std::bit_cast<double>(regs[ip->Rz]);
            reg_flags = (val1 <= val2);
            DISPATCH_NEXT();
        }

        do_jmp: {
            ip += ip->Imm;
            DISPATCH();
        }

        do_jez: {
            if(!reg_flags){
                ip += ip->Imm;
                DISPATCH();
            }
            DISPATCH_NEXT();
        }

        do_jnz: {
            if(reg_flags){
                ip += ip->Imm;
                DISPATCH();
            }
            DISPATCH_NEXT();
        }
    }

}