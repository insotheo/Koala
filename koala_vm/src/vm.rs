use std::process;

use crate::opcode::OpCode;

const STACK_SIZE: usize = 16;

macro_rules! define_instructions {
    (
        $(
            $name:ident ($vm:ident, $ip:ident, $sp:ident, $stack:ident) => $body:block
        )*
    ) => {
        $(
            #[inline(always)]
            #[allow(unsafe_op_in_unsafe_fn)]
            unsafe fn $name(_vm: &VM, _ip: &mut usize, _sp: &mut usize, _stack: &mut [u64; STACK_SIZE]) -> bool{
                #[allow(unused)]
                let mut $vm = _vm;
                #[allow(unused)]
                let mut $ip = _ip;
                #[allow(unused)]
                let mut $sp = _sp;
                #[allow(unused)]
                let mut $stack = _stack;
                
                $body

                #[allow(unreachable_code)]
                true
            }
        )*
    };
}

type InstructionFn = unsafe fn(vm: &VM, ip: &mut usize, sp: &mut usize, stack: &mut [u64; STACK_SIZE]) -> bool;

pub struct VM{
    pub bytecode: Vec<u8>,
    dispatch_table: [InstructionFn; 256]
}

impl VM{
    pub fn new(bytecode: Vec<u8>) -> Self{
        let mut table: [InstructionFn; 256] = [vm_unknown; 256];

        table[OpCode::Ret as usize] = vm_ret;
        table[OpCode::Push as usize] = vm_push;

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

define_instructions! {
    vm_ret(vm, ip, sp, stack) => {
        println!("VM finished via ret");

        println!("===STACK===");
        for i in 0..*sp{
            println!("{}", stack[i]);
        }

        return true;
    }

    vm_push(vm, ip, sp, stack) => {
        let mut bytes = [0u8; 8];
        let src_ptr = vm.bytecode.as_ptr().add(*ip);
        std::ptr::copy_nonoverlapping(src_ptr, bytes.as_mut_ptr(), 8);
        *ip += 8;
        stack[*sp] = u64::from_le_bytes(bytes);
        *sp += 1;
    }

    vm_unknown(vm, ip, sp, stack) => {
        eprintln!("VM Critical error: Unknown instruction at {}", *ip - 1);
        process::exit(1);
    }
}