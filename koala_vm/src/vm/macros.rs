#[macro_export]
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

#[macro_export]
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
            pub unsafe fn $name(_vm: &VM, _ip: &mut usize, _sp: &mut usize, _stack: &mut [u64; STACK_SIZE]) -> bool{
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