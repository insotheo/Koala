//*.asm.kls
//kls stands for Koala Source

#include <iostream>


int main(int argc, char** argv){
    if(argc == 2){
        std::cout << argv[1] << "\n";
    }
    return 0;
}