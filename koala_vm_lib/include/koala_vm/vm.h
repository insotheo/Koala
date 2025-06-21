#ifndef KOALA_VM_VM_H
#define KOALA_VM_VM_H

#include <iostream>
#include <optional>
#include <cstdint>
#include "koala_vm/byte_data.h"
#include "koala_vm/vm_config.h"
#include "koala_vm/op_codes.h"

#define HANDLER_FUNC_ARGS size_t& ip, bool& running, std::optional<Value>& ret_val

class KoalaVM{
public:
    using HandlerFunc = void (KoalaVM::*)(HANDLER_FUNC_ARGS);

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
    
    std::optional<Value> run();

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
    void op_push(HANDLER_FUNC_ARGS);
    void op_dup(HANDLER_FUNC_ARGS);
    void op_pop(HANDLER_FUNC_ARGS);
    void op_ret(HANDLER_FUNC_ARGS);
    void op_pop_n(HANDLER_FUNC_ARGS);
    void op_jmp(HANDLER_FUNC_ARGS);
    void op_jez(HANDLER_FUNC_ARGS);
    void op_jnz(HANDLER_FUNC_ARGS);

    void op_inc(HANDLER_FUNC_ARGS);
    void op_dec(HANDLER_FUNC_ARGS);
    void op_add(HANDLER_FUNC_ARGS);
    void op_sub(HANDLER_FUNC_ARGS);
    void op_mul(HANDLER_FUNC_ARGS);
    void op_div(HANDLER_FUNC_ARGS);

    void op_and(HANDLER_FUNC_ARGS);
    void op_or(HANDLER_FUNC_ARGS);
    void op_xor(HANDLER_FUNC_ARGS);
    void op_not(HANDLER_FUNC_ARGS);
};

#endif