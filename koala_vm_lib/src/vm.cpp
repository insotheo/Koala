#include "koala_vm/vm.h"

#include "koala_vm/op_codes.h"
#include <iostream>
#include <type_traits>

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
                    ip += 1;
                    const OP_ARG_TYPE* it = get_const_by_index(const_idx);
                    if(!it) throw std::runtime_error("Invalid constant index");

                    if(std::holds_alternative<int>(*it)){
                        stack.push(std::get<int>(*it));
                    }
                    else {
                        throw std::runtime_error("Unsupported constant type on stack");
                    }
                }
                break;
            }

            case OpCode::OP_RET:{
                ip += 1;
                if (stack.empty()) {
                    return std::nullopt;
                }
                OP_ARG_TYPE val = stack.top();
                stack.pop();
                return val;
                break;
            }

            case OpCode::OP_POP:
                ip += 1;
                stack.pop();
                break;

            case OpCode::OP_POP_N:{
                ip += 1;
                if(CURRENT_INSTR != OpCode::M_CONST){
                    throw std::runtime_error("Uexpected bytecode argument!");
                }
                ip += 1;
                size_t const_idx = m_data.code[ip];
                ip += 1;
                const OP_ARG_TYPE* it = get_const_by_index(const_idx);
                if(!it) throw std::runtime_error("Invalid constant index");

                int num;
                if(!std::holds_alternative<int>(*it)){
                    throw std::runtime_error("Uexpected bytecode argument!");
                }
                num = std::get<int>(*it);
                
                if (num > static_cast<int>(stack.size())) {
                    throw std::runtime_error("POP_N argument exceeds stack size!");
                }

                for(int i = 0; i < num; ++i){
                    stack.pop();
                }

                break;
            }

            case OpCode::OP_JMP: {
                ip += 1;
                size_t target_ip = m_data.code[ip];
                ip = target_ip - 1; //index
                break;
            }

            case OpCode::OP_INC:
            case OpCode::OP_DEC:
            case OpCode::OP_ADD:
            case OpCode::OP_SUB:
            case OpCode::OP_MUL:
            case OpCode::OP_DIV:
                arithmetic(CURRENT_INSTR, stack);
                ip += 1;
                break;

            case OpCode::OP_AND:
            case OpCode::OP_OR:
            case OpCode::OP_XOR:
            case OpCode::OP_NOT:
                logical(CURRENT_INSTR, stack);
                ip += 1;
                break;

        default:
            break;
        }
    }
    return std::nullopt;
}

const OP_ARG_TYPE* KoalaVM::get_const_by_index(size_t idx){
    std::string name = "CONST_" + std::to_string(idx);
    auto it = m_data.constants.find(name);
    if(it == m_data.constants.end()){
        return nullptr;
    }
    return &it->second;
}

void KoalaVM::arithmetic(size_t instr, std::stack<OP_ARG_TYPE>& stack){
    if(instr == OpCode::OP_INC || instr == OpCode::OP_DEC){
        OP_ARG_TYPE a = stack.top(); stack.pop();
        std::visit([&](const auto& val){
            using T = std::decay_t<decltype(val)>;

            if constexpr (std::is_arithmetic_v<T> && !std::is_same_v<T, bool>){
                if (instr == OpCode::OP_INC){
                    stack.push(val + 1);
                }
                else if(instr == OpCode::OP_DEC){
                    stack.push(val - 1);
                }
            }
        }, a);
    }
    else if(instr == OpCode::OP_ADD || instr == OpCode::OP_SUB || instr == OpCode::OP_MUL || instr == OpCode::OP_DIV){
        OP_ARG_TYPE b = stack.top(); stack.pop();
        OP_ARG_TYPE a = stack.top(); stack.pop();

        std::visit([&](const auto& valA, const auto& valB){
            using T_a = std::decay_t<decltype(valA)>;
            using T_b = std::decay_t<decltype(valB)>;

            if constexpr ((std::is_arithmetic_v<T_a> && !std::is_same_v<T_a, bool>) && (std::is_arithmetic_v<T_b> && !std::is_same_v<T_b, bool>)){
                if(instr == OpCode::OP_ADD){
                    stack.push(valA + valB);
                }
                else if(instr == OpCode::OP_SUB){
                    stack.push(valA - valB);
                }
                else if(instr == OpCode::OP_MUL){
                    stack.push(valA * valB);
                }
                else if(instr == OpCode::OP_DIV){
                    stack.push(valA / valB);
                }
            }
        }, a, b);
    }
}

void KoalaVM::logical(size_t instr, std::stack<OP_ARG_TYPE>& stack){
    if(instr == OpCode::OP_NOT){
        OP_ARG_TYPE a = stack.top(); stack.pop();
        std::visit([&](const auto& val){
            using T = std::decay_t<decltype(val)>;
            if constexpr (std::is_arithmetic_v<T>){
                stack.push(!val);
            }
        }, a);
    }
    else if(instr == OpCode::OP_AND || instr == OpCode::OP_OR || instr == OpCode::OP_XOR){
        OP_ARG_TYPE b = stack.top(); stack.pop();
        OP_ARG_TYPE a = stack.top(); stack.pop();
        std::visit([&](const auto& valA, const auto& valB){
            using T_a = std::decay_t<decltype(valA)>;
            using T_b = std::decay_t<decltype(valB)>;

            if constexpr (std::is_arithmetic_v<T_a> && std::is_arithmetic_v<T_b>){
                if(instr == OpCode::OP_AND){
                    stack.push(valA && valB);
                }
                else if(instr == OpCode::OP_OR){
                    stack.push(valA || valB);
                }
                else if(instr == OpCode::OP_XOR){
                    stack.push(valA != valB);
                }
            }
        }, a, b);
    }
}