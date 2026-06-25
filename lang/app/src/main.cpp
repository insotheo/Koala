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
                KOALA_ASM_MOV_IMM(R0, 1),
                KOALA_ASM_MOV_IMM(R1, 6),
                KOALA_ASM_SHL(R0, R0, R1),
                KOALA_ASM_RET(),
            },
            .ConstantPool = { }
        };
        
        vm.Run(&data, &exec);
    }
    
    auto t1 = std::chrono::high_resolution_clock::now();
    vm.Run(nullptr, &exec);
    auto t2 = std::chrono::high_resolution_clock::now();
    std::cout << "Execution time: " << std::chrono::duration<double>(t2 - t1) << "\n";

    return 0;
}