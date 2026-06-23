#include <VM.h>
#include <OpCodes.h>
#include <Registers.h>

int main(){
    Koala::VMData data = {
        .MemRequired = 0,
        .Bytecode = {
            KOALA_ASM_I(MOV_IMM, R0, 2),
            KOALA_ASM_I(MOV_IMM, R1, 2),
            KOALA_ASM_R(MUL, R1, R1, R1),
            KOALA_ASM_R(ADD, R0, R0, R1),
            Koala::RET,
        },
        .ConstantPool = {}
    };

    Koala::KoalaVM vm;
    
    vm.Run(data);

    return 0;
}