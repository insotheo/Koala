#include "parser/translator.h"

#include <iostream>
#include <cstdint>
#include "koala_vm/op_codes.h"

ProgramData translate(const std::vector<CodeBlock>& blocks){
    ProgramData byte_data;

    std::unordered_map<std::string, size_t> label_to_offset;
    std::unordered_map<Value, size_t> const_index_map;

    size_t const_counter = 0;
    size_t code_offset = 0;

    //calculating offset
    for(const auto& block : blocks){
        Block byte_block;
        label_to_offset[block.label] = code_offset;
        byte_block.begin = code_offset;

        for(const auto& instr : block.block_instructions){
            code_offset += 1;

            if(instr.op_code == OpCode::PC_MARK){
                std::string label = std::get<std::string>(instr.operands[0]);
                label_to_offset[block.label + "::" + label] = code_offset;
                code_offset -= 1;
                continue;
            }

            for(const auto& op : instr.operands){
                code_offset += 1;
            }
        }
        byte_block.end = code_offset;
        byte_data.blocks[block.label] = byte_block;
    }

    //gen bytecode
    for(const auto& block : blocks){
        for(const auto& instr : block.block_instructions){
            if (instr.op_code == OpCode::PC_MARK) {
                continue;
            }

            byte_data.code.push_back(instr.op_code);

            if(instr.op_code == OpCode::OP_JMP || instr.op_code == OpCode::OP_JEZ || instr.op_code == OpCode::OP_JNZ){
                std::string label = std::get<std::string>(instr.operands[0]);
                auto it = label_to_offset.find(block.label + "::" + label);
                if(it == label_to_offset.end()){
                    throw std::runtime_error("Label " + label + " doesn't exist in current context(" + (block.label + "::" + label) + ")!");
                }
                int offset = it->second;
                byte_data.code.push_back(offset);
                continue;
            }

            for(const auto& op : instr.operands){
                if(std::holds_alternative<int>(op)){
                    //constant
                    auto it = const_index_map.find(op);
                    if(it == const_index_map.end()){
                        const_counter += 1;
                        size_t idx = const_counter - 1;
                        const_index_map[op] = idx;
                        byte_data.constants.push_back(op);
                        byte_data.code.push_back(idx);
                    }
                    else{
                        byte_data.code.push_back(it->second);
                    }
                }
            }
        }
    }

    return byte_data;
}