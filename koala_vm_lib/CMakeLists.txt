cmake_minimum_required(VERSION 3.10)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

project(Koala_VM)
set(PRJ_NAME koala_vm_lib)

file(GLOB KOALA_SOURCES
src/op_codes.cpp
src/vm.cpp
)

add_library("${PRJ_NAME}" STATIC ${KOALA_SOURCES})

target_include_directories("${PRJ_NAME}" 
PUBLIC include/
)