use std::collections::HashMap;

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

    pub fn compile_to_bytes(&self) -> Vec<u8>{
        let mut label_addresses: HashMap<String, u64> = HashMap::new();
        let mut bytecode: Vec<u8> = Vec::new();
        let mut curr_bytecode_offset: u64 = 0;

        for node in &self.nodes{
            match node {
                IRNode::LabelDeclaraction(name) => {
                    label_addresses.insert(name.clone(), curr_bytecode_offset);
                }

                IRNode::Instruction { opcode: _, operand } => {
                    curr_bytecode_offset += 1;

                    curr_bytecode_offset += match operand {
                        Operand::None => 0,
                        Operand::IntConstant(_) => 8,
                    };
                }
            }
        }
        

        for node in &self.nodes{
            if let IRNode::Instruction { opcode, operand } = node{
                bytecode.push(*opcode as u8);

                match operand {
                    Operand::None => {},

                    Operand::IntConstant(val) => {
                        let bytes = val.to_le_bytes();

                        bytecode.extend_from_slice(&bytes);
                    }

                }
            }
        }

        return bytecode;
    }
}