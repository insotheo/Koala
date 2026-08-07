#[derive(Debug, Clone, Copy, PartialEq)]
#[repr(u8)]
pub enum OpCode{
    None,

    Ret,

    Push, //only for parser and frontend use
    Push1b,
    Push2b,
    Push4b,
    Push8b,

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