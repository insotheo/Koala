#include "lexer/token.hpp"
#include <variant>
extern "C"{
    #include <test.h>
}
#include <iostream>
#include <cstring>
#include <string>
#include <fstream>
#include <unordered_map>

#include "lexer/lexer.hpp"


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

    { //processing source code
        koalac::Lexer lexer(source);

        koalac::Token t = lexer.NextToken();
        while(t.Type != koalac::TokenType::EndOfFile){
            std::cout << "Type(" << t.Span.Line << ", " << t.Span.Column << "): " << (int)t.Type << " Val: ";
            std::visit([](const auto& arg){
                using T = std::decay_t<decltype(arg)>;
                if constexpr (std::is_same_v<T, std::monostate>){
                    std::cout << "<NONE>";
                } else {
                    std::cout << arg;
                }
            }, t.Val);
            std::cout << "\n";

            t = lexer.NextToken(); 
        }
    }

    return 0; 
}