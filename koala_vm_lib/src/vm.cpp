#include "koala_vm/vm.h"

#include "koala_vm/op_codes.h"

std::optional<Value> KoalaVM::run(){
    const Block& entry_point = m_data.blocks[0];
    return execute(entry_point.begin, entry_point.end);
}

std::optional<Value> KoalaVM::execute(size_t begin, size_t end){
    size_t ip = begin;
    std::optional<Value> ret_val = std::nullopt;
    bool running = true;
    
    while(ip < end && running){
        uint8_t opcode = code[ip++];
        HandlerFunc handler = dispatch_table[opcode];

        if(!handler){
            throw std::runtime_error("Unknown or unimplemented opcode");
        }

        (this->*handler)(ip, running, ret_val);
    }

    return ret_val;
}