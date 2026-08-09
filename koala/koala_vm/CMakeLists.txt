project(KOALA_VM VERSION 0.0.1 LANGUAGES C)
set(LIB_NAME "koala_vm")

set(VM_SOURCES
src/test.c
)

add_library(${LIB_NAME} STATIC ${VM_SOURCES})

set_target_properties(${LIB_NAME} PROPERTIES LINKER_LANGUAGE C)

target_include_directories(${LIB_NAME}
PUBLIC include/
PRIVATE src/
)