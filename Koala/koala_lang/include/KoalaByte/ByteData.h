#ifndef KOALA_BYTE_BYTE_DATA_H
#define KOALA_BYTE_BYTE_DATA_H

#include <cstdint>
#include <vector>

typedef std::vector<uint8_t> ByteData_t;

namespace KoalaByte{
    enum class ByteDataType: uint8_t{
        //N >= 1
        
        //numerics: 2^N
        VOID = 0,
        INT = 2,
        FLOAT = 4,
    };

    bool VerifyByteDataType(ByteData_t& data, const ByteDataType& type);
}

#endif