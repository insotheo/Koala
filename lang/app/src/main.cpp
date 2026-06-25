#include <VM.h>
#include <OpCodes.h>
#include <Registers.h>
#include <iostream>
#include <chrono>

int main(){
    Koala::KoalaVM vm;
    Koala::Executable exec;

    {//VMData(provided by compiler) -> Executable
        Koala::VMData data = {
            .MemRequired = 0,
            .Bytecode = {
                KOALA_ASM_LOAD_CONST(R0, 0),
                KOALA_ASM_MOV_IMM(R1, 1),
                KOALA_ASM_MOV_IMM(R2, 0),

                KOALA_ASM_SUB(R0, R0, R1),  

                KOALA_ASM_CMP_EQ(R0, R2),
                KOALA_ASM_JEZ((1 << 24) - 2),
                KOALA_ASM_RET(),
            },
            .ConstantPool = {
                1000000000
            }
        };
        
        vm.Run(&data, &exec);
    }
    
    auto t1 = std::chrono::high_resolution_clock::now();
    vm.Run(nullptr, &exec);
    auto t2 = std::chrono::high_resolution_clock::now();
    std::cout << "Execution time: " << std::chrono::duration<double>(t2 - t1) << "\n";

    return 0;
}