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
            }
            else if(CURRENT_TOKEN.value == "RET"){ //RET <> ; OR ; RET
                instr.op_code = OpCode::OP_RET;

                size_t peek_idx = m_index + 1;

                if(TOKEN(peek_idx).type == TokenType::Number){
                    next();
                    instr.operands.push_back(std::stoi(CURRENT_TOKEN.value));
                }
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