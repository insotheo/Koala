#ifndef KOALA_LANG_AST_NODES_H
#define KOALA_LANG_AST_NODES_H

#include "Kernel.h"
#include <vector>
#include <string>

namespace KoalaLang{
    class KOALA_LANG_API ASTNode{
    public:
        ASTNode() {}
        virtual ~ASTNode() {}

        virtual void Use() {}
    };

    class KOALA_LANG_API ASTConstant : public ASTNode{
    public:
        ASTConstant(const std::string& c) : m_const(c)
        {}

        inline std::string& GetConst() { return m_const; }
    private:
        std::string m_const;
    };

    class KOALA_LANG_API ASTCodeBlock : public ASTNode{
    public:
        ASTCodeBlock(const std::vector<SHARED_PTR_T(ASTNode)>& nodes) : m_nodes(nodes)
        {}

        inline std::vector<SHARED_PTR_T(ASTNode)>& GetNodes() { return m_nodes; }

    private:
        std::vector<SHARED_PTR_T(ASTNode)> m_nodes;
    };

    class KOALA_LANG_API ASTFunction : public ASTNode{
    public:
        ASTFunction(const std::string& fnName, const std::string& retType, SHARED_PTR_T(ASTCodeBlock) body) 
        : m_function_name(fnName), m_return_type(retType), m_body(body)
        {}

    private:
        std::string m_function_name;
        std::string m_return_type;
        SHARED_PTR_T(ASTCodeBlock) m_body;
    };

    class KOALA_LANG_API ASTRet : public ASTNode{
    public:
        ASTRet(SHARED_PTR_T(ASTNode) node) : m_ret_node(node)
        {}

    private:
        SHARED_PTR_T(ASTNode) m_ret_node;
    };
}

#endif