#include "parser/parser.h"
#include "koala_vm/op_codes.h"

#define CURRENT_TOKEN m_tokens[m_index]
#define TOKEN(index) m_tokens[index]

std::vector<CodeBlock> Parser::parse(){
    std::vector<CodeBlock> blocks;

    while (m_index < m_tokens.size())
    {
        if (CURRENT_TOKEN.type == TokenType::EndOfFile){
            break;
        }
        else if(CURRENT_TOKEN.type == TokenType::Identifier){
            FullCodeBlock block = parse_block();
            expand(blocks, block);
            next();
        }
        if(!m_is_success)
            break;
    }
    
    if(!m_is_success)
        return {};

    return blocks;
}

FullCodeBlock Parser::parse_block(){
    FullCodeBlock block;
    block.label = CURRENT_TOKEN.value;
    
    next();
    fatal(TokenType::Colon);
    
    //parsing instructions
    while(m_index < m_tokens.size() && m_is_success){
        next();
        if(CURRENT_TOKEN.type == TokenType::Keyword){
            Instruction instr;

            if(CURRENT_TOKEN.value == "END"){ //END <label>
                next();
                fatal(TokenType::Identifier);
                if (CURRENT_TOKEN.value == block.label){
                    break; //END of the current block
                }
                throw std::runtime_error("Parser error: line is \"END " + CURRENT_TOKEN.value + "\", but it have to be \"END " + block.label + "\"!");
            }
            else if(CURRENT_TOKEN.value == "RET"){ //RET
                instr.op_code = OpCode::OP_RET;
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "PUSH"){ //PUSH <value>
                instr.op_code = OpCode::OP_PUSH;
                next();

                if(CURRENT_TOKEN.type == TokenType::Number){
                    instr.operands.push_back(std::stoi(CURRENT_TOKEN.value));
                }
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "POP"){ //POP
                instr.op_code = OpCode::OP_POP;
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "DUP"){ //DUP
                instr.op_code = OpCode::OP_DUP;
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "POP_N"){ //POP_N <value>
                instr.op_code = OpCode::OP_POP_N;
                next();

                if(CURRENT_TOKEN.type != TokenType::Number){
                    throw std::runtime_error("After POP_N expects a number!");
                }
                instr.operands.push_back(std::stoi(CURRENT_TOKEN.value));
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "MARK"){ //MARK <label>
                instr.op_code = OpCode::PC_MARK;
                next();

                if(CURRENT_TOKEN.type != TokenType::Identifier){
                    throw std::runtime_error("After MARK expects an identifier(label name)!");
                }
                instr.operands.push_back(CURRENT_TOKEN.value);
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "JMP"){ //JMP <id>
                instr.op_code = OpCode::OP_JMP;
                next();
                if(CURRENT_TOKEN.type != TokenType::Identifier){
                    throw std::runtime_error("After JMP expects an identifier(label name)!");
                }
                instr.operands.push_back(CURRENT_TOKEN.value);
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "JEZ"){ //JEZ <id>
                instr.op_code = OpCode::OP_JEZ;
                next();
                if(CURRENT_TOKEN.type != TokenType::Identifier){
                    throw std::runtime_error("After JEZ expects an identifier(label name)!");
                }
                instr.operands.push_back(CURRENT_TOKEN.value);
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "JNZ"){ //JNZ <id>
                instr.op_code = OpCode::OP_JNZ;
                next();
                if(CURRENT_TOKEN.type != TokenType::Identifier){
                    throw std::runtime_error("After JNZ expects an identifier(label name)!");
                }
                instr.operands.push_back(CURRENT_TOKEN.value);
                block.block_instructions.push_back(instr);
                continue;
            }

            //arithmetic
            else if(CURRENT_TOKEN.value == "INC"){
                instr.op_code = OpCode::OP_INC;
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "DEC"){
                instr.op_code = OpCode::OP_DEC;
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "ADD"){
                instr.op_code = OpCode::OP_ADD;
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "SUB"){
                instr.op_code = OpCode::OP_SUB;
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "MUL"){
                instr.op_code = OpCode::OP_MUL;
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "DIV"){
                instr.op_code = OpCode::OP_DIV;
                block.block_instructions.push_back(instr);
                continue;
            }

            //logical
            else if(CURRENT_TOKEN.value == "AND"){
                instr.op_code = OpCode::OP_AND;
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "OR"){
                instr.op_code = OpCode::OP_OR;
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "XOR"){
                instr.op_code = OpCode::OP_XOR;
                block.block_instructions.push_back(instr);
                continue;
            }
            else if(CURRENT_TOKEN.value == "NOT"){
                instr.op_code = OpCode::OP_NOT;
                block.block_instructions.push_back(instr);
                continue;
            }
            
            
        }
        else if(CURRENT_TOKEN.type == TokenType::Identifier){
            FullCodeBlock inside_block = parse_block();
            block.inside_blocks.push_back(inside_block);
            continue;
        }
    }

    return block;
}

void Parser::expand(std::vector<CodeBlock>& glob, FullCodeBlock& full){
    CodeBlock head_block;
    head_block.label = full.label;
    head_block.block_instructions = full.block_instructions;
    glob.push_back(head_block);

    for(FullCodeBlock& sub_block : full.inside_blocks){
        expand(glob, sub_block);
    }
}

void Parser::next(){
    if(m_index + 1 < m_tokens.size())
        m_index += 1;
}

bool Parser::expect(const TokenType type){
    next();
    if(CURRENT_TOKEN.type != type){
        fatal(type);
        return false;
    }
    return true;
}

void Parser::fatal(const TokenType type){
    if(CURRENT_TOKEN.type == type){
        return;
    }
    std::cerr << "Unexpected token: " << CURRENT_TOKEN.value << ", expected type " << static_cast<int>(type) << " (tokens index: " << static_cast<int>(m_index) << ")\n";
    m_is_success = false;
}