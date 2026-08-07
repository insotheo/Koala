#[derive(Debug, Clone, Copy, PartialEq)]
#[repr(u8)]
pub enum OpCode{
    None,

    Ret,
    Push,

    Add,
    Sub,
    Neg,
    Mul,
    UMul,
    Div,
    UDiv,

    And,
    Or,
    Xor,
    Not,
    Shl,
    Shrl,
    Shra,
}