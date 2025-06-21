#include "koala_vm/vm.h"

#define HOLDS_NUMERIC(VAL) (std::holds_alternative<int>(VAL))

void KoalaVM::op_push(size_t& ip, std::optional<Value>& ret_val, bool& running){
    uint8_t idx = code[ip++];
    *sp++ = m_data.constants[idx];
}

void KoalaVM::op_dup(size_t& ip, std::optional<Value>& ret_val, bool& running){
    sp[0] = sp[-1];
    sp++;
}

void KoalaVM::op_pop(size_t& ip, std::optional<Value>& ret_val, bool& running){
    sp[-1] = {};
    --sp;
}

void KoalaVM::op_ret(size_t& ip, std::optional<Value>& ret_val, bool& running){
    ret_val = *--sp;
    running = false;
}

void KoalaVM::op_pop_n(size_t& ip, std::optional<Value>& ret_val, bool& running){
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

void KoalaVM::op_jmp(size_t& ip, std::optional<Value>& ret_val, bool& running){
    size_t target = code[ip++];
    ip = target - 1;
}

void KoalaVM::op_jez(size_t& ip, std::optional<Value>& ret_val, bool& running){
    uint8_t target = code[ip++];
    const Value& top = sp[-1];
    if(HOLDS_NUMERIC(top) && std::get<int>(top) == 0){ ///TODO: FIX!
        ip = target - 1;
    }
}

void KoalaVM::op_jnz(size_t& ip, std::optional<Value>& ret_val, bool& running){
    uint8_t target = code[ip++];
    const Value& top = sp[-1];
    if(HOLDS_NUMERIC(top) && std::get<int>(top) != 0){ ///TODO: FIX!
        ip = target - 1;
    }
}

void KoalaVM::op_inc(size_t& ip, std::optional<Value>& ret_val, bool& running){
    Value& val = sp[-1];
    if(std::holds_alternative<int>(val)){
        val = std::get<int>(val) + 1;
    }
}

void KoalaVM::op_dec(size_t& ip, std::optional<Value>& ret_val, bool& running){
    Value& val = sp[-1];
    if(std::holds_alternative<int>(val)){
        val = std::get<int>(val) - 1;
    }
}

void KoalaVM::op_add(size_t& ip, std::optional<Value>& ret_val, bool& running){
    int b = std::get<int>(*--sp);
    int a = std::get<int>(*--sp);
    *sp++ = (a + b);
}

void KoalaVM::op_sub(size_t& ip, std::optional<Value>& ret_val, bool& running){
    int b = std::get<int>(*--sp);
    int a = std::get<int>(*--sp);
    *sp++ = (a - b);
}

void KoalaVM::op_mul(size_t& ip, std::optional<Value>& ret_val, bool& running){
    int b = std::get<int>(*--sp);
    int a = std::get<int>(*--sp);
    *sp++ = (a * b);
}

void KoalaVM::op_div(size_t& ip, std::optional<Value>& ret_val, bool& running){
    int b = std::get<int>(*--sp);
    int a = std::get<int>(*--sp);
    *sp++ = (a / b);
}

void KoalaVM::op_and(size_t& ip, std::optional<Value>& ret_val, bool& running){
    int b = std::get<int>(*--sp);
    int a = std::get<int>(*--sp);
    *sp++ = (a && b);
}

void KoalaVM::op_or(size_t& ip, std::optional<Value>& ret_val, bool& running){
    int b = std::get<int>(*--sp);
    int a = std::get<int>(*--sp);
    *sp++ = (a || b);
}

void KoalaVM::op_xor(size_t& ip, std::optional<Value>& ret_val, bool& running){
    int b = std::get<int>(*--sp);
    int a = std::get<int>(*--sp);
    *sp++ = (a != b);
}

void KoalaVM::op_not(size_t& ip, std::optional<Value>& ret_val, bool& running){
    int a = std::get<int>(*--sp);
    *sp++ = (!a);
}