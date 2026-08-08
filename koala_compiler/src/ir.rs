use std::collections::HashMap;

use koala_vm::opcode::OpCode;

#[derive(Debug, Clone)]
pub enum Operand{
    None,
    IntConstant(u64),
    Label(String),
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
        let mut addresses_changed = true;
        
        while addresses_changed { 
            addresses_changed = false;
            let mut curr_bytecode_offset: u64 = 0;
            
            for node in &self.nodes{
                match node {
                    IRNode::LabelDeclaraction(name) => {
                        let old_address = label_addresses.insert(name.clone(), curr_bytecode_offset);
                        if old_address != Some(curr_bytecode_offset) {
                            addresses_changed = true;
                        }
                    }
                    
                    IRNode::Instruction { opcode, operand} => {
                        curr_bytecode_offset += 1;

                        curr_bytecode_offset += match operand {
                            Operand::None => 0,

                            Operand::IntConstant(val) => {
                                if *opcode == OpCode::Push || *opcode == OpCode::Jmp || *opcode == OpCode::Jez || *opcode == OpCode::Jnz {
                                    self.get_push_size(*val)
                                } else { 8 }
                            },

                            Operand::Label(target_label) => {
                                if let Some(&target_addr) = label_addresses.get(target_label) {
                                    let distance = ((target_addr as i64) - (curr_bytecode_offset as i64)).abs() as u64;

                                    if distance <= i8::MAX as u64 { 1 }
                                    else if distance <= i16::MAX as u64 { 2 }
                                    else if distance <= i32::MAX as u64 { 4 }
                                    else { 8 }
                                } else { 4 }
                            }
                        };
                    }
                }
            }
        }
        
        
        let mut bytecode: Vec<u8> = Vec::new();

        for node in &self.nodes{
            if let IRNode::Instruction { opcode, operand } = node{
                if *opcode == OpCode::Push || *opcode == OpCode::Jmp || *opcode == OpCode::Jez || *opcode == OpCode::Jnz {
                    if let Operand::IntConstant(val) = *operand{
                        if val <= u8::MAX as u64{
                            bytecode.push(match *opcode{
                                OpCode::Push => OpCode::Push1b,
                                OpCode::Jmp => OpCode::Jmp1b,
                                OpCode::Jez => OpCode::Jez1b,
                                OpCode::Jnz => OpCode::Jnz1b,
                                

                                _ => OpCode::None,
                            } as u8);


                            bytecode.push(val as u8);
                        } else if val <= u16::MAX as u64{
                            bytecode.push(match *opcode{
                                OpCode::Push => OpCode::Push2b,
                                OpCode::Jmp => OpCode::Jmp2b,
                                OpCode::Jez => OpCode::Jez2b,
                                OpCode::Jnz => OpCode::Jnz2b,
                                

                                _ => OpCode::None,
                            } as u8);

                            bytecode.extend_from_slice(&(val as u16).to_le_bytes());
                        } else if val <= u32::MAX as u64{
                            bytecode.push(match *opcode{
                                OpCode::Push => OpCode::Push4b,
                                OpCode::Jmp => OpCode::Jmp4b,
                                OpCode::Jez => OpCode::Jez4b,
                                OpCode::Jnz => OpCode::Jnz4b,
                                

                                _ => OpCode::None,
                            } as u8);

                            bytecode.extend_from_slice(&(val as u32).to_le_bytes());
                        } else {
                            bytecode.push(match *opcode{
                                OpCode::Push => OpCode::Push8b,
                                OpCode::Jmp => OpCode::Jmp8b,
                                OpCode::Jez => OpCode::Jez8b,
                                OpCode::Jnz => OpCode::Jnz8b,
                                

                                _ => OpCode::None,
                            } as u8);

                            bytecode.extend_from_slice(&val.to_le_bytes());
                        }

                        continue;
                    }

                    else if let Operand::Label(target_label) = operand {
                        let target_addr = *label_addresses.get(target_label).expect("Label not found!");
                        let current_op_pos = bytecode.len() as i64;

                        let mut size = 4;
                        for temp_size in &[1, 2, 4, 8]{
                            let next_ip = current_op_pos + 1 + temp_size;
                            let offset = target_addr as i64 - next_ip;
                            let dist = offset.abs() as u64;

                            let fits = match temp_size {
                                1 => dist <= i8::MAX as u64,
                                2 => dist <= i16::MAX as u64,
                                4 => dist <= i32::MAX as u64,
                                8 => dist <= i64::MAX as u64,

                                _ => true
                            };

                            if fits {
                                size = *temp_size;
                                break;
                            }
                        }

                        let final_offset = target_addr as i64 - (current_op_pos + size) - 1;

                        match size {
                            1 => { 
                                bytecode.push(match *opcode{
                                    OpCode::Push => OpCode::Push1b,
                                    OpCode::Jmp => OpCode::Jmp1b,
                                    OpCode::Jez => OpCode::Jez1b,
                                    OpCode::Jnz => OpCode::Jnz1b,
                                    

                                    _ => OpCode::None,
                                } as u8);

                                bytecode.push(final_offset as i8 as u8);
                            },
                            2 => {
                                bytecode.push(match *opcode{
                                    OpCode::Push => OpCode::Push2b,
                                    OpCode::Jmp => OpCode::Jmp2b,
                                    OpCode::Jez => OpCode::Jez2b,
                                    OpCode::Jnz => OpCode::Jnz2b,
                                    

                                    _ => OpCode::None,
                                } as u8);

                                bytecode.extend_from_slice(&(final_offset as i16).to_le_bytes());
                            },
                            4 => {
                                bytecode.push(match *opcode{
                                    OpCode::Push => OpCode::Push4b,
                                    OpCode::Jmp => OpCode::Jmp4b,
                                    OpCode::Jez => OpCode::Jez4b,
                                    OpCode::Jnz => OpCode::Jnz4b,
                                    

                                    _ => OpCode::None,
                                } as u8);

                                bytecode.extend_from_slice(&(final_offset as i32).to_le_bytes());
                            },
                            _ => {
                                bytecode.push(match *opcode{
                                    OpCode::Push => OpCode::Push8b,
                                    OpCode::Jmp => OpCode::Jmp8b,
                                    OpCode::Jez => OpCode::Jez8b,
                                    OpCode::Jnz => OpCode::Jnz8b,
                                    

                                    _ => OpCode::None,
                                } as u8);

                                bytecode.extend_from_slice(&final_offset.to_le_bytes());
                            }
                        }

                        continue;
                    }
                }
                
                bytecode.push(*opcode as u8);

                match operand {
                    Operand::None | Operand::Label(_) => {},

                    Operand::IntConstant(val) => {
                        let bytes = val.to_le_bytes().to_vec();
                        bytecode.extend_from_slice(&bytes);
                    }
                }
    
            }
        }

        return bytecode;
    }
}