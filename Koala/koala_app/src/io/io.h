#ifndef KOALA_APP_IO_H
#define KOALA_APP_IO_H

#include "KoalaByte/Bytecode.h"
#include <string>

void WriteBytecodeToFile(const KoalaByte::Bytecode& bytecode, const std::string& output_path);

#endif