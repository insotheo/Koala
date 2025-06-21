//*.asm.kls
//kls stands for Koala Source
//kbc stands for Koala Byte Code

#include <iostream>
#include <string>
#include <vector>
#include <unordered_map>
#include <fstream>
#include <optional>

#include "config.h"
#include "err_code.h"
#include "parser/lexer.h"
#include "parser/token.h"
#include "parser/instruction.h"
#include "parser/parser.h"
#include "parser/translator.h"
#include "parser/io.h"
#include "koala_vm/config.h"
#include "koala_vm/vm.h"

void print_help() {
    std::cout << "koala_asm <command> [options]\n"
              << "  --help         Show help message\n"
              << "  --version      Show version\n\n"
              << "koala_asm build/make [options]:\n"
              << "  -p <path>      Path to the *.asm.kls file(REQUIRED)\n"
              << "  -o <symbol>    Output path(REQUIRED)\n\n"
              << "koala_asm run [options]:\n"
              << "  -p <path>      Path to the *.kbc file(REQUIRED)\n"
              << "  -s <symbol>    Start symbol (e.g., _main)\n"
              << "  -r             Print returned value on the screen\n"
              << "  -k             Wait for an [Enter] key to be pressed before closing the program\n"
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
    std::string command = argv[1];

    for(int i = 2; i < argc; ++i){
        std::string arg = argv[i];

        if(arg == "--help"){
            print_help();
            return KOALA_ASM_ERR_CODE_OKAY;
        }
        
        if(arg == "--version"){
            print_version();
            return KOALA_ASM_ERR_CODE_OKAY;
        }

        if((arg == "-p" || arg == "-o" || arg == "-s") && (i + 1 < argc)){
            args[arg] = argv[++i];
        }
        else if(arg == "-r" || arg == "-k"){
            args[arg] = "";
        }
        else if(arg[0] == '-'){
            std::cerr << "Unknown or incomplete option: " << arg << "\n\n";
            print_help();
            return KOALA_ASM_ERR_CODE_INCOMPLETE_OPTION;
        }
    }

    if (command == "build" || command == "make") {
        //config
        std::string file_path;
        std::string output_path;

        if(args.find("-p") == args.end() || args.find("-o") == args.end()){
            std::cerr << "Missing required -p, -o arguments\n\n";
            print_help();
            return KOALA_ASM_ERR_CODE_FILE_PATH_NOT_FOUND;
        }
        else{
            file_path = args["-p"];
            output_path = args["-o"];
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

        Parser parser(tokens);
        std::vector<CodeBlock> codeblocks = parser.parse();
        if(!parser.is_success()){
            return KOALA_ASM_ERR_CODE_LEXER_FAILED;
        }

        ProgramData bytes = translate(codeblocks);
        save_to_file(bytes, output_path);
    }
    else if(command == "run"){
        //config
        std::string file_path;
        std::string entry_point = "_main";
        bool print_return_value = false;
        bool await_key = false;

        if(args.find("-p") == args.end()){
            std::cerr << "Missing required -p arguments\n\n";
            print_help();
            return KOALA_ASM_ERR_CODE_FILE_PATH_NOT_FOUND;
        }
        else{
            file_path = args["-p"];
        }

        if(args.find("-s") != args.end()){
            entry_point = args["-s"];       
        }

        if(args.find("-r") != args.end()){
            print_return_value = true;      
        }
        if(args.find("-k") != args.end()){
            await_key = true;
        }

        ProgramData data = load_from_file(file_path);
        
        KoalaVM vm(data);
        std::optional<Value> result = vm.run(entry_point);

        if(print_return_value){
            if(result.has_value()){
                std::visit([&](const auto& val){
                    std::cout << "\nProgram finished with result: " << val << "\n";
                }, result.value());
            }
            else{
                std::cout << "\nProgram finished with zero-result.\n";
            }
        }
        if(await_key){
            std::getchar();
        }
    }
    else{
        print_help();
    }

    return KOALA_ASM_ERR_CODE_OKAY;
}