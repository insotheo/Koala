#ifndef KOALA_BYTE_BYTE_DATA_H
#define KOALA_BYTE_BYTE_DATA_H

#include <vector>

typedef std::vector<size_t> ByteData_t;

namespace KoalaByte{
    enum class ByteDataType: size_t{
        INT = 0,
    };

    bool VerifyByteDataType(ByteData_t& data, const ByteDataType& type);
}

#endif