#include <iostream>
#include <fstream>
#include <vector>
#include <cstdint>
#include <KoalaCore>

std::vector<uint8_t> readBytecode(char* filepath){
    std::ifstream fs(filepath, std::ios::ate | std::ios::binary);
    if(!fs){
        std::cerr << "Failed to open file: " << filepath << "\n";
        return {};
    }

    std::streamsize size = fs.tellg();
    if(size < 5){
        return {};
    }

    fs.seekg(0, std::ios::beg);

    uint8_t magicHeader[5];
    if(!fs.read(reinterpret_cast<char*>(magicHeader), 5)){
        std::cerr << "Failed to read file header.\n";
        return {};
    }
    
    if(magicHeader[0] != KOALA_MAG_0 ||
        magicHeader[1] != KOALA_MAG_1 ||
        magicHeader[2] != KOALA_MAG_2 ||
        magicHeader[3] != KOALA_MAG_3 ||
        magicHeader[4] != KOALA_MAG_4){
        std::cerr << "Error: Invalid magic bytes! Not a Koala Bytecode binary.\n";
        return {};
    }

    if(static_cast<size_t>(size) < 5){
        return {};
    }

    size_t bodySize = static_cast<size_t>(size) - 5;
    std::vector<uint8_t> bytecode(bodySize);
    if(!fs.read(reinterpret_cast<char*>(bytecode.data()), bodySize)){
        std::cerr << "Failed to read file.\n";
        return {};
    }

    fs.close();

    return std::move(bytecode);
}

void printHelp(){
    std::cout << R"(=====Koala Virtual Machine=====
Version 0.0.1
Core Version: )" << KOALA_CORE_VERSION
<< R"(

Syntax:
koala <path_to_koala_bytecode.klbc>
)";
}

int main(int argc, char** argv){
    if(argc != 2){
        printHelp();
    }

    std::vector<uint8_t> bytecode = readBytecode(argv[1]);
    if(bytecode.empty()){
        std::cerr << "Bytecode is empty.\n";
        return -1;
    }
    
    koalaVMRun(bytecode.data());

    return 0;
}