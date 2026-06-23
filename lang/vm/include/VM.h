#ifndef KOALA_VM_VM_H
#define KOALA_VM_VM_H

#include "VMData.h"

namespace Koala{
    class KoalaVM{
    public:
        KoalaVM() = default;
        ~KoalaVM() = default;
        
        void Run(const VMData& data);
    };
}

#endif