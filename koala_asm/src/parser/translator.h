#ifndef KOALA_ASM_TRANSLATOR_H
#define KOALA_ASM_TRANSLATOR_H

#include "koala_vm/byte_data.h"
#include "parser/instruction.h"

ByteData translate(const std::vector<CodeBlock>& blocks);

#endif