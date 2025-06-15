//*.asm.kls
//kls stands for Koala Source

#include <iostream>
#include <string>
#include <vector>
#include <unordered_map>
#include <fstream>

#include "config.h"
#include "err_code.h"
#include "parser/lexer.h"
#include "parser/token.h"
#include "koala_vm/config.h"

void print_help(){
    std::cout << "koala_asm [options]\n"
              << "Options:\n"
              << "  -p <path>      Path to the *.asm.kls file(REQUIRED)\n"
              << "  -s <symbol>    Start symbol (e.g., _main)\n"
              << "  --help         Show help message\n"
              << "  --version      Show version\n"
              ;
}

void print_version(){
    std::cout << "koala_asm\n"
              << "ASM version: " << KOALA_ASM_VERSION_MAJOR << "." << KOALA_ASM_VERSION_MINOR << "." << KOALA_ASM_VERSION_PATCH << KOALA_ASM_VERSION_IDENTIFIER << "\n"
              << "VM version: " << KOALA_VM_VERSION_MAJOR << "." << KOALA_VM_VERSION_MINOR << "." << KOALA_VM_VERSION_PATCH << KOALA_VM_VERSION_IDENTIFIER
              ;
}

int main(int argc, char** argv){
    std::unordered_map<std::string, std::string> args;

    for(int i = 1; i < argc; ++i){
        std::string arg = argv[i];

        if(arg == "--help"){
            print_help();
            return KOALA_ASM_ERR_CODE_OKAY;
        }
        
        if(arg == "--version"){
            print_version();
            return KOALA_ASM_ERR_CODE_OKAY;
        }

        if((arg == "-p" || arg == "-s") && (i + 1 < argc)){
            args[arg] = argv[++i];
        }
        else if(arg[0] == '-'){
            std::cerr << "Unknown or incomplete option: " << arg << "\n\n";
            print_help();
            return KOALA_ASM_ERR_CODE_INCOMPLETE_OPTION;
        }
    }

    //config
    std::string file_path;
    std::string entry_point = "_main";

    if(args.find("-p") != args.end()){
        file_path = args["-p"];
    }
    else{
        std::cerr << "Missing required -p argument\n\n";
        print_help();
        return KOALA_ASM_ERR_CODE_FILE_PATH_NOT_FOUND;
    }

    //loading file content
    std::string asm_content;
    std::fstream fs(file_path, std::ios::in | std::ios::binary);
    if(!fs.is_open()){
        std::cerr << "Failed to open file stream!\n";
        return KOALA_ASM_ERR_CODE_FILE_STREAM_OPENING_FAILED;
    }
    fs.seekg(0, std::ios_base::end);
    asm_content.resize(fs.tellg());
    fs.seekg(0, std::ios_base::beg);
    fs.read(&asm_content[0], asm_content.size());
    fs.close();

    Lexer lexer(asm_content);
    std::vector<Token> tokens = lexer.parse();
    if(!lexer.is_success()){
        lexer.print_errors();
        return KOALA_ASM_ERR_CODE_LEXER_FAILED;
    }
    lexer.clear_errors_list();

    return KOALA_ASM_ERR_CODE_OKAY;
}