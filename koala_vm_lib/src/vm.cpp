#include "koala_vm/vm.h"

#include "koala_vm/op_codes.h"
#include <iostream>

std::optional<OP_ARG_TYPE> KoalaVM::run(const std::string& entry_label){
    std::stack<OP_ARG_TYPE> stack;

    auto it = m_data.blocks.find(entry_label);
    if(it == m_data.blocks.end()){
        throw std::runtime_error("Unknown entry label: " + entry_label);
    }

    size_t entry_begin = it->second.begin;
    size_t entry_end = it->second.end;

    return execute(entry_begin, entry_end, stack);
}

#define CURRENT_INSTR m_data.code[ip]
std::optional<OP_ARG_TYPE> KoalaVM::execute(size_t begin, size_t end, std::stack<OP_ARG_TYPE>& stack){
    int ip = begin;

    while(ip < m_data.code.size() && ip < end){
        switch (CURRENT_INSTR)
        {
            case OpCode::OP_PUSH:{
                ip += 1;
                if(CURRENT_INSTR == OpCode::M_CONST){
                    ip += 1;
                    size_t const_idx = m_data.code[ip];
                    const OP_ARG_TYPE* it = get_const_by_index(const_idx);
                    if(!it) throw std::runtime_error("Invalid constant index");

                    if(std::holds_alternative<int>(*it)){
                        stack.push(std::get<int>(*it));
                    }
                    else {
                        throw std::runtime_error("Unsupported constant type on stack");
                    }
                }
            }

            case OpCode::OP_RET:{
                ip += 1;
                size_t ret_count = m_data.code[ip];

                if (ret_count == 0)
                    return std::nullopt;
                
                else{
                    if (stack.empty()) {
                        return std::nullopt;
                    }
                    OP_ARG_TYPE val = stack.top();
                    stack.pop();
                    return val;
                }
            }
        
        default:
            break;
        }
    }
    return OP_ARG_TYPE();
}

const OP_ARG_TYPE* KoalaVM::get_const_by_index(size_t idx){
    std::string name = "CONST_" + std::to_string(idx);
    auto it = m_data.constants.find(name);
    if(it == m_data.constants.end()){
        return nullptr;
    }
    return &it->second;
}