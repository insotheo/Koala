#include "translator/translator.hpp"

#include <unordered_map>
#include <string>
#include <bit>
#include <array>

namespace koalac{
    Bytecode translateToBytecode(const IRProgram& program){
        Bytecode bc;
        
        std::unordered_map<std::string, size_t> labels;
        size_t bcPtr = 0;
        for(const auto& node : program.GetNodes()){
            if(auto* instr = dynamic_cast<IRInstruction*>(node.get())){
                bcPtr += instr->GetSize();
            } else if(auto* label = dynamic_cast<IRLabel*>(node.get())){
                labels.emplace(label->Label, bcPtr);
            }
        }
        bc.reserve(bcPtr);

        for(const auto& node : program.GetNodes()){
            if(auto* instr = dynamic_cast<IRInstruction*>(node.get())){
                bc.push_back(static_cast<uint8_t>(instr->Op));

                for(const auto& arg : instr->Args){
                    std::visit([&bc](auto val){
                        using T = std::decay_t<decltype(val)>;

                        if(std::is_same_v<T, uint8_t>){
                            bc.push_back(val);
                        } else if(std::is_same_v<T, uint16_t>){
                            auto bytes = std::bit_cast<std::array<uint8_t, sizeof(uint16_t)>>(static_cast<uint16_t>(val));
                            bc.insert(bc.end(), bytes.begin(), bytes.end());
                        } else if(std::is_same_v<T, uint64_t>){
                            auto bytes = std::bit_cast<std::array<uint8_t, sizeof(uint64_t)>>(static_cast<uint64_t>(val));
                            bc.insert(bc.end(), bytes.begin(), bytes.end());
                        }

                    }, arg);
                }
            }
        }

        return std::move(bc);
    }
}