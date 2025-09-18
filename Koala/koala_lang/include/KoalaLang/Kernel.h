#ifndef KOALA_LANG_KERNEL_H
#define KOALA_LANG_KERNEL_H

#ifdef KOALA_LANG_EXPORTS
   #define KOALA_LANG_API __declspec(dllexport)
#else
   #define KOALA_LANG_API __declspec(dllimport)
#endif

#ifndef KOALA_WINDOWS
    #error Koala supports only Windows yet
#endif

#include <memory>
#define SHARED_PTR_T(type) std::shared_ptr<type>

#endif