#include "parser/io.h"

#include <fstream>
#include <cstring>
#include "koala_vm/type_codes.h"
#include "config.h"

void save_to_file(const ProgramData& data, const std::string& path){
    std::ofstream output(path, std::ios::binary);
    if (!output.is_open()) {
        throw std::runtime_error("Could not open file " + path);
    }

    const char magic[5] = {'K', 'O', 'A', 'L', 'A'};
    uint8_t ver = KOALA_ASM_TRANSLATOR_VERSION;
    output.write(magic, 5);
    output.write(reinterpret_cast<const char*>(&ver), sizeof(uint8_t));

    //blocks
    size_t blocks_counter = data.blocks.size();
    output.write(reinterpret_cast<const char*>(&blocks_counter), sizeof(size_t));
    for (const auto& block : data.blocks){
        output.write(reinterpret_cast<const char*>(&block.begin), sizeof(size_t));
        output.write(reinterpret_cast<const char*>(&block.end), sizeof(size_t));
    }

    //consts
    size_t const_counter = data.constants.size();
    output.write(reinterpret_cast<const char*>(&const_counter), sizeof(size_t));

    for (const auto& val : data.constants) {
        if (std::holds_alternative<int>(val)) {
            uint8_t type_id = TypeCode::INT;
            output.write(reinterpret_cast<const char*>(&type_id), sizeof(uint8_t));

            int value = std::get<int>(val);
            output.write(reinterpret_cast<const char*>(&value), sizeof(int));
        }
        else {
            throw std::runtime_error("Unsupported constant type");
        }
    }

    //code
    size_t code_size = data.code.size();
    output.write(reinterpret_cast<const char*>(&code_size), sizeof(size_t));
    output.write(reinterpret_cast<const char*>(data.code.data()), sizeof(size_t) * code_size);

    output.close();
}

ProgramData load_from_file(const std::string& path) {
    std::ifstream input(path, std::ios::binary);
    if (!input.is_open()) {
        throw std::runtime_error("Failed to open file: " + path);
    }

    char magic[5];
    input.read(magic, 5);
    if (std::strncmp(magic, "KOALA", 5) != 0) {
        throw std::runtime_error("Invalid file format");
    }

    uint8_t ver;
    input.read(reinterpret_cast<char*>(&ver), sizeof(uint8_t));
    if (ver != KOALA_ASM_TRANSLATOR_VERSION) {
        throw std::runtime_error("Unsupported file version");
    }

    ProgramData byte_data;
    size_t blocks_counter = 0;
    input.read(reinterpret_cast<char*>(&blocks_counter), sizeof(size_t));

    for(size_t i = 0; i < blocks_counter; ++i){
        size_t begin;
        input.read(reinterpret_cast<char*>(&begin), sizeof(size_t));

        size_t end;
        input.read(reinterpret_cast<char*>(&end), sizeof(size_t));

        byte_data.blocks.push_back({begin, end});
    }

    size_t const_counter = 0;
    input.read(reinterpret_cast<char*>(&const_counter), sizeof(size_t));

    for (size_t i = 0; i < const_counter; ++i) {
        uint8_t type_id;
        input.read(reinterpret_cast<char*>(&type_id), sizeof(uint8_t));

        if (type_id == TypeCode::INT) {
            int val;
            input.read(reinterpret_cast<char*>(&val), sizeof(int));
            byte_data.constants.push_back(val);
        }
        else {
            throw std::runtime_error("Unknown constant type ID");
        }
    }

    size_t code_size;
    input.read(reinterpret_cast<char*>(&code_size), sizeof(size_t));
    byte_data.code.resize(code_size);
    input.read(reinterpret_cast<char*>(byte_data.code.data()), sizeof(size_t) * code_size);

    input.close();
    return byte_data;
}