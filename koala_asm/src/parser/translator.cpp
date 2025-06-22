#include "parser/translator.h"

#include <iostream>
#include <cstdint>
#include <unordered_map>
#include "koala_vm/op_codes.h"

ProgramData translate(const std::vector<CodeBlock>& blocks, const std::string& entry_name){
    ProgramData byte_data;

    std::unordered_map<std::string, size_t> label_to_offset;
    std::unordered_map<std::string, size_t> label_to_index;
    std::unordered_map<Value, size_t> const_index_map;

    size_t const_counter = 0;
    size_t code_offset = 0;
    bool found_entry_point = false;

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
        if(block.label == entry_name){
            byte_data.blocks.insert(byte_data.blocks.begin(), byte_block);
            for(auto& [label, index] : label_to_index){
                if(index == 0){
                    index = byte_data.blocks.size() - 1;
                    break;
                }
            }
            label_to_index[entry_name] = 0;
            found_entry_point = true;
        }
        else{
            byte_data.blocks.push_back(byte_block);
            label_to_index[block.label] = byte_data.blocks.size() - 1;
        }
    }

    if(!found_entry_point){
        throw std::runtime_error("No entry point found!");
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
            if(instr.op_code == OpCode::OP_CALL){
                std::string label = std::get<std::string>(instr.operands[0]);
                auto it = label_to_index.find(label);
                if(it == label_to_index.end()){
                    throw std::runtime_error("Label " + label + " doesn't exist in current context!");
                }
                int idx = it->second;
                byte_data.code.push_back(idx);
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