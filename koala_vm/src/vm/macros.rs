#[macro_export]
macro_rules! read_bytes {
    ($bytecode:ident, $ip:ident, $bytes_count:expr, $type:ident) => {
        {
            let mut bytes = [0u8; $bytes_count];
            let src_ptr = $bytecode.as_ptr().add($ip);
            std::ptr::copy_nonoverlapping(src_ptr, bytes.as_mut_ptr(), $bytes_count);
            $ip += $bytes_count;

            $type::from_le_bytes(bytes) as u64
        }
    };
}

#[macro_export]
macro_rules! pop2 {
    ($stack:ident, $sp:ident, $a:ident, $b:ident) => {
        $sp -= 2;
        let $a = *$stack.get_unchecked($sp);
        let $b = *$stack.get_unchecked($sp + 1);
    };
}

#[macro_export]
macro_rules! define_instructions {
    (
        ctx: ($vm:ident, $ip:ident, $sp:ident, $stack:ident);
        match $opcode:ident;

        $(
            $name:ident => $body:block
        )*


    ) => {
        match $opcode{
            $(
                crate::opcode::raw_opcodes::$name => {
                    $body
                }
            )*

            _ => {
                eprintln!("VM Critical error: Unknown instruction at {}", $ip - 1);
                std::process::exit(1);
            }
        }
    };
}