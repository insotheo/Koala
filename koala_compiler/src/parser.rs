use koala_vm::opcode::OpCode;

use crate::ir::{IRNode, Operand, ProgramIR};
use crate::lexer::{Lexer, Span, Token, TokenWithSpan};

#[derive(Debug, Clone)]
pub struct SyntaxError{
    pub msg: String,
    pub span: Span,
}

pub struct Parser{
    lexer: Lexer,
    current_token: TokenWithSpan,
    peek_token: TokenWithSpan,
    errors: Vec<SyntaxError>,
}

impl Parser{
    pub fn new(mut lexer: Lexer) -> Self{
        let current_token = lexer.next_token();
        let peek_token = lexer.next_token();
        return Parser {
            lexer: lexer,
            current_token: current_token,
            peek_token: peek_token,
            errors: Vec::new()
        };
    }
    
    fn next_token(&mut self){
        self.current_token = self.peek_token.clone();
        self.peek_token = self.lexer.next_token();
    }

    fn synchronize(&mut self){
        while self.current_token.token != Token::EOF{
            if let Token::OpCode(_) = &self.current_token.token{
                break;
            }

            if let Token::Identifier(_) = &self.current_token.token {
                if self.peek_token.token == Token::Colon {
                    break;
                }
            }

            self.next_token();
        }
    }

    fn add_error(&mut self, msg: &str, span: Span){
        self.errors.push(SyntaxError {
            msg: msg.to_string(),
            span: span 
        });
    }

    pub fn parse_program(&mut self) -> ProgramIR{
        let mut nodes: Vec<IRNode> = Vec::new();        

        while self.current_token.token != Token::EOF{
            match &self.current_token.token{

                Token::OpCode(op_name) => {
                    let mut failed_parsing_instr = false;

                    // getting opcode
                    let opcode = match op_name.as_str() {
                        "ret" => OpCode::Ret,

                        "push" => OpCode::Push,

                        "add" => OpCode::Add,
                        "sub" => OpCode::Sub,
                        "neg" => OpCode::Neg,
                        "mul" => OpCode::Mul,
                        "umul" => OpCode::UMul,
                        "div" => OpCode::Div,
                        "udiv" => OpCode::UDiv,
                        "rem" => OpCode::Rem,
                        "urem" => OpCode::URem,

                        "fadd" => OpCode::FAdd,
                        "fsub" => OpCode::FSub,
                        "fneg" => OpCode::FNeg,
                        "fmul" => OpCode::FMul,
                        "fdiv" => OpCode::FDiv,

                        "and" => OpCode::And,
                        "or" => OpCode::Or,
                        "xor" => OpCode::Xor,
                        "not" => OpCode::Not,
                        "shl" => OpCode::Shl,
                        "shrl" => OpCode::Shrl,
                        "shra" => OpCode::Shra,

                        _ => {
                            self.add_error(
                                &format!("OpCode '{}' recognized by lexer but unsupported by parser", op_name),
                                self.current_token.span
                            );
                            failed_parsing_instr = true;

                            OpCode::None
                        }
                    };

                    self.next_token();

                    //getting its operand(if has)
                    let operand = match opcode {
                        OpCode::Push => {
                            let oper = match &self.current_token.token {
                                Token::Number(num) => Operand::IntConstant(*num),
                                
                                _ => {
                                    self.add_error(
                                        &format!("Instruction 'push' requires constant argument"),
                                        self.current_token.span
                                    );
                                    failed_parsing_instr = true;

                                    Operand::None
                                }
                            };

                            self.next_token();
                            
                            oper
                        },

                        _ => Operand::None,
                    };


                    if !failed_parsing_instr{
                        nodes.push(IRNode::Instruction { opcode, operand })
                    } else {
                        self.synchronize();
                    }
                },


                Token::Identifier(ident) => {
                    if self.peek_token.token == Token::Colon{
                        nodes.push(IRNode::LabelDeclaraction(ident.clone()));
                        self.next_token(); //label name
                        self.next_token(); //colon
                    } else {
                        self.add_error(
                            &format!("Dangling identifier '{}'. Missing ':' for label declaration?", ident),
                            self.current_token.span 
                        );
                        self.next_token();
                        self.synchronize();
                    }
                },


                _ => {
                    self.add_error(
                        &format!("Unexpected token {:?}", self.current_token.token),
                        self.current_token.span
                    );
                    self.next_token();
                    self.synchronize();
                },


            }

        }

        return ProgramIR::new(nodes);
    }

    pub fn failed(&self) -> bool{
        return self.errors.len() > 0;
    }

    pub fn print_errors_report(&self, source_code: &str){
        let lines: Vec<&str> = source_code.lines().collect();

        eprintln!("\nFound {} syntax errors during compilation:\n", self.errors.len());

        for (idx, err) in self.errors.iter().enumerate() {
            let err_line = lines.get(err.span.line - 1).unwrap_or(&"");

            eprintln!("[{}]Error(ln: {}, col: {}): {}", idx + 1, err.span.line, err.span.column, err.msg);
            eprintln!("-> {}", err_line);

            let p = " ".repeat(err.span.column - 1 + 3) + "^";
            eprintln!("{}", p);
        }
    }

}