#include "koala_vm/vm.h"

#define HOLDS_NUMERIC(VAL) (std::holds_alternative<int>(VAL))

void KoalaVM::op_push(HANDLER_FUNC_ARGS){
    uint8_t idx = code[ip++];
    *sp++ = m_data.constants[idx];
}

void KoalaVM::op_dup(HANDLER_FUNC_ARGS){
    sp[0] = sp[-1];
    sp++;
}

void KoalaVM::op_pop(HANDLER_FUNC_ARGS){
    sp[-1] = {};
    --sp;
}

void KoalaVM::op_ret(HANDLER_FUNC_ARGS){
    ret_val = *--sp;
    running = false;
}

void KoalaVM::op_pop_n(HANDLER_FUNC_ARGS){
    uint8_t const_idx = code[ip++];
    const Value& val = m_data.constants[const_idx];

    if (!HOLDS_NUMERIC(val)){
        throw std::runtime_error("Expected integer constant for POP_N");
    }
    int num = std::get<int>(val);
                
    if (num > static_cast<int>(sp - stack)) {
        throw std::runtime_error("POP_N argument exceeds stack size");
    }

    for(int i = 1; i < num; ++i){
        sp[-i] = {};
    }
    sp -= num;
}

void KoalaVM::op_jmp(HANDLER_FUNC_ARGS){
    size_t target = code[ip++];
    ip = target - 1;
}

void KoalaVM::op_jez(HANDLER_FUNC_ARGS){
    uint8_t target = code[ip++];
    const Value& top = sp[-1];
    if(HOLDS_NUMERIC(top) && std::get<int>(top) == 0){ ///TODO: FIX!
        ip = target - 1;
    }
}

void KoalaVM::op_jnz(HANDLER_FUNC_ARGS){
    uint8_t target = code[ip++];
    const Value& top = sp[-1];
    if(HOLDS_NUMERIC(top) && std::get<int>(top) != 0){ ///TODO: FIX!
        ip = target - 1;
    }
}

void KoalaVM::op_inc(HANDLER_FUNC_ARGS){
    Value& val = sp[-1];
    if(std::holds_alternative<int>(val)){
        val = std::get<int>(val) + 1;
    }
}

void KoalaVM::op_dec(HANDLER_FUNC_ARGS){
    Value& val = sp[-1];
    if(std::holds_alternative<int>(val)){
        val = std::get<int>(val) - 1;
    }
}

void KoalaVM::op_add(HANDLER_FUNC_ARGS){
    int b = std::get<int>(*--sp);
    int a = std::get<int>(*--sp);
    *sp++ = (a + b);
}

void KoalaVM::op_sub(HANDLER_FUNC_ARGS){
    int b = std::get<int>(*--sp);
    int a = std::get<int>(*--sp);
    *sp++ = (a - b);
}

void KoalaVM::op_mul(HANDLER_FUNC_ARGS){
    int b = std::get<int>(*--sp);
    int a = std::get<int>(*--sp);
    *sp++ = (a * b);
}

void KoalaVM::op_div(HANDLER_FUNC_ARGS){
    int b = std::get<int>(*--sp);
    int a = std::get<int>(*--sp);
    *sp++ = (a / b);
}

void KoalaVM::op_and(HANDLER_FUNC_ARGS){
    int b = std::get<int>(*--sp);
    int a = std::get<int>(*--sp);
    *sp++ = (a && b);
}

void KoalaVM::op_or(HANDLER_FUNC_ARGS){
    int b = std::get<int>(*--sp);
    int a = std::get<int>(*--sp);
    *sp++ = (a || b);
}

void KoalaVM::op_xor(HANDLER_FUNC_ARGS){
    int b = std::get<int>(*--sp);
    int a = std::get<int>(*--sp);
    *sp++ = (a != b);
}

void KoalaVM::op_not(HANDLER_FUNC_ARGS){
    int a = std::get<int>(*--sp);
    *sp++ = (!a);
}