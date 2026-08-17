#include "ir.hpp"

namespace koalac{

    size_t IRInstruction::GetSize(){
        size_t size = 1; //opcode is always 1 byte

        for(const auto& arg : Args){
            std::visit([&size](auto val){
                using T = std::decay_t<decltype(val)>;
                
                if(std::is_same_v<T, uint8_t>){
                    size += 1;
                } else if(std::is_same_v<T, uint16_t>) {
                    size += 2;
                } else if(std::is_same_v<T, uint64_t>) {
                    size += 8;
                }

            }, arg);
        }

        return size;
    }

}