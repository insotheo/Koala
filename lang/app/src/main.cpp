#include <VM.h>
#include <OpCodes.h>
#include <Registers.h>
#include <bit>

int main(){
    Koala::VMData data = {
        .MemRequired = 0,
        .Bytecode = {
            KOALA_ASM_MOV_IMM(R0, 10),
            KOALA_ASM_MOV_IMM(R1, 5),
            KOALA_ASM_JMP(1),
            KOALA_ASM_MOV_IMM(R0, 5),
            KOALA_ASM_CMP_EQ(R0, R1),
            KOALA_ASM_JEZ((1 << 24) - 3),
            KOALA_ASM_RET(),
        },
        .ConstantPool = {
            std::bit_cast<uint64_t>(3.14),
            std::bit_cast<uint64_t>(2.71),
        }
    };

    Koala::KoalaVM vm;
    
    vm.Run(data);

    return 0;
}