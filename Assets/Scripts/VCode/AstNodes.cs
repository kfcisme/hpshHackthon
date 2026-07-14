using System.Collections.Generic;

namespace GlitchCompiler.VCode
{
    public sealed class ProgramNode { public readonly List<FunctionNode> Functions = new List<FunctionNode>(); public readonly List<StatementNode> Statements = new List<StatementNode>(); }
    public sealed class FunctionNode { public string Name; public List<string> Parameters = new List<string>(); public List<StatementNode> Body = new List<StatementNode>(); }
    public abstract class StatementNode { public Token Token; }
    public sealed class CommandNode : StatementNode { public string Name; public List<ExpressionNode> Arguments = new List<ExpressionNode>(); }
    public sealed class CallNode : StatementNode { public string Name; public List<ExpressionNode> Arguments = new List<ExpressionNode>(); }
    public sealed class LetNode : StatementNode { public string Name; public ExpressionNode Value; }
    public sealed class LoopNode : StatementNode { public ExpressionNode Count; public List<StatementNode> Body = new List<StatementNode>(); }
    public sealed class IfNode : StatementNode { public ExpressionNode Condition; public List<StatementNode> Then = new List<StatementNode>(); public List<StatementNode> Else = new List<StatementNode>(); }
    public abstract class ExpressionNode { public Token Token; }
    public sealed class NumberNode : ExpressionNode { public double Value; }
    public sealed class StringNode : ExpressionNode { public string Value; }
    public sealed class BoolNode : ExpressionNode { public bool Value; }
    public sealed class VariableNode : ExpressionNode { public string Name; }
    public sealed class UnaryNode : ExpressionNode { public TokenType Operator; public ExpressionNode Value; }
    public sealed class BinaryNode : ExpressionNode { public ExpressionNode Left; public TokenType Operator; public ExpressionNode Right; }
}
