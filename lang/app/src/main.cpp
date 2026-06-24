#include <VM.h>
#include <OpCodes.h>
#include <Registers.h>
#include <bit>

int main(){
    Koala::VMData data = {
        .MemRequired = 0,
        .Bytecode = {
            KOALA_ASM_LOAD_CONST(R0, 0),
            KOALA_ASM_LOAD_CONST(R1, 1),
            KOALA_ASM_ADD_F(R2, R0, R1),
            KOALA_ASM_MUL_F(R3, R0, R0),
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