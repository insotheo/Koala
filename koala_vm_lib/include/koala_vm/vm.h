#ifndef KOALA_VM_VM_H
#define KOALA_VM_VM_H

#include <stack>
#include <optional>
#include "koala_vm/byte_data.h"

class KoalaVM{
public:
    KoalaVM(const ByteData& data) : m_data(data)
    {}
    
    std::optional<OP_ARG_TYPE> run(const std::string& entry_label);
private:
    const ByteData m_data;

    const OP_ARG_TYPE* get_const_by_index(size_t idx);
    std::optional<OP_ARG_TYPE> execute(size_t begin, size_t end, std::stack<OP_ARG_TYPE>& stack);

    void arithmetic(size_t instr, std::stack<OP_ARG_TYPE>& stack);
};

#endif