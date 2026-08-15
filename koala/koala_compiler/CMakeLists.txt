project(KOALA_COMPILER VERSION 0.0.1 LANGUAGES C CXX)
set(APP_NAME "koalac")

set(COMPILER_SOURCES
src/main.cpp
src/ir.cpp
src/lexer/lexer.cpp
src/parser/parser.cpp
src/translator/translator.cpp
)

add_executable(${APP_NAME} ${COMPILER_SOURCES})

target_include_directories(${APP_NAME}
PRIVATE src/
)

target_link_libraries(${APP_NAME} PRIVATE koala_core)