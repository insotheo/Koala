project(KoalaVM VERSION 0.0.1 LANGUAGES CXX C)

file(GLOB VM_SOURCES
src/test.cpp
)

add_library(koala_vm STATIC ${VM_SOURCES})

target_include_directories(koala_vm
PUBLIC include/
)