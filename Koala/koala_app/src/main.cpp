#include <iostream>
#include <format>
#include <fstream>
#include <unordered_map>
#include <string>
#include <KoalaLang/KoalaLangVersionInfo.h>
#include <KoalaLang/Lexer.h>
#include <KoalaLang/Parser.h>
#include <KoalaLang/Translator.h>
#include "io/io.h"
#include <chrono>
#include <filesystem>

void printTimeDuration(const std::chrono::high_resolution_clock::time_point& start_time, const std::chrono::high_resolution_clock::time_point& end_time){
    auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(end_time - start_time);

    auto hours = std::chrono::duration_cast<std::chrono::hours>(duration);
    duration -= hours;
    
    auto minutes = std::chrono::duration_cast<std::chrono::minutes>(duration);
    duration -= minutes;
    
    auto seconds = std::chrono::duration_cast<std::chrono::seconds>(duration);
    duration -= seconds;

    auto milliseconds  = duration.count();

    std::cout << std::setfill('0') << std::setw(2) << hours.count() << ":"
              << std::setfill('0') << std::setw(2) << minutes.count() << ":"
              << std::setfill('0') << std::setw(2) << seconds.count() << "."
              << std::setfill('0') << std::setw(3) << milliseconds;
}

void printHelpMsg(){
    std::cout << std::format(R"(
Welcome to Koala Compiler!
Version: {}.{}.{}
---------------------------------
koala <command> <args>

> koala <build|make> <args>
    -p <path> - REQUIRED, path to file for building
    -o <path> - REQUIRED, path to output file
    -m <module name> - name of global module(default: filename)
)", 
    KOALA_LANG_MAJOR_VERSION, KOALA_LANG_MINOR_VERSION, KOALA_LANG_PATCH_VERSION); 
}

int main(int argc, char** argv){
    std::string base_action;
    std::unordered_map<std::string, std::string> args;
    { //parsing args
        if(argc <= 2){ //koala <key_word|unknown_word>
            printHelpMsg();
            return 0;
        }
        base_action = argv[1];
        if(base_action == "help" || base_action == "version" || base_action == "-h" || base_action == "--help" || base_action == "-v" || base_action == "--version"){
            printHelpMsg();
            return 0;
        }

        for(int i = 2; i < argc; ++i){
            if(argv[i][0] == '-'){ //found flag
                if(i + 1 < argc){
                    if(argv[i + 1][0] != '-'){//not flag
                        args.insert({static_cast<std::string>(argv[i]), static_cast<std::string>(argv[i + 1])});
                        i += 1;
                        continue;
                    }
                }
                args.insert({static_cast<std::string>(argv[i]), ""});
            }
        }
    }

    if(base_action == "build" || base_action == "make"){
        if(!args.contains("-p")){
            std::cerr << "Path is required!\n";
            return -1;
        }
        std::string path = args.at("-p");
        
        if(!args.contains("-o")){
            std::cerr << "Output path is required!\n";
            return -1;
        }
        std::string output = args.at("-o");

        std::string moduleName;
        if (args.contains("-m")) moduleName = args.at("-m");
        else {
            std::filesystem::path p(path);
            moduleName = p.stem().string();
        }

        std::fstream fs(path);
        if(!fs.is_open()){
            std::cerr << std::format("Cannot open file \"{}\"\n", path);
            return -1;
        }
        std::string source;

        fs.seekg(0, std::ios::end);
        source.reserve(fs.tellg());
        fs.seekg(0, std::ios::beg);

        source.assign(std::istreambuf_iterator<char>(fs), std::istreambuf_iterator<char>());
        fs.close();

        auto start_time = std::chrono::high_resolution_clock::now();

        KoalaLang::Lexer lexer(source);
        lexer.Tokenize();

        KoalaLang::Parser parser(lexer);
        parser.Parse(moduleName);

        KoalaLang::Translator translator(parser);
        translator.Translate();
        
        KoalaByte::Bytecode bytecode = translator.GetBytecode();
        
        WriteBytecodeToFile(bytecode, output);

        auto end_time = std::chrono::high_resolution_clock::now();
        
        std::cout << "Success!\n";
        std::cout << "Build completed: ";
        printTimeDuration(start_time, end_time);
        std::cout << "\n";
    }

    return 0;
}