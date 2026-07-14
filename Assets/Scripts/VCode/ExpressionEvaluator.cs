using System;

namespace GlitchCompiler.VCode
{
    public static class ExpressionEvaluator
    {
        public static RuntimeValue Evaluate(ExpressionNode node, ExecutionContext context)
        {
            if(node is NumberNode n) return new RuntimeValue(n.Value); if(node is StringNode s) return new RuntimeValue(s.Value); if(node is BoolNode b) return new RuntimeValue(b.Value); if(node is VariableNode v) return context.Get(v.Name);
            if(node is UnaryNode u) { var value=Evaluate(u.Value,context); return u.Operator==TokenType.Bang?new RuntimeValue(!value.AsBool()):new RuntimeValue(-Number(value)); }
            var binary=node as BinaryNode; var left=Evaluate(binary.Left,context); var right=Evaluate(binary.Right,context);
            switch(binary.Operator) { case TokenType.Plus:return new RuntimeValue(Number(left)+Number(right)); case TokenType.Minus:return new RuntimeValue(Number(left)-Number(right)); case TokenType.Star:return new RuntimeValue(Number(left)*Number(right)); case TokenType.Slash: if(Math.Abs(Number(right))<double.Epsilon) throw new VCodeRuntimeException("不可除以零。"); return new RuntimeValue(Number(left)/Number(right)); case TokenType.Greater:return new RuntimeValue(Number(left)>Number(right)); case TokenType.GreaterEqual:return new RuntimeValue(Number(left)>=Number(right)); case TokenType.Less:return new RuntimeValue(Number(left)<Number(right)); case TokenType.LessEqual:return new RuntimeValue(Number(left)<=Number(right)); case TokenType.EqualEqual:return new RuntimeValue(Equal(left,right)); case TokenType.BangEqual:return new RuntimeValue(!Equal(left,right)); default:throw new VCodeRuntimeException("不支援的運算子。"); }
        }
        public static double Number(RuntimeValue value) { if(value.Kind!=ValueKind.Number) throw new VCodeRuntimeException("此處需要數值。"); return value.Number; }
        private static bool Equal(RuntimeValue a, RuntimeValue b) => a.Kind==b.Kind && (a.Kind==ValueKind.Number?Math.Abs(a.Number-b.Number)<double.Epsilon:a.Kind==ValueKind.Bool?a.Bool==b.Bool:a.String==b.String);
    }
}
