#include <VM.h>
#include <OpCodes.h>
#include <Registers.h>

int main(){
    Koala::VMData data = {
        .MemRequired = 0,
        .Bytecode = {
            KOALA_ASM_MOV_IMM(R0, 2),
            KOALA_ASM_MOV_IMM(R1, 2),
            KOALA_ASM_MUL(R1, R1, R1),
            KOALA_ASM_SUB(R0, R0, R1),
            KOALA_ASM_MOV(R10, R0),
            Koala::RET,
        },
        .ConstantPool = {}
    };

    Koala::KoalaVM vm;
    
    vm.Run(data);

    return 0;
}