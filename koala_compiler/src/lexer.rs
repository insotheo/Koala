#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Span{
    pub line: usize,
    pub column: usize,
}

#[derive(Debug, Clone, PartialEq)]
pub enum Token{
    Unknown(String),

    OpCode(String),
    Identifier(String),
    Number(u64),

    Colon,
    EOF
}

#[derive(Debug, Clone, PartialEq)]
pub struct TokenWithSpan{
    pub token: Token,
    pub span: Span,
}

pub struct Lexer<>{
    input: Vec<char>,
    pos: usize,
    curr_line: usize,
    curr_column: usize,
}

impl Lexer{
    pub fn new(input: &String) -> Self{
        return Lexer {
            input: input.chars().collect(),
            pos: 0, //points to next char
            curr_line: 1,
            curr_column: 1
        };
    }

    fn is_eof(&self) -> bool{
        return self.pos >= self.input.len();
    }

    fn read_char(&mut self) -> Option<char>{
        if self.is_eof(){
            return None;
        }

        let ch = self.input[self.pos];
        self.pos += 1;

        if ch == '\n'{
            self.curr_line += 1;
            self.curr_column = 1;
        } else{
            self.curr_column += 1;
        }

        return Some(ch);
    }

    fn peek_char(&self) -> Option<char>{
        if self.is_eof(){
            return None;
        }
        return Some(self.input[self.pos]);
    }

    fn skip_whitespaces_and_comments(&mut self){
        while let Some(ch) = self.peek_char(){ 
            if ch.is_whitespace(){
                self.read_char();
            } else if ch == ';'{
                while let Some(c) = self.peek_char(){
                    self.read_char();
                    if c == '\n' { break; }
                }
            } else{
                break;
            }
        }
    }

    pub fn next_token(&mut self) -> TokenWithSpan{
        self.skip_whitespaces_and_comments();

        let start_span = Span {
            line: self.curr_line,
            column: self.curr_column,
        };

        let ch = match self.peek_char() {
            Some(c) => c,
            None => return TokenWithSpan { token: Token::EOF, span: start_span },
        };

        if ch == ':'{
            self.read_char();
            return TokenWithSpan { token: Token::Colon, span: start_span };
        }

        if ch.is_ascii_digit(){
            let mut num_str = String::new();
            let mut radix = 10;

            if ch == '0'{
                if self.pos + 1 < self.input.len(){
                    let next_ch = self.input[self.pos + 1].to_ascii_lowercase();
                    match next_ch {
                        'x' => { radix = 16; self.read_char(); self.read_char(); }, //skip 0x
                        'o' => { radix = 8; self.read_char(); self.read_char(); }, //skip 0o
                        'b' => { radix = 2; self.read_char(); self.read_char(); }, //skip 0b
                        _ => {},
                    }
                }
            }

            while let Some(c) = self.peek_char(){
                let is_valid = match radix {
                    16 => c.is_ascii_hexdigit(),
                    8 => c >= '0' && c <= '7',
                    2 => c == '0' || c == '1',
                    _ => c.is_ascii_digit(),
                };

                if is_valid {
                    num_str.push(self.read_char().unwrap());
                } else {
                    break;
                }
            }

            if num_str.is_empty(){
                return TokenWithSpan { token: Token::Unknown("No digits found after prefix".to_string()), span: start_span };
            }

            let num = match u64::from_str_radix(&num_str, radix) {
                Ok(n) => n,
                Err(_) => return TokenWithSpan { token:Token::Unknown("Incorrect number format".to_string()), span: start_span }
            };

            return TokenWithSpan { token: Token::Number(num), span: start_span };
        }

        if ch.is_alphabetic() || ch == '_'{
            let mut word = String::new();

            while let Some(c) = self.peek_char(){
                if c.is_alphabetic() || c == '_'{
                    word.push(self.read_char().unwrap());
                } else{
                    break;
                }
            }

            let lower_word = word.to_lowercase();

            let token = if [
                    "ret",
                    "push",

                    "add",
                    "sub",
                    "neg",
                    "mul",
                    "umul",
                    "div",
                    "udiv",

                    "and",
                    "or",
                    "xor",
                    "not",
                    "shl",
                    "shrl",
                    "shra",
                ].contains(&lower_word.as_str()){
                    Token::OpCode(lower_word)
                } else{
                    Token::Identifier(word)
                };

            return TokenWithSpan { token, span: start_span }; 
        }

        self.read_char();
        return TokenWithSpan { token:Token::Unknown(ch.to_string()), span: start_span };
    }
}