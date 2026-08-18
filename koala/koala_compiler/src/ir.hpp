#pragma once

#include "lexer/token.hpp"
#include <memory>
#include <opcodes.h>
#include <string>
#include <variant>
#include <cstdint>
#include <vector>

namespace koalac{

    struct IRNode{
        struct Span Span;

        IRNode(struct Span span) : Span(span)
        {}

        virtual ~IRNode() = default;
    };

    using IRArg = std::variant<uint8_t, uint16_t, uint64_t, std::string>;

    struct IRInstruction : public IRNode{
        OpCode Op;
        std::vector<IRArg> Args;

        IRInstruction(OpCode op, std::vector<IRArg> args, struct Span span)
        : Op(op), Args(std::move(args)), IRNode(span)
        {}

        size_t GetSize();

        ~IRInstruction() override = default;
    };

    struct IRLabel : public IRNode{
        std::string Label;

        IRLabel(const std::string& label, struct Span span)
        : Label(label), IRNode(span)
        {}

        ~IRLabel() override = default;
    };

    using IRNodes = std::vector<std::unique_ptr<IRNode>>;

    class IRProgram{
    public:
        IRProgram(IRNodes nodes) : m_Nodes(std::move(nodes))
        {}

        ~IRProgram() = default;

        inline const IRNodes& GetNodes() const { return m_Nodes; }
    private:
        IRNodes m_Nodes;
    };

}