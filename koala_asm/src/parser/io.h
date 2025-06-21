#ifndef KOALA_ASM_IO_H
#define KOALA_ASM_IO_H

#include <string>
#include "koala_vm/byte_data.h"

void save_to_file(const ProgramData& data, const std::string& path);
ProgramData load_from_file(const std::string& path);

#endif
