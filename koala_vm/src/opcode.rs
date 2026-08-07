#[derive(Debug, Clone, Copy, PartialEq)]
#[repr(u8)]
pub enum OpCode{
    None,

    Ret,
    Push,
}