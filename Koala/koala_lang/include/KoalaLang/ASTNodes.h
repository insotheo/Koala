#ifndef KOALA_LANG_AST_NODES_H
#define KOALA_LANG_AST_NODES_H

#include "Kernel.h"
#include <vector>
#include <string>

namespace KoalaLang{
    class KOALA_LANG_API ASTNode{
    public:
        ASTNode() = default;
        virtual ~ASTNode() = default;
    };

    class KOALA_LANG_API ASTNumberLiteral final : public ASTNode{
    public:
        explicit ASTNumberLiteral(const long int val) : m_const(val)
        {}

        inline long int& GetConst() { return m_const; }
    private:
        long int m_const;
    };

    class KOALA_LANG_API ASTFloatLiteral final : public ASTNode{
    public:
        explicit ASTFloatLiteral(const double val) : m_const(val)
        {}

        inline double& GetConst() { return m_const; }
    private:
        double m_const;
    };

    enum BinOperation {
        Addition = 0x2, Subtraction = 0x3, Multiplication = 0x4, Division = 0x5, ReminderOnDivision = 0x6
    };

    class KOALA_LANG_API ASTBinaryOperation final : public ASTNode {
    public:
        ASTBinaryOperation(SHARED_PTR_T(ASTNode) left, const BinOperation op, SHARED_PTR_T(ASTNode) right)
        : m_left(std::move(left)), m_operation(op), m_right(std::move(right))
        {}

        inline SHARED_PTR_T(ASTNode) GetLeft() { return m_left; }
        inline SHARED_PTR_T(ASTNode) GetRight() { return m_right; }
        inline BinOperation GetOperation() { return m_operation; }
    private:
        SHARED_PTR_T(ASTNode) m_left;
        BinOperation m_operation;
        SHARED_PTR_T(ASTNode) m_right;
    };

    class KOALA_LANG_API ASTCodeBlock final : public ASTNode{
    public:
        explicit ASTCodeBlock(const std::vector<SHARED_PTR_T(ASTNode)>& nodes) : m_nodes(nodes)
        {}

        inline std::vector<SHARED_PTR_T(ASTNode)>& GetNodes() { return m_nodes; }

    private:
        std::vector<SHARED_PTR_T(ASTNode)> m_nodes;
    };

    class KOALA_LANG_API ASTFunction final : public ASTNode{
    public:
        ASTFunction(const std::string& fnName, const std::string& retType, SHARED_PTR_T(ASTCodeBlock) body)
        : m_function_name(fnName), m_return_type(retType), m_body(std::move(body))
        {}

        inline const std::string& GetFunctionName() { return m_function_name; }
        inline SHARED_PTR_T(ASTCodeBlock) GetBody() { return m_body; }
    private:
        std::string m_function_name;
        std::string m_return_type;
        SHARED_PTR_T(ASTCodeBlock) m_body;
    };

    class KOALA_LANG_API ASTRet final : public ASTNode{
    public:
        explicit ASTRet(SHARED_PTR_T(ASTNode) node) : m_ret_node(std::move(node))
        {}

        inline SHARED_PTR_T(ASTNode) GetReturnNode() { return m_ret_node; }
    private:
        SHARED_PTR_T(ASTNode) m_ret_node;
    };

    class KOALA_LANG_API ASTModule final: public ASTNode{
    public:
        ASTModule(SHARED_PTR_T(ASTCodeBlock) body, std::string  name)
            : m_name(std::move(name)), m_body(std::move(body))
        {}

        inline const std::string& GetName() { return m_name; }
        inline SHARED_PTR_T(ASTCodeBlock) GetBody() { return m_body; }
    private:
        std::string m_name;
        SHARED_PTR_T(ASTCodeBlock) m_body;
    };
}

#endif