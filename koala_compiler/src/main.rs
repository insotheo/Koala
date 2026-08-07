use std::env;
use std::fs;

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

    let _program = parser.parse_program();

    if parser.failed(){
        parser.print_errors_report(&asm_source);
    }
}