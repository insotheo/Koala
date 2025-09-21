#include "KoalaLang/Translator.h"
#include "KoalaByte/OpCodes.h"
#include "KoalaLang/ASTNodes.h"

#include <cstring>
#include <stdexcept>

namespace KoalaLang{

    #define TRANSLATOR_CONSTANT_MAKING(ptr_name, const_type, byte_data_type, it)\
                                                const auto& constValue = ptr_name->GetConst();\
                                                ByteData_t buffer;\
                                                buffer.resize(1  + sizeof(constValue));\
                                                buffer[0] = static_cast<uchar>(KoalaByte::ByteDataType::byte_data_type);\
                                                std::memcpy(&buffer[1], &constValue, sizeof(constValue));\
                                                m_constants.insert(buffer);\
                                                it = m_constants.find(buffer)\


    #define OPCODE(code) static_cast<size_t>(KoalaByte::OpCode::code)

    void Translator::Translate(){
        VisitCodeBlock(m_code);
    }

    void Translator::VisitCodeBlock(ASTCodeBlock& block){
        for(SHARED_PTR_T(ASTNode)& node : block.GetNodes()){
            //fn decl
            if(auto fn = dynamic_cast<ASTFunction*>(node.get())){
                m_regions_ptrs.insert({fn->GetFunctionName(), m_codes.size()});
                VisitCodeBlock(*(fn->GetBody().get()));
                continue;
            }

            //all other stuff
            if(auto block = dynamic_cast<ASTCodeBlock*>(node.get())){
                VisitCodeBlock(*block);
                continue;
            }

            if(auto retNode = dynamic_cast<ASTRet*>(node.get())){
                VisitExpression(retNode->GetReturnNode());
                m_codes.push_back(OPCODE(RET));
            }
        }
    }

    void Translator::VisitExpression(SHARED_PTR_T(ASTNode) node){
        m_codes.push_back(OPCODE(PUSH));
        const int idx = VisitConstant(node);
        if(idx == -1) throw std::runtime_error("Runtime error during translation: constant declaration failed!");
        m_codes.push_back(idx);
    }

    int Translator::VisitConstant(SHARED_PTR_T(ASTNode) constant){
        auto iter = m_constants.end();
        
        if(auto intConst = dynamic_cast<ASTNumberLiteral*>(constant.get())) { TRANSLATOR_CONSTANT_MAKING(intConst, unsigned long int, INT, iter); }

        return iter == m_constants.end() ? -1 : std::distance(m_constants.begin(), iter);
    }
}