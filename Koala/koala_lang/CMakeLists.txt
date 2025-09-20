project(koala_lang)

set(KOALA_NAME koalaLang)

file(GLOB KOALA_SOURCES
src/lang/*.cpp
src/byte/*.cpp
)

set(KOALA_LANG_PRIVATE_DEFS)

if(WIN32)
    list(APPEND KOALA_LANG_PRIVATE_DEFS KOALA_WINDOWS)
endif()

add_library(${KOALA_NAME} SHARED ${KOALA_SOURCES})

target_include_directories(${KOALA_NAME} PUBLIC include/)

target_compile_definitions(${KOALA_NAME}
PRIVATE KOALA_LANG_EXPORTS
PUBLIC ${KOALA_LANG_PRIVATE_DEFS})