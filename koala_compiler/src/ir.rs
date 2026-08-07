use koala_vm::opcode::OpCode;

#[derive(Debug, Clone)]
pub enum Operand{
    None,
    IntConstant(u64),
    // Label(String),
}

#[derive(Debug, Clone)]
pub enum IRNode{
    LabelDeclaraction(String),

    Instruction {
        opcode: OpCode,
        operand: Operand,
    }
}

pub struct ProgramIR{
    pub nodes: Vec<IRNode>,
}

impl ProgramIR{
    pub fn new(nodes: Vec<IRNode>) -> Self{
        return ProgramIR { nodes: nodes };
    }
}