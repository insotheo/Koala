project(App)

file(GLOB APP_SOURCES
src/main.cpp
)

add_executable(KoalaApp ${APP_SOURCES})

target_link_libraries(KoalaApp
PRIVATE koala_vm
)