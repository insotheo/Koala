#include "KoalaByte/ByteData.h"

namespace KoalaByte{
    bool VerifyByteDataType(ByteData_t& data, const ByteDataType& type){
        return data[0] == static_cast<size_t>(type);
    }
}