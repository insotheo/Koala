#ifndef KOALA_VM_VM_DATA_H
#define KOALA_VM_VM_DATA_H

#include <cstdint>
#include <vector>

namespace Koala{
    struct VMData{
        uint32_t MemRequired = 0; //in bytes
        std::vector<uint32_t> Bytecode;
        std::vector<uint64_t> ConstantPool;
    };
    
    struct alignas(16) Instruction{
        const void* Handler;
        int32_t Imm;
        uint8_t Rx;      
        uint8_t Ry;      
        uint8_t Rz;

        uint8_t _padding;
    };

    struct Executable{
        uint64_t MemRequired;
        std::vector<Instruction> Bytecode;
        std::vector<uint64_t> ConstantPool;
    };
}

#endif