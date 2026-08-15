project(KOALA_VM VERSION 0.0.1 LANGUAGES C CXX)
set(APP_NAME "koala")

set(VM_SOURCES
src/main.cpp
)

add_executable(${APP_NAME} ${VM_SOURCES})

target_include_directories(${APP_NAME}
PRIVATE src/
)

target_link_libraries(${APP_NAME} PRIVATE koala_core)