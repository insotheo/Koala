#include <VM.h>
#include <OpCodes.h>
#include <Registers.h>
#include <bit>

int main(){
    Koala::VMData data = {
        .MemRequired = 0,
        .Bytecode = {
            KOALA_ASM_MOV_IMM(R0, 10),
            KOALA_ASM_CONV_UI2F(R1, R0),
            KOALA_ASM_LOAD_CONST(R0, 0),
            KOALA_ASM_MUL_F(R0, R0, R1),
            KOALA_ASM_CONV_F2SI(R0, R0),
            KOALA_ASM_MOV(R1, R0),
            Koala::RET,
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