use crate::opcode::OpCode;

#[macro_use]
mod macros;

const STACK_SIZE: usize = 16;

pub struct VM{
    pub bytecode: Vec<u8>,
}

impl VM{
    pub fn new(bytecode: Vec<u8>) -> Self{
        return VM
        { 
            bytecode: bytecode,
        };
    }

    pub fn run(&self){
        let vm = self;
        let code = self.bytecode.as_slice();

        let mut ip: usize = 0;
        let mut sp: usize = 0;
        let mut stack: [u64; STACK_SIZE] = [0; STACK_SIZE];

        while ip < code.len(){
            unsafe {
                let opcode_byte = *code.get_unchecked(ip);
                ip += 1;

                let opcode: OpCode = std::mem::transmute(opcode_byte);

                define_instructions! {
                    ctx: (vm, ip, sp, stack);
                    match opcode;

                    Ret => {
                        println!("VM finished via ret");

                        println!("===STACK===");
                        for i in 0..sp{
                            println!("U: {} | S: {} | F: {:.5}", stack[i], stack[i] as i64, f64::from_bits(stack[i]));
                        }
                        println!("===========");
                    }


                    Push1b => {
                        stack[sp] = read_bytes!(vm, ip, 1, u8);
                        sp += 1;
                    }
                    Push2b => {
                        stack[sp] = read_bytes!(vm, ip, 2, u16);
                        sp += 1;
                    }
                    Push4b => {
                        stack[sp] = read_bytes!(vm, ip, 4, u32);
                        sp += 1;
                    }
                    Push8b => {
                        stack[sp] = read_bytes!(vm, ip, 8, u64);
                        sp += 1;
                    }


                    Add => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = a.wrapping_add(b) as u64;
                    }
                    Sub => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = a.wrapping_sub(b) as u64;
                    }
                    Neg => {
                        stack[sp - 1] = stack[sp - 1].wrapping_neg();
                    }
                    Mul => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = (a as i64).wrapping_mul(b as i64) as u64;
                    }
                    UMul => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = a.wrapping_mul(b);
                    }
                    Div => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = (a as i64).wrapping_div(b as i64) as u64;
                    }
                    UDiv => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = a.wrapping_div(b);
                    }
                    Rem => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = (a as i64).wrapping_rem(b as i64) as u64;
                    }
                    URem => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = a.wrapping_rem(b);
                    }


                    FAdd => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = (f64::from_bits(a) + f64::from_bits(b)) as u64;
                    }
                    FSub => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = (f64::from_bits(a) - f64::from_bits(b)) as u64;
                    }
                    FNeg => {
                        stack[sp - 1] = (-f64::from_bits(stack[sp - 1])) as u64;
                    }
                    FMul => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = (f64::from_bits(a) * f64::from_bits(b)) as u64;
                    }
                    FDiv => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = (f64::from_bits(a) / f64::from_bits(b)) as u64;
                    }


                    And => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = a & b;
                    }
                    Or => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = a | b;
                    }
                    Xor => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = a ^ b;
                    }
                    Not => {
                        stack[sp - 1] = !stack[sp - 1];
                    }
                    Shl => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = a.wrapping_shl(b as u32) as u64;
                    }
                    Shrl => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = a.wrapping_shr(b as u32) as u64;
                    }
                    Shra => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = (a as i64).wrapping_shr(b as u32) as u64;
                    }


                    Eq => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = (a == b) as u64;
                    }
                    Neq => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = (a != b) as u64;
                    }
                    Cmplt => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = ((a as i64) < (b as i64)) as u64;
                    }
                    Cmple => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = ((a as i64) <= (b as i64)) as u64;
                    }
                    UCmplt => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = (a < b) as u64;
                    }
                    UCmple => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = (a <= b) as u64;
                    }
                    FCmplt => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = (f64::from_bits(a) < f64::from_bits(b)) as u64;
                    }
                    FCmple => {
                        pop2!(stack, sp, a, b);
                        stack[sp - 1] = (f64::from_bits(a) <= f64::from_bits(b)) as u64;
                    }


                    ConvF2I => {
                        stack[sp - 1] = (f64::from_bits(stack[sp - 1]) as i64) as u64;
                    }
                    ConvF2U => {
                        stack[sp - 1] = f64::from_bits(stack[sp - 1]) as u64;
                    }
                    ConvI2F => {
                        stack[sp - 1] = ((stack[sp - 1] as i64) as f64).to_bits();
                    }
                    ConvU2F => {
                        stack[sp - 1] = (stack[sp - 1] as f64).to_bits();
                    }
                }
            }
        }
    }
}
