#include "parser/translator.h"

#include <cstdint>
#include "koala_vm/op_codes.h"

ByteData translate(const std::vector<CodeBlock>& blocks){
    ByteData byte_data;

    std::unordered_map<std::string, size_t> label_to_offset;
    std::unordered_map<OP_ARG_TYPE, size_t> const_index_map;

    size_t const_counter = 0;
    size_t code_offset = 0;

    //calculating offset
    for(const auto& block : blocks){
        label_to_offset[block.label] = code_offset;

        for(const auto& instr : block.block_instructions){
            code_offset += 1;

            for(const auto& op : instr.operands){
                if(std::holds_alternative<int>(op)){ //M_CONST <const>
                    code_offset += 2;
                }
                ///TODO: other types
            }
        } 
    }

    //gen bytecode
    for(const auto& block : blocks){
        for(const auto& instr : block.block_instructions){
            byte_data.code.push_back(instr.op_code);

            for(const auto& op : instr.operands){
                if(std::holds_alternative<int>(op)){
                    //constant
                    auto it = const_index_map.find(op);
                    if(it == const_index_map.end()){
                        const_counter += 1;
                        size_t idx = const_counter;

                        const_index_map[op] = idx;
                        std::string title = "CONST_" + std::to_string(idx);
                        byte_data.constants[title] = op;

                        byte_data.code.push_back(OpCode::M_CONST);
                        byte_data.code.push_back(idx);
                    }
                    else{
                        byte_data.code.push_back(OpCode::M_CONST);
                        byte_data.code.push_back(it->second);
                    }
                }
                ///TODO: label operands
            }
        }
    }

    return byte_data;
}