#include <iostream>
#include <format>
#include <fstream>
#include <unordered_map>
#include <string>
#include <KoalaLang/KoalaLangVersionInfo.h>
#include <KoalaLang/Lexer.h>
#include <KoalaLang/Parser.h>

void printHelpMsg(){
    std::cout << std::format(R"(
Welcome to Koala Compiler!
Version: {}.{}.{}
---------------------------------
koala <command> <args>

> koala <build|make> <args>
    -p <path> - REQUIRED, path to file for building
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

        KoalaLang::Lexer lexer(source);
        lexer.Tokenize();

        KoalaLang::Parser parser(lexer);
        parser.Parse();

        //DBG
        std::cout << "Nodes: " << parser.GetAST().GetNodes().size() << "\n";
    }

    return 0;
}