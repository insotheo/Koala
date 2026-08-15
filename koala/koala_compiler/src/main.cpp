#include <KoalaCore>
#include <iostream>
#include <cstring>
#include <string>
#include <fstream>
#include <unordered_map>

#include "lexer/lexer.hpp"
#include "parser/parser.hpp"
#include "translator/translator.hpp"
#include "ir.hpp"

const uint8_t KOALA_MAGIC_BYTES[] = {KOALA_MAG_0, KOALA_MAG_1, KOALA_MAG_2, KOALA_MAG_3, KOALA_MAG_4 };

void printHelp() {
    std::cout << R"(kolac <path_to_source.klasm> <args>
    
Flags
| -o <path> ; output save file
)";
}

int main(int argc, char** argv){
    if(argc < 2){
        printHelp();
        return 0;
    }

    std::unordered_map<std::string, std::string> args;
    std::string source;

    { //parsing args
        bool areArgsFine = true;
        for(size_t i = 2; i < argc; ++i){
            if(argv[i][0] == '-'){
                if(std::strcmp(argv[i], "-o") == 0){
                    if(i + 1 >= argc || argv[i + 1][0] == '-'){
                        std::cerr << "Wrong argument format for '" << argv[i] << "'. Expected: " << argv[i] << " <arg>.\n";
                        areArgsFine = false;
                    } else {
                        args[std::string(argv[i])] = std::string(argv[i + 1]);
                        i++;
                    }
                }
            }
        }

        if(!areArgsFine) {
            return -1;
        }
    }
    
    { // reading from file
        std::string pathToSource = argv[1];
        std::fstream fs(pathToSource);
        if(!fs){
            std::cerr << "Failed to open source file!\n";
            return -1;
        }

        fs.seekg(0, std::ios::end);
        source.resize(static_cast<size_t>(fs.tellg()));

        fs.seekg(0, std::ios::beg);
        fs.read(&source[0], static_cast<std::streamsize>(source.size()));

        fs.close();
    }
    
    koalac::Bytecode bc;
    { //processing source code
        koalac::Lexer lexer(source);
        koalac::Parser parser(&lexer);

        koalac::IRProgram program = parser.MakeProgram();
        if(!parser.IsSuccess()){
            parser.PrintErrors();
            return -1;
        }
        
        bc = koalac::translateToBytecode(program);
    }

    { //saving bytecode to file
        std::string outName;

        if(args.contains("-o")){
            outName = args["-o"];
        } else {
            std::string inputPath = argv[1];
            size_t lastDot = inputPath.find_last_of(".");
            if(lastDot != std::string::npos){
                outName = inputPath.substr(0, lastDot) + ".klbc"; //klbc is Koala Bytecode
            } else {
                outName = inputPath + ".klbc";
            }
        }

        std::ofstream outFs(outName, std::ios::out | std::ios::binary);
        if(!outFs){
            std::cerr << "Failed to open output file for writting: " << outName << "\n";
            return -1;
        }

        outFs.write(reinterpret_cast<const char*>(KOALA_MAGIC_BYTES), static_cast<std::streamsize>(5));
        outFs.write(reinterpret_cast<const char*>(bc.data()), static_cast<std::streamsize>(bc.size()));
        if(!outFs.good()){
            std::cerr << "Error occured while writing bytecode data.\n";
            return -1;
        }

        std::cout << "Successfully compiled and saved to " << outName << "\n";
    }

    return 0; 
}