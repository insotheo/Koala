#ifndef KOALA_VM_VM_DATA_H
#define KOALA_VM_VM_DATA_H

#include <cstdint>
#include <vector>

namespace Koala{
    struct VMData{
        uint32_t MemRequired = 0; //in bytes
        std::vector<uint32_t> Bytecode;
        // std::vector<uint64_t> ConstantPool;
    };
}

#endif