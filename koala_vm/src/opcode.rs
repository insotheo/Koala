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
    Rem,
    URem,

    FAdd,
    FSub,
    FNeg,
    FMul,
    FDiv,

    And,
    Or,
    Xor,
    Not,
    Shl,
    Shrl,
    Shra,

    Eq,
    Neq,
    Cmplt,
    Cmple,
    UCmplt,
    UCmple,
    FCmplt,
    FCmple,

    Jmp,
    Jmp1b,
    Jmp2b,
    Jmp4b,
    Jmp8b,
    Jez,
    Jez1b,
    Jez2b,
    Jez4b,
    Jez8b,
    Jnz,
    Jnz1b,
    Jnz2b,
    Jnz4b,
    Jnz8b,

    ConvF2I,
    ConvF2U,
    ConvI2F,
    ConvU2F,
}