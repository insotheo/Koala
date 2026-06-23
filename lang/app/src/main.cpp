#include <VM.h>
#include <OpCodes.h>
#include <Registers.h>

int main(){
    Koala::VMData data = {
        .MemRequired = 0,
        .Bytecode = {
            KOALA_ASM_I(MOV_IMM, R0, 11),
            KOALA_ASM_I(MOV_IMM, R3, 1 << 7),
            KOALA_ASM_I(LOAD_CONST, R1, 0),
            Koala::RET,
        },
        .ConstantPool = {
            1 << 10
        }
    };

    Koala::KoalaVM vm;
    
    vm.Run(data);

    return 0;
}