project(KOALA_COMPILER VERSION 0.0.1 LANGUAGES C CXX)
set(APP_NAME "kolac")

set(COMPILER_SOURCES
src/main.cpp
src/lexer/lexer.cpp
)

add_executable(${APP_NAME} ${COMPILER_SOURCES})

target_include_directories(${APP_NAME}
PRIVATE src/
)

target_link_libraries(${APP_NAME} PRIVATE koala_vm)