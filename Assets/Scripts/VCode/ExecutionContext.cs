using System;
using System.Collections.Generic;
using GlitchCompiler.Core;

namespace GlitchCompiler.VCode
{
    public sealed class ExecutionContext
    {
        private readonly Stack<Dictionary<string, RuntimeValue>> scopes = new Stack<Dictionary<string, RuntimeValue>>();
        public readonly Dictionary<string, FunctionNode> Functions = new Dictionary<string, FunctionNode>(StringComparer.OrdinalIgnoreCase);
        public int InstructionCount { get; private set; } public int RecursionDepth => scopes.Count;
        public ExecutionContext() { scopes.Push(new Dictionary<string, RuntimeValue>(StringComparer.OrdinalIgnoreCase)); }
        public void Step() { if(++InstructionCount > RuntimeLimits.MaxInstructions) throw new VCodeRuntimeException("執行指令超過上限。"); }
        public void PushScope() { if(scopes.Count>=RuntimeLimits.MaxRecursionDepth) throw new VCodeRuntimeException("遞迴深度超過上限。"); scopes.Push(new Dictionary<string, RuntimeValue>(StringComparer.OrdinalIgnoreCase)); }
        public void PopScope() => scopes.Pop(); public void Set(string name, RuntimeValue value) => scopes.Peek()[name]=value;
        public RuntimeValue Get(string name) { foreach(var scope in scopes) if(scope.TryGetValue(name,out var value)) return value; throw new VCodeRuntimeException($"未定義的變數 '{name}'。"); }
    }
    public readonly struct RuntimeValue { public readonly double Number; public readonly string String; public readonly bool Bool; public readonly ValueKind Kind; public RuntimeValue(double value) { Number=value; String=null; Bool=false; Kind=ValueKind.Number; } public RuntimeValue(string value) { Number=0; String=value; Bool=false; Kind=ValueKind.String; } public RuntimeValue(bool value) { Number=0; String=null; Bool=value; Kind=ValueKind.Bool; } public bool AsBool()=>Kind==ValueKind.Bool?Bool:Math.Abs(Number)>double.Epsilon; }
    public enum ValueKind { Number, String, Bool }
    public sealed class VCodeRuntimeException : Exception { public VCodeRuntimeException(string message) : base(message) { } }
}
