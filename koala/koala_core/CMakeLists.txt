project(KOALA_CORE VERSION 0.0.1 LANGUAGES C)
set(LIB_NAME "koala_core")

set(VM_SOURCES
src/vm.c
)

add_library(${LIB_NAME} STATIC ${VM_SOURCES})

set_target_properties(${LIB_NAME} PROPERTIES LINKER_LANGUAGE C)

target_include_directories(${LIB_NAME}
PUBLIC include/
PRIVATE src/
)