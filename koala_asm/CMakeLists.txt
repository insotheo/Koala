cmake_minimum_required(VERSION 3.10)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

project(Koala_ASM)
set(PRJ_NAME koala_asm)

file(GLOB KOALA_SOURCES
src/main.cpp
src/parser/lexer.cpp
)

file(GLOB KOALA_HEADERS
src/config.h
src/err_code.h
src/parser/lexer.h
src/parser/token.h
)

add_executable("${PRJ_NAME}" ${KOALA_SOURCES} ${KOALA_HEADERS})

target_include_directories("${PRJ_NAME}" 
PRIVATE src/
)

target_link_libraries("${PRJ_NAME}"
PUBLIC koala_vm_lib
)