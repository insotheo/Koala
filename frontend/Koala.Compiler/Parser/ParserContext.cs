using System;
using System.Collections.Generic;
using Koala.Compiler.Lexer;
using Koala.Compiler.Parser.ASTNodes;

namespace Koala.Compiler.Parser;

class ParserContext
{
    private readonly string _filePath;
    private int _idx;
    private readonly Token[] _tokens;

    private readonly List<INode> _parsed;
    private readonly List<string> _errors;

    internal Token Current => _tokens[_idx];
    internal TokenType CurrentType => _tokens[_idx].Type;

    internal ParserContext(string path, Lexer.Lexer lexer)
    {
        _filePath = path;
        _idx = 0;
        _tokens = lexer.Tokens.ToArray();
        _errors = new();
        _parsed = new();
    }

    internal void Push(INode node) => _parsed.Add(node);
    internal void Next()
    {
        _idx++;
        if (_idx >= _tokens.Length) _idx = _tokens.Length - 1;
    }
    internal void Back() => _idx--;
    internal bool IsNext(TokenType type)
    {
        Next();
        return CurrentType == type;
    }
    internal bool TryCurrent(TokenType expectation)
    {
        if (CurrentType != expectation)
        {
            Panic($"Unexpected token {CurrentType}. Expected: {expectation}");
            return false;
        }
        return true;
    }
    internal bool TryNext(TokenType expectation)
    {
        Next();
        return TryCurrent(expectation);
    }
    internal void Panic(string msg) => _errors.Add($"At ({Current.Line}; {Current.Col}): {msg}");

    internal int GetErrorsCount() => _errors.Count;
    internal void PrintErrors()
    {
        if (_errors.Count == 0) return;
        string errorStirng = _errors.Count > 1 ? "errors" : "error";
        Console.Error.WriteLine($"{_errors.Count} parser's {errorStirng} occurred at \"{_filePath}\":");
        foreach (string err in _errors)
        {
            Console.Error.WriteLine("\t" + err);
        }
    }

    internal void SkipUntil(params TokenType[] stopTokens)
    {
        while (_idx < _tokens.Length && Array.IndexOf(stopTokens, CurrentType) < 0)
        {
            Next();
        }
    }
}