#include "translator/translator.hpp"

#include <unordered_map>
#include <string>
#include <bit>
#include <array>
#include <format>

namespace koalac{
    Bytecode translateToBytecode(IRProgram& program){
        std::unordered_map<std::string, size_t> labelPositions;
        bool sizeChanged = true;
        size_t bcSize = 0;
        
        while(sizeChanged){
            sizeChanged = false;
            labelPositions.clear();
            size_t bcPtr = 0;

            //calc tentative pos
            for(const auto& node : program.GetNodes()){
                if(auto* instr = dynamic_cast<IRInstruction*>(node.get())){
                    if(instr->Op == OpCode::_JMP_UNDEFINED ||
                       instr->Op == OpCode::_JEZ_UNDEFINED ||
                       instr->Op == OpCode::_JNZ_UNDEFINED
                    ){
                        instr->Op = static_cast<OpCode>(static_cast<uint8_t>(instr->Op) + 1); //_SHORT is always + 1 after _UNDEFINED
                    }
                    bcPtr += instr->GetSize();
                } else if(auto* label = dynamic_cast<IRLabel*>(node.get())){
                    labelPositions[label->Label] = bcPtr;
                }
            }
            bcSize = bcPtr;

            //validate constraits and upgrade
            bcPtr = 0;
            for(const auto& node : program.GetNodes()){
                if(auto* instr = dynamic_cast<IRInstruction*>(node.get())){
                    size_t currInstrSize = instr->GetSize();

                    if(instr->Op == OpCode::JMP_SHORT ||
                        instr->Op == OpCode::JEZ_SHORT ||
                        instr->Op == OpCode::JNZ_SHORT
                    ){
                        for(const auto& arg : instr->Args){
                            if(std::holds_alternative<std::string>(arg)){
                                const std::string& target = std::get<std::string>(arg);

                                if(labelPositions.find(target) == labelPositions.end()){
                                    std::runtime_error(std::format("Compilation failed with fatal error: label '{}' not found", target));
                                }

                                size_t targetPos = labelPositions[target];
                                size_t nextInstrPos = bcPtr + currInstrSize;

                                int64_t relOffset = static_cast<int64_t>(targetPos) - static_cast<int64_t>(nextInstrPos);

                                if(relOffset < INT16_MIN || relOffset > INT16_MAX){
                                    instr->Op = static_cast<OpCode>(static_cast<uint8_t>(instr->Op) + 1); //_LONG is always + 1 after _SHORT
                                    sizeChanged = true;
                                }

                                break;
                            }
                        }
                    }

                    bcPtr += currInstrSize;
                }
            }
        }

        //bytecode generating
        Bytecode bc;
        bc.reserve(bcSize);
        size_t bcPtr = 0;

        for(const auto& node : program.GetNodes()){
            if(auto* instr = dynamic_cast<IRInstruction*>(node.get())){
                size_t instrSize = instr->GetSize();
                
                bc.push_back(static_cast<uint8_t>(instr->Op));

                for(const auto& arg : instr->Args){
                    std::visit([&](auto val){
                        using T = std::decay_t<decltype(val)>;

                        if constexpr (std::is_same_v<T, uint8_t>){
                            bc.push_back(val);
                        } else if constexpr (std::is_same_v<T, uint16_t>){
                            auto bytes = std::bit_cast<std::array<uint8_t, sizeof(uint16_t)>>(static_cast<uint16_t>(val));
                            bc.insert(bc.end(), bytes.begin(), bytes.end());
                        } else if constexpr (std::is_same_v<T, uint64_t>){
                            auto bytes = std::bit_cast<std::array<uint8_t, sizeof(uint64_t)>>(static_cast<uint64_t>(val));
                            bc.insert(bc.end(), bytes.begin(), bytes.end());
                        } else if constexpr (std::is_same_v<T, std::string>){
                            size_t targetPos = labelPositions[val];
                            size_t nextInstrPos = bcPtr + instrSize;
                            int64_t relOffset = static_cast<int64_t>(targetPos) - static_cast<int64_t>(nextInstrPos);

                            if(instr->Op == OpCode::JMP_SHORT ||
                               instr->Op == OpCode::JEZ_SHORT ||
                               instr->Op == OpCode::JNZ_SHORT
                            ){
                                int16_t offset16 = static_cast<int16_t>(relOffset);
                                auto bytes = std::bit_cast<std::array<uint8_t, sizeof(int16_t)>>(offset16);
                                bc.insert(bc.end(), bytes.begin(), bytes.end());
                            } else if(instr->Op == OpCode::JMP_LONG ||
                                      instr->Op == OpCode::JEZ_LONG ||
                                      instr->Op == OpCode::JNZ_LONG
                            ){
                                int64_t offset64 = static_cast<int64_t>(relOffset);
                                auto bytes = std::bit_cast<std::array<uint8_t, sizeof(int64_t)>>(offset64);
                                bc.insert(bc.end(), bytes.begin(), bytes.end());
                            }

                        }

                    }, arg);
                }

                bcPtr += instrSize;
            }
        }

        return std::move(bc);
    }
}