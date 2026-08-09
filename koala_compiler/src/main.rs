use std::env;
use std::fs;
use std::path::Path;

use koala_vm::vm;

mod lexer;
mod ir;
mod parser;

fn main(){
    let args: Vec<String> = env::args().collect();
    if args.len() < 2{
        println!("Koala compiler usage:\nkoalac <path to klasm file>");
        return;    
    }

    let asm_path = &args[1];
    let asm_source = match fs::read_to_string(asm_path) {
        Ok(source) => source,
        Err(err) => {
            eprintln!("Failed to read file {}: {}", asm_path, err);
            return;
        }
    };

    let lexer = lexer::Lexer::new(&asm_source);
    let mut parser = parser::Parser::new(lexer);

    let program = parser.parse_program();

    if parser.failed(){
        parser.print_errors_report(&asm_source);
        println!("Compilation failed!");
        return;
    }

    let bytecode = program.compile_to_bytes();

    //save to file
    let path = Path::new(asm_path);
    let output_path = path.with_extension("klbc");

    match fs::write(&output_path, &bytecode) {
        Ok(_) => {
            println!("Successfully compiled! Output saved to: {}", output_path.display());
        },
        Err(err) => {
            eprintln!("Failed to write binary file {}: {}", output_path.display(), err);
        }
    }

    //dbg run
    let vm = vm::VM::new(bytecode);
    
    let start = std::time::Instant::now();
    vm.run();
    let duration = start.elapsed();
    println!("Exec time: {:?}", duration);

    
}