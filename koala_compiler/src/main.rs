use std::env;
use std::fs;

use koala_vm::opcode;

mod lexer;
mod ir;

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

    let mut lexer = lexer::Lexer::new(&asm_source);

    let mut t = lexer.next_token();
    while t.token != lexer::Token::EOF{
        println!("{:?}", t.token);
        t = lexer.next_token();
    }

}