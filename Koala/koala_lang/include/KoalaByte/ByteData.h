#ifndef KOALA_BYTE_BYTE_DATA_H
#define KOALA_BYTE_BYTE_DATA_H

#include <vector>

typedef unsigned char uchar;
typedef std::vector<uchar> ByteData_t;

namespace KoalaByte{
    enum class ByteDataType: uchar{
        //N >= 1
        
        //numerics: 2^N
        INT = 2,
        FLOAT = 4,
    };

    bool VerifyByteDataType(ByteData_t& data, const ByteDataType& type);
}

#endif