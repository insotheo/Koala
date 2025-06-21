#include "koala_vm/vm.h"

#include "koala_vm/op_codes.h"

std::optional<Value> KoalaVM::run(const std::string& entry_label){
    auto it = m_data.blocks.find(entry_label);
    if(it == m_data.blocks.end()){
        throw std::runtime_error("Unknown entry label: " + entry_label);
    }

    return execute(it->second.begin, it->second.end);
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

        (this->*handler)(ip, ret_val, running);
    }

    return ret_val;
}