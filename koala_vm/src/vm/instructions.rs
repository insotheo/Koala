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