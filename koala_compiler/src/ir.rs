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

    fn get_push_size(&self, val: u64) -> u64 {
        if val <= u8::MAX as u64 { 1 }
        else if val <= u16::MAX as u64 { 2 }
        else if val <= u32::MAX as u64 { 4 }
        else { 8 }
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

                IRNode::Instruction { opcode, operand} => {
                    curr_bytecode_offset += 1;

                    curr_bytecode_offset += match operand {
                        Operand::None => 0,
                        Operand::IntConstant(val) => {
                            if *opcode == OpCode::Push {
                                self.get_push_size(*val)
                            } else { 8 }
                        },
                    };
                }
            }
        }
        

        for node in &self.nodes{
            if let IRNode::Instruction { opcode, operand } = node{
                if *opcode == OpCode::Push {
                    if let Operand::IntConstant(val) = *operand{
                        if val <= u8::MAX as u64{
                            bytecode.push(OpCode::Push1b as u8);
                            bytecode.push(val as u8);
                        } else if val <= u16::MAX as u64{
                            bytecode.push(OpCode::Push2b as u8);
                            bytecode.extend_from_slice(&(val as u16).to_le_bytes());
                        } else if val <= u32::MAX as u64{
                            bytecode.push(OpCode::Push4b as u8);
                            bytecode.extend_from_slice(&(val as u32).to_le_bytes());
                        } else {
                            bytecode.push(OpCode::Push8b as u8);
                            bytecode.extend_from_slice(&val.to_le_bytes());
                        }
                    }
                }
                else {
                
                    bytecode.push(*opcode as u8);

                    match operand {
                        Operand::None => {},

                        Operand::IntConstant(val) => {
                            let bytes: Vec<u8> = match *opcode {
                                OpCode::Push1b => (*val as u8).to_le_bytes().to_vec(),
                                OpCode::Push2b => (*val as u16).to_le_bytes().to_vec(),
                                OpCode::Push4b => (*val as u32).to_le_bytes().to_vec(),

                                _ => val.to_le_bytes().to_vec()
                            };
                            bytecode.extend_from_slice(&bytes);
                        }

                    }
                }
            }
        }

        return bytecode;
    }
}