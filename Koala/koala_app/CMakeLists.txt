project(koala_app)

set(KOALA_APP_NAME koala)

file(GLOB KOALA_SOURCES
src/main.cpp
)

add_executable(${KOALA_APP_NAME} ${KOALA_SOURCES})

target_link_libraries(${KOALA_APP_NAME} PRIVATE koalaLang)