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
        let code = self.bytecode.as_slice();

        let mut ip: usize = 0;
        let mut sp: usize = 0;
        let mut stack: [u64; STACK_SIZE] = [0; STACK_SIZE];

        while ip < code.len(){
            unsafe {
                let opcode_byte = *code.get_unchecked(ip);
                ip += 1;

                define_instructions! {
                    ctx: (vm, ip, sp, stack);
                    match opcode_byte;

                    Ret => {
                        // println!("VM finished via ret");

                        // println!("===STACK===");
                        // for i in 0..sp{
                        //     println!("U: {} | S: {} | F: {:.5}", stack[i], stack[i] as i64, f64::from_bits(stack[i]));
                        // }
                        // println!("===========");

                        break;
                    }


                    Push1b => {
                        stack[sp + 1] = read_bytes!(code, ip, 1, u8);
                        sp += 1;
                    }
                    Push2b => {
                        stack[sp + 1] = read_bytes!(code, ip, 2, u16);
                        sp += 1;
                    }
                    Push4b => {
                        stack[sp + 1] = read_bytes!(code, ip, 4, u32);
                        sp += 1;
                    }
                    Push8b => {
                        stack[sp + 1] = read_bytes!(code, ip, 8, u64);
                        sp += 1;
                    }
                    

                    Dup => {
                        stack[sp + 1] = stack[sp];
                        sp += 1;
                    }


                    Add => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = a.wrapping_add(b) as u64;
                    }
                    Sub => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = a.wrapping_sub(b) as u64;
                    }
                    Inc => {
                        stack[sp] = stack[sp].wrapping_add(1);
                    }
                    Dec => {
                        stack[sp] = stack[sp].wrapping_sub(1);
                    }
                    Neg => {
                        stack[sp] = stack[sp].wrapping_neg();
                    }
                    Mul => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = (a as i64).wrapping_mul(b as i64) as u64;
                    }
                    UMul => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = a.wrapping_mul(b);
                    }
                    Div => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = (a as i64).wrapping_div(b as i64) as u64;
                    }
                    UDiv => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = a.wrapping_div(b);
                    }
                    Rem => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = (a as i64).wrapping_rem(b as i64) as u64;
                    }
                    URem => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = a.wrapping_rem(b);
                    }


                    FAdd => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = (f64::from_bits(a) + f64::from_bits(b)).to_bits();
                    }
                    FSub => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = (f64::from_bits(a) - f64::from_bits(b)).to_bits();
                    }
                    FNeg => {
                        stack[sp] = (-f64::from_bits(stack[sp])).to_bits();
                    }
                    FMul => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = (f64::from_bits(a) * f64::from_bits(b)).to_bits();
                    }
                    FDiv => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = (f64::from_bits(a) / f64::from_bits(b)).to_bits();
                    }


                    And => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = a & b;
                    }
                    Or => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = a | b;
                    }
                    Xor => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = a ^ b;
                    }
                    Not => {
                        stack[sp] = !stack[sp];
                    }
                    Shl => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = a.wrapping_shl(b as u32) as u64;
                    }
                    Shrl => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = a.wrapping_shr(b as u32) as u64;
                    }
                    Shra => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = (a as i64).wrapping_shr(b as u32) as u64;
                    }


                    Eq => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = (a == b) as u64;
                    }
                    Neq => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = (a != b) as u64;
                    }
                    Cmplt => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = ((a as i64) < (b as i64)) as u64;
                    }
                    Cmple => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = ((a as i64) <= (b as i64)) as u64;
                    }
                    UCmplt => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = (a < b) as u64;
                    }
                    UCmple => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = (a <= b) as u64;
                    }
                    FCmplt => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = (f64::from_bits(a) < f64::from_bits(b)) as u64;
                    }
                    FCmple => {
                        pop2!(stack, sp, a, b);
                        stack[sp] = (f64::from_bits(a) <= f64::from_bits(b)) as u64;
                    }


                    Jmp1b => {
                        let offset = read_bytes!(code, ip, 1, i8);
                        ip = (ip as i64 + offset as i64) as usize;
                    }
                    Jmp2b => {
                        let offset = read_bytes!(code, ip, 2, i16);
                        ip = (ip as i64 + offset as i64) as usize;
                    }
                    Jmp4b => {
                        let offset = read_bytes!(code, ip, 4, i32);
                        ip = (ip as i64 + offset as i64) as usize;
                    }
                    Jmp8b => {
                        let offset = read_bytes!(code, ip, 8, i64);
                        ip = (ip as i64 + offset as i64) as usize;
                    }
                    Jez1b => {
                        let offset = read_bytes!(code, ip, 1, i8);
                        if stack[sp] == 0 { ip = (ip as i64 + offset as i64) as usize; }
                        sp -= 1;
                    }
                    Jez2b => {
                        let offset = read_bytes!(code, ip, 2, i16);
                        if stack[sp] == 0 { ip = (ip as i64 + offset as i64) as usize; }
                        sp -= 1;
                    }
                    Jez4b => {
                        let offset = read_bytes!(code, ip, 4, i32);
                        if stack[sp] == 0 { ip = (ip as i64 + offset as i64) as usize; }
                        sp -= 1;
                    }
                    Jez8b => {
                        let offset = read_bytes!(code, ip, 8, i64);
                        if stack[sp] == 0 { ip = (ip as i64 + offset as i64) as usize; }
                        sp -= 1;
                    }
                    Jnz1b => {
                        let offset = read_bytes!(code, ip, 1, i8);
                        if stack[sp] != 0 { ip = (ip as i64 + offset as i64) as usize; }
                        sp -= 1;
                    }
                    Jnz2b => {
                        let offset = read_bytes!(code, ip, 2, i16);
                        if stack[sp] != 0 { ip = (ip as i64 + offset as i64) as usize; }
                        sp -= 1;
                    }
                    Jnz4b => {
                        let offset = read_bytes!(code, ip, 4, i32);
                        if stack[sp] != 0 { ip = (ip as i64 + offset as i64) as usize; }
                        sp -= 1;
                    }
                    Jnz8b => {
                        let offset = read_bytes!(code, ip, 8, i64);
                        if stack[sp] != 0 { ip = (ip as i64 + offset as i64) as usize; }
                        sp -= 1;
                    }



                    ConvF2I => {
                        stack[sp] = (f64::from_bits(stack[sp]) as i64) as u64;
                    }
                    ConvF2U => {
                        stack[sp] = f64::from_bits(stack[sp]) as u64;
                    }
                    ConvI2F => {
                        stack[sp] = ((stack[sp] as i64) as f64).to_bits();
                    }
                    ConvU2F => {
                        stack[sp] = (stack[sp] as f64).to_bits();
                    }
                }
            }
        }
    }
}
