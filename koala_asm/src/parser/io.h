#ifndef KOALA_ASM_IO_H
#define KOALA_ASM_IO_H

#include <string>
#include "koala_vm/byte_data.h"

void save_to_file(const ByteData& data, const std::string& path);
ByteData load_from_file(const std::string& path);

#endif
