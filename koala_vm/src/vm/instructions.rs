use std::process;
use crate::vm::{VM, STACK_SIZE};

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
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = a.wrapping_add(b);
    }
    vm_sub => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = a.wrapping_sub(b);
    }
    vm_neg => {
        stack[*sp - 1] = stack[*sp - 1].wrapping_neg();
    }
    vm_mul => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = (a as i64).wrapping_mul(b as i64) as u64;
    }
    vm_umul => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = a.wrapping_mul(b);
    }
    vm_div => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = (a as i64).wrapping_div(b as i64) as u64;
    }
    vm_udiv => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = a.wrapping_div(b);
    }
    vm_rem => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = (a as i64).wrapping_rem(b as i64) as u64;
    }
    vm_urem => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = a.wrapping_rem(b);
    }


    vm_fadd => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = (f64::from_bits(a) + f64::from_bits(b)).to_bits();
    }
    vm_fsub => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = (f64::from_bits(a) - f64::from_bits(b)).to_bits();
    }
    vm_fneg => {
        stack[*sp - 1] = (-f64::from_bits(*stack.get_unchecked(*sp - 1))).to_bits();
    }
    vm_fmul => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = (f64::from_bits(a) * f64::from_bits(b)).to_bits();
    }
    vm_fdiv => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = (f64::from_bits(a) / f64::from_bits(b)).to_bits();
    }


    vm_and => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = a & b;
    }
    vm_or => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = a | b;
    }
    vm_xor => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = a ^ b;
    }
    vm_not => {
        stack[*sp - 1] = !*stack.get_unchecked(*sp - 1);
    }
    vm_shl => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = a.wrapping_shl(b as u32);
    }
    vm_shrl => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = a.wrapping_shr(b as u32);
    }
    vm_shra => {
        pop2!(stack, sp, a, b);
        stack[*sp - 1] = (a as i32).wrapping_shr(b as u32) as u64;
    }


    vm_conv_f2i => {
        stack[*sp - 1] = (f64::from_bits(*stack.get_unchecked(*sp - 1)) as i64) as u64;
    }
    vm_conv_f2u => {
        stack[*sp - 1] = f64::from_bits(*stack.get_unchecked(*sp - 1)) as u64;
    }
    vm_conv_i2f => {
        stack[*sp - 1] = (*stack.get_unchecked(*sp - 1) as i64 as f64).to_bits();
    }
    vm_conv_u2f => {
        stack[*sp - 1] = (*stack.get_unchecked(*sp - 1) as f64).to_bits();
    }


    vm_unknown => {
        eprintln!("VM Critical error: Unknown instruction at {}", *ip - 1);
        process::exit(1);
    }
}