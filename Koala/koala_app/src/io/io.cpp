#include "io.h"

#include <iostream>
#include <fstream>
#include "KoalaLang/KoalaLangVersionInfo.h"

void WriteBytecodeToFile(const KoalaByte::Bytecode& bytecode, const std::string& output_path){
    if(bytecode.GetCode().size() == 0) return;
    
    std::ofstream outFile(output_path, std::ios::binary);

    if(!outFile){
        std::cerr << "Error opening file: " << output_path << "\n";
        std::exit(-1);
    }

    //header
    const char koala_name[] = "KOALA";
    int version = KOALA_LANG_MAJOR_VERSION;
    outFile.write(koala_name, sizeof(koala_name) - 1);
    outFile.write(reinterpret_cast<const char*>(&version), sizeof(version));

    //regions
    size_t regionsCount = bytecode.GetRegions().size();
    outFile.write(reinterpret_cast<const char*>(&regionsCount), sizeof(regionsCount));
    outFile.write(reinterpret_cast<const char*>(bytecode.GetRegions().data()), sizeof(bytecode.GetRegions().data()));

    //constants
    size_t constantsCount = bytecode.GetConstants().size();
    outFile.write(reinterpret_cast<const char*>(&constantsCount), sizeof(constantsCount));

    for(const auto& constant : bytecode.GetConstants()){
        size_t size = constant.size();
        outFile.write(reinterpret_cast<const char*>(&size), sizeof(size)); //volume of data
        outFile.write(reinterpret_cast<const char*>(constant.data()), sizeof(constant.data())); //data
    }

    //bytecode
    size_t bytesCount = bytecode.GetCode().size();
    outFile.write(reinterpret_cast<const char*>(&bytesCount), sizeof(bytesCount));
    outFile.write(reinterpret_cast<const char*>(bytecode.GetCode().data()), sizeof(bytecode.GetCode().data()));

    outFile.close();
}