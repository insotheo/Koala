use std::process;

use crate::opcode::OpCode;

const STACK_SIZE: usize = 16;

macro_rules! read_bytes {
    ($vm:ident, $ip:ident, $bytes_count:expr, $type:ident) => {
        {
            let mut bytes = [0u8; $bytes_count];
            let src_ptr = $vm.bytecode.as_ptr().add(*$ip);
            std::ptr::copy_nonoverlapping(src_ptr, bytes.as_mut_ptr(), $bytes_count);
            *$ip += $bytes_count;

            $type::from_le_bytes(bytes) as u64
        }
    };
}

macro_rules! define_instructions {
    (
        ctx: ($vm:ident, $ip:ident, $sp:ident, $stack:ident);

        $(
            $name:ident => $body:block
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

        table[OpCode::And as usize] = vm_and;
        table[OpCode::Or as usize] = vm_or;
        table[OpCode::Xor as usize] = vm_xor;
        table[OpCode::Not as usize] = vm_not;
        table[OpCode::Shl as usize] = vm_shl;
        table[OpCode::Shrl as usize] = vm_shrl;
        table[OpCode::Shra as usize] = vm_shra;

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
    ctx: (vm, ip, sp, stack);

    vm_ret => {
        println!("VM finished via ret");

        println!("===STACK===");
        for i in 0..*sp{
            println!("U: {} | S: {} | F: {:.5}", stack[i], stack[i] as i64, f64::from_bits(stack[i]));
        }
        println!("===========");

        return false;
    }


    vm_push1b => {
        stack[*sp] = read_bytes!(vm, ip, 1, u8);
        *sp += 1;
    }
    vm_push2b => {
        stack[*sp] = read_bytes!(vm, ip, 2, u16);
        *sp += 1;
    }
    vm_push4b => {
        stack[*sp] = read_bytes!(vm, ip, 4, u32);
        *sp += 1;
    }
    vm_push8b => {
        stack[*sp] = read_bytes!(vm, ip, 8, u64);
        *sp += 1;
    }

    vm_add => {
        let a = *stack.get_unchecked(*sp - 2);
        let b = *stack.get_unchecked(*sp - 1);
        stack[*sp - 2] = a.wrapping_add(b);
        *sp -= 1;
    }

    vm_sub => {
        let a = *stack.get_unchecked(*sp - 2);
        let b = *stack.get_unchecked(*sp - 1);
        stack[*sp - 2] = a.wrapping_sub(b);
        *sp -= 1;
    }

    vm_neg => {
        stack[*sp - 1] = stack[*sp - 1].wrapping_neg();
    }

    vm_mul => {
        let a = *stack.get_unchecked(*sp - 2) as i64;
        let b = *stack.get_unchecked(*sp - 1) as i64;
        stack[*sp - 2] = a.wrapping_mul(b) as u64;
        *sp -= 1;
    }

    vm_umul => {
        let a = *stack.get_unchecked(*sp - 2);
        let b = *stack.get_unchecked(*sp - 1);
        stack[*sp - 2] = a.wrapping_mul(b);
        *sp -= 1;
    }

    vm_div => {
        let a = *stack.get_unchecked(*sp - 2) as i64;
        let b = *stack.get_unchecked(*sp - 1) as i64;
        stack[*sp - 2] = (a.wrapping_div(b)) as u64;
        *sp -= 1;
    }

    vm_udiv => {
        let a = *stack.get_unchecked(*sp - 2);
        let b = *stack.get_unchecked(*sp - 1);
        stack[*sp - 2] = a.wrapping_div(b);
        *sp -= 1;
    }

    vm_and => {
        let a = *stack.get_unchecked(*sp - 2);
        let b = *stack.get_unchecked(*sp - 1);
        stack[*sp - 2] = a & b;
        *sp -= 1;
    }

    vm_or => {
        let a = *stack.get_unchecked(*sp - 2);
        let b = *stack.get_unchecked(*sp - 1);
        stack[*sp - 2] = a | b;
        *sp -= 1;
    }

    vm_xor => {
        let a = *stack.get_unchecked(*sp - 2);
        let b = *stack.get_unchecked(*sp - 1);
        stack[*sp - 2] = a ^ b;
        *sp -= 1;
    }

    vm_not => {
        stack[*sp - 1] = !stack.get_unchecked(*sp - 1);
    }

    vm_shl => {
        let a = *stack.get_unchecked(*sp - 2);
        let b = *stack.get_unchecked(*sp - 1);
        stack[*sp - 2] = a.wrapping_shl(b as u32);
        *sp -= 1;
    }

    vm_shrl => {
        let a = *stack.get_unchecked(*sp - 2);
        let b = *stack.get_unchecked(*sp - 1);
        stack[*sp - 2] = a.wrapping_shr(b as u32);
        *sp -= 1;
    }

    vm_shra => {
        let a = *stack.get_unchecked(*sp - 2) as i64;
        let b = *stack.get_unchecked(*sp - 1);
        stack[*sp - 2] = a.wrapping_shr(b as u32) as u64;
        *sp -= 1;
    }

    vm_unknown => {
        eprintln!("VM Critical error: Unknown instruction at {}", *ip - 1);
        process::exit(1);
    }
}