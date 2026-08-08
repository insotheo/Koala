use crate::opcode::OpCode;

#[macro_use]
mod macros;
mod instructions;

use instructions::*;

const STACK_SIZE: usize = 16;


type InstructionFn = unsafe fn(vm: &VM, ip: &mut usize, sp: &mut usize, stack: &mut [u64; STACK_SIZE]) -> bool;


pub struct VM{
    pub bytecode: Vec<u8>,
    dispatch_table: [InstructionFn; 256]
}

impl VM{
    pub fn new(bytecode: Vec<u8>) -> Self{
        let mut table: [InstructionFn; 256] = [vm_unknown; 256];

        //table
        table[OpCode::Ret as usize] = vm_ret;
        
        table[OpCode::Push1b as usize] = vm_push1b;
        table[OpCode::Push2b as usize] = vm_push2b;
        table[OpCode::Push4b as usize] = vm_push4b;
        table[OpCode::Push8b as usize] = vm_push8b;
        
        table[OpCode::Add as usize] = vm_add;
        table[OpCode::Sub as usize] = vm_sub;
        table[OpCode::Neg as usize] = vm_neg;
        table[OpCode::Mul as usize] = vm_mul;
        table[OpCode::UMul as usize] = vm_umul;
        table[OpCode::Div as usize] = vm_div;
        table[OpCode::UDiv as usize] = vm_udiv;
        table[OpCode::Rem as usize] = vm_rem;
        table[OpCode::URem as usize] = vm_urem;

        table[OpCode::FAdd as usize] = vm_fadd;
        table[OpCode::FSub as usize] = vm_fsub;
        table[OpCode::FNeg as usize] = vm_fneg;
        table[OpCode::FMul as usize] = vm_fmul;
        table[OpCode::FDiv as usize] = vm_fdiv;

        table[OpCode::And as usize] = vm_and;
        table[OpCode::Or as usize] = vm_or;
        table[OpCode::Xor as usize] = vm_xor;
        table[OpCode::Not as usize] = vm_not;
        table[OpCode::Shl as usize] = vm_shl;
        table[OpCode::Shrl as usize] = vm_shrl;
        table[OpCode::Shra as usize] = vm_shra;
        ////////////////////////////////////////////////////////////////////

        return VM
        { 
            bytecode: bytecode,
            dispatch_table: table
        };
    }

    pub fn run(&self){
        let mut ip: usize = 0;
        let mut sp: usize = 0;
        let mut stack: [u64; STACK_SIZE] = [0; STACK_SIZE];

        while ip < self.bytecode.len(){
            unsafe {
                let opcode_byte = *self.bytecode.get_unchecked(ip);
                ip += 1;

                let handler = *self.dispatch_table.get_unchecked(opcode_byte as usize);

                if !handler(self, &mut ip, &mut sp, &mut stack){
                    break;
                }
            }
        }
    }
}
