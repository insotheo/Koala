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
    outFile.write(koala_name, 5);
    outFile.write(reinterpret_cast<const char*>(&version), sizeof(version));

    //regions
    const auto& regions = bytecode.GetRegions();
    uint64_t regionsCount = static_cast<uint64_t>(regions.size());
    outFile.write(reinterpret_cast<const char*>(&regionsCount), sizeof(regionsCount));
    for (auto r : regions) {
        uint64_t tmp = static_cast<uint64_t>(r);
        outFile.write(reinterpret_cast<const char*>(&tmp), sizeof(tmp));
    }

    //constants
    const auto& constants = bytecode.GetConstants();
    uint64_t constantsCount = static_cast<uint64_t>(constants.size());
    outFile.write(reinterpret_cast<const char*>(&constantsCount), sizeof(constantsCount));
    for(const auto& constant : bytecode.GetConstants()) {
        uint64_t size = static_cast<uint64_t>(constant.size());
        outFile.write(reinterpret_cast<const char*>(&size), sizeof(size)); //volume of data
        if (size) outFile.write(reinterpret_cast<const char*>(constant.data()), static_cast<std::streamsize>(size)); //data
    }

    //bytecode
    const auto& code = bytecode.GetCode();
    uint64_t bytesCount = static_cast<uint64_t>(code.size());
    outFile.write(reinterpret_cast<const char*>(&bytesCount), sizeof(bytesCount));
    if (bytesCount) outFile.write(reinterpret_cast<const char*>(code.data()), static_cast<std::streamsize>(bytesCount * sizeof(uint64_t)));

    if (!outFile) {
        std::cerr << "Error occurred while writing file: " << output_path << "\n";
        std::exit(-1);
    }

    outFile.close();
}

KoalaByte::Bytecode ReadBytecodeFromFile(const std::string &input_path) {
    std::ifstream inFile(input_path, std::ios::binary);
    if(!inFile) {
        std::cerr << "Error opening file: " << input_path << "\n";
        std::exit(-1);
    }

    //header
    char magic[5];
    inFile.read(magic,5);
    int version = -1;
    inFile.read(reinterpret_cast<char*>(&version), sizeof(version));
    if (std::string(magic, 5) != "KOALA" || version != KOALA_LANG_MAJOR_VERSION) {
        std::cerr << "Invalid file or version missmatch!\n";
        std::exit(-1);
    }

    //regions
    uint64_t regionsCount = 0;
    inFile.read(reinterpret_cast<char*>(&regionsCount), sizeof(regionsCount));
    std::vector<uint64_t> regions;
    regions.reserve(static_cast<size_t>(regionsCount));
    for (uint64_t i = 0; i < regionsCount; ++i) {
        uint64_t tmp = 0;
        inFile.read(reinterpret_cast<char*>(&tmp), sizeof(tmp));
        regions.push_back(tmp);
    }

    //constants
    uint64_t constantsCount = 0;
    inFile.read(reinterpret_cast<char*>(&constantsCount), sizeof(constantsCount));
    std::vector<ByteData_t> constants;
    constants.reserve(static_cast<size_t>(constantsCount));
    for (uint64_t i = 0; i < constantsCount; ++i) {
        uint64_t size = 0;
        inFile.read(reinterpret_cast<char*>(&size), sizeof(size));

        ByteData_t buffer;
        if (size) {
            buffer.resize(static_cast<size_t>(size));
            inFile.read(reinterpret_cast<char*>(buffer.data()), static_cast<std::streamsize>(size));
            if(inFile.gcount() != static_cast<std::streamsize>(size)){
                std::cerr << "Truncated constant data!\n";
                std::exit(-1);
            }
        }
        constants.push_back(std::move(buffer));
    }

    //bytecode
    uint64_t bytesCount = 0;
    inFile.read(reinterpret_cast<char*>(&bytesCount), sizeof(bytesCount));
    std::vector<size_t> code;
    if (bytesCount) {
        code.resize(static_cast<size_t>(bytesCount));
        inFile.read(reinterpret_cast<char*>(code.data()), static_cast<std::streamsize>(bytesCount * sizeof(uint64_t)));
    }

    inFile.close();
    return KoalaByte::Bytecode(code, constants, regions);
}
