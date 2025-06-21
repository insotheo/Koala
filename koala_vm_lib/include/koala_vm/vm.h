#ifndef KOALA_VM_VM_H
#define KOALA_VM_VM_H

#include <iostream>
#include <optional>
#include <cstdint>
#include "koala_vm/byte_data.h"
#include "koala_vm/vm_config.h"
#include "koala_vm/op_codes.h"

class KoalaVM{
public:
    using HandlerFunc = void (KoalaVM::*)(size_t& ip, std::optional<Value>& ret_val, bool& running);

    explicit KoalaVM(const ProgramData& data) 
    : m_data(data), sp(stack), code(data.code)
    {
        dispatch_table[OpCode::OP_RET] = &KoalaVM::op_ret;
        dispatch_table[OpCode::OP_PUSH] = &KoalaVM::op_push;
        dispatch_table[OpCode::OP_POP] = &KoalaVM::op_pop;
        dispatch_table[OpCode::OP_POP_N] = &KoalaVM::op_pop_n;
        dispatch_table[OpCode::OP_JMP] = &KoalaVM::op_jmp;
        dispatch_table[OpCode::OP_JEZ] = &KoalaVM::op_jez;
        dispatch_table[OpCode::OP_JNZ] = &KoalaVM::op_jnz;
        dispatch_table[OpCode::OP_DUP] = &KoalaVM::op_dup;

        dispatch_table[OpCode::OP_INC] = &KoalaVM::op_inc;
        dispatch_table[OpCode::OP_DEC] = &KoalaVM::op_dec;
        dispatch_table[OpCode::OP_ADD] = &KoalaVM::op_add;
        dispatch_table[OpCode::OP_SUB] = &KoalaVM::op_sub;
        dispatch_table[OpCode::OP_MUL] = &KoalaVM::op_mul;
        dispatch_table[OpCode::OP_DIV] = &KoalaVM::op_div;

        dispatch_table[OpCode::OP_AND] = &KoalaVM::op_and;
        dispatch_table[OpCode::OP_OR]  = &KoalaVM::op_or;
        dispatch_table[OpCode::OP_XOR] = &KoalaVM::op_xor;
        dispatch_table[OpCode::OP_NOT] = &KoalaVM::op_not;
    }
    
    std::optional<Value> run(const std::string& entry_label);

private:
    HandlerFunc dispatch_table[OP_CODE_COUNT];

private:
    const ProgramData m_data;

    Value stack[STACK_MAX];
    Value* sp;
    const Bytecode& code;

    std::optional<Value> execute(size_t begin, size_t end);

private:
    //op's
    void op_push(size_t& ip, std::optional<Value>& ret_val, bool& running);
    void op_dup(size_t& ip, std::optional<Value>& ret_val, bool& running);
    void op_pop(size_t& ip, std::optional<Value>& ret_val, bool& running);
    void op_ret(size_t& ip, std::optional<Value>& ret_val, bool& running);
    void op_pop_n(size_t& ip, std::optional<Value>& ret_val, bool& running);
    void op_jmp(size_t& ip, std::optional<Value>& ret_val, bool& running);
    void op_jez(size_t& ip, std::optional<Value>& ret_val, bool& running);
    void op_jnz(size_t& ip, std::optional<Value>& ret_val, bool& running);

    void op_inc(size_t& ip, std::optional<Value>& ret_val, bool& running);
    void op_dec(size_t& ip, std::optional<Value>& ret_val, bool& running);
    void op_add(size_t& ip, std::optional<Value>& ret_val, bool& running);
    void op_sub(size_t& ip, std::optional<Value>& ret_val, bool& running);
    void op_mul(size_t& ip, std::optional<Value>& ret_val, bool& running);
    void op_div(size_t& ip, std::optional<Value>& ret_val, bool& running);

    void op_and(size_t& ip, std::optional<Value>& ret_val, bool& running);
    void op_or(size_t& ip, std::optional<Value>& ret_val, bool& running);
    void op_xor(size_t& ip, std::optional<Value>& ret_val, bool& running);
    void op_not(size_t& ip, std::optional<Value>& ret_val, bool& running);
};

#endif