#include <iostream>
#include <format>
#include <fstream>
#include <unordered_map>
#include <string>
#include <KoalaLang/KoalaLangVersionInfo.h>

void printHelpMsg(){
    std::cout << std::format(R"(
Welcome to Koala Compiler!
Version: {}.{}.{}
---------------------------------
koala <command> <args>

> koala build <args>
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

    //DBG
    for(const auto& arg : args){
        std::cout << arg.first << " : " << arg.second << "\n";
    }

    return 0;
}