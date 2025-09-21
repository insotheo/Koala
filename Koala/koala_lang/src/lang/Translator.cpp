#include "KoalaLang/Translator.h"
#include "KoalaByte/OpCodes.h"
#include "KoalaLang/ASTNodes.h"

#include <cstring>

namespace KoalaLang{

    #define TRANSLATOR_CONSTANT_MAKING(buffer, ptr_name, const_type, byte_data_type, it)\
                                                const auto& constValue = ptr_name->GetConst();\
                                                buffer.resize(1  + sizeof(constValue));\
                                                buffer[0] = static_cast<uchar>(KoalaByte::ByteDataType::byte_data_type);\
                                                std::memcpy(&buffer[1], &constValue, sizeof(constValue));\


    #define OPCODE(code) static_cast<size_t>(KoalaByte::OpCode::code)

    void Translator::Translate(){
        VisitCodeBlock(m_code);
    }

    void Translator::VisitCodeBlock(ASTCodeBlock& block){
        for(SHARED_PTR_T(ASTNode)& node : block.GetNodes()){
            //fn decl
            if(const auto fn = dynamic_cast<ASTFunction*>(node.get())){
                m_regions_ptrs.insert({fn->GetFunctionName(), m_codes.size()});
                VisitCodeBlock(*(fn->GetBody()));
                continue;
            }

            //all other stuff
            if(auto block = dynamic_cast<ASTCodeBlock*>(node.get())){
                VisitCodeBlock(*block);
                continue;
            }

            if(auto retNode = dynamic_cast<ASTRet*>(node.get())){
                VisitExpression(*(retNode->GetReturnNode()));
                m_codes.push_back(OPCODE(RET));
            }
        }
    }

    void Translator::VisitExpression(ASTNode& node){
        if (auto* binOperation = dynamic_cast<ASTBinaryOperation*>(&node)) {
            VisitExpression(*(binOperation->GetLeft()));
            VisitExpression(*(binOperation->GetRight()));
            m_codes.push_back(static_cast<size_t>(binOperation->GetOperation()));
        }
        else {//try visiting constant
            const size_t idx = VisitConstant(node);
            if(idx == -1) return;
            m_codes.push_back(OPCODE(PUSH));
            m_codes.push_back(idx);
        }
    }

    size_t Translator::VisitConstant(ASTNode& constant) {
        ByteData_t buffer;
        
        if(auto* intConst = dynamic_cast<ASTNumberLiteral*>(&constant)) {
            TRANSLATOR_CONSTANT_MAKING(buffer, intConst, long int, INT, iter);
        }
        else if(auto* floatConst = dynamic_cast<ASTFloatLiteral*>(&constant)) {
            TRANSLATOR_CONSTANT_MAKING(buffer, floatConst, double, FLOAT, iter);
        }

        if (buffer.empty()) {
            return -1;
        }

        for (int i = 0; i < m_constants.size(); i++) {
            if (m_constants[i] == buffer) return i;
        }

        m_constants.push_back(buffer);
        return m_constants.size() - 1;
    }
}