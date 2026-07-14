using System;
using System.Collections.Generic;
using GlitchCompiler.Rendering;
using UnityEngine;

namespace GlitchCompiler.VCode
{
    public sealed class ExecutionResult { public readonly List<DrawCommand> DrawCommands=new List<DrawCommand>(); public readonly List<SystemCommand> SystemCommands=new List<SystemCommand>(); public readonly List<VCodeDiagnostic> Diagnostics=new List<VCodeDiagnostic>(); public int Instructions; public int RecursionDepth; public bool Success=>Diagnostics.Count==0; }
    public sealed class VCodeInterpreter
    {
        public ExecutionResult Execute(ProgramNode program, IDictionary<string, RuntimeValue> systemVariables=null)
        {
            var output=new ExecutionResult(); var context=new ExecutionContext();
            try { foreach(var function in program.Functions) { if(context.Functions.ContainsKey(function.Name)) throw new VCodeRuntimeException($"重複的函數 '{function.Name}'。"); context.Functions.Add(function.Name,function); } if(systemVariables!=null) foreach(var pair in systemVariables) context.Set(pair.Key,pair.Value); ExecuteBlock(program.Statements,context,output); output.Instructions=context.InstructionCount; output.RecursionDepth=context.RecursionDepth; }
            catch(VCodeRuntimeException ex) { output.Diagnostics.Add(new VCodeDiagnostic(0,0,ex.Message)); } return output;
        }
        private void ExecuteBlock(List<StatementNode> statements,ExecutionContext context,ExecutionResult output) { foreach(var statement in statements) { context.Step(); if(statement is LetNode let) context.Set(let.Name,ExpressionEvaluator.Evaluate(let.Value,context)); else if(statement is LoopNode loop) { int count=(int)Math.Floor(ExpressionEvaluator.Number(ExpressionEvaluator.Evaluate(loop.Count,context))); if(count<0) throw new VCodeRuntimeException("LOOP 次數不可為負數。"); for(int i=0;i<count;i++) ExecuteBlock(loop.Body,context,output); } else if(statement is IfNode conditional) { ExecuteBlock(ExpressionEvaluator.Evaluate(conditional.Condition,context).AsBool()?conditional.Then:conditional.Else,context,output); } else if(statement is CallNode call) ExecuteCall(call,context,output); else if(statement is CommandNode command) ExecuteCommand(command,context,output); } }
        private void ExecuteCall(CallNode call,ExecutionContext context,ExecutionResult output) { if(!context.Functions.TryGetValue(call.Name,out var function)) throw new VCodeRuntimeException($"未定義的函數 '{call.Name}'。"); if(function.Parameters.Count!=call.Arguments.Count) throw new VCodeRuntimeException($"函數 '{call.Name}' 的參數數量不符。"); var values=new RuntimeValue[call.Arguments.Count]; for(int i=0;i<values.Length;i++) values[i]=ExpressionEvaluator.Evaluate(call.Arguments[i],context); context.PushScope(); try { for(int i=0;i<values.Length;i++) context.Set(function.Parameters[i],values[i]); ExecuteBlock(function.Body,context,output); } finally { context.PopScope(); } }
        private void ExecuteCommand(CommandNode command,ExecutionContext context,ExecutionResult output)
        {
            double Number(int index) => ExpressionEvaluator.Number(ExpressionEvaluator.Evaluate(command.Arguments[index],context));
            if(command.Name=="SYSTEM.RESET") { output.SystemCommands.Add(new SystemCommand(SystemCommandType.Reset)); return; }
            if(command.Name=="SHIELD") { output.SystemCommands.Add(new SystemCommand(SystemCommandType.Shield,ExpressionEvaluator.Evaluate(command.Arguments[0],context).AsBool())); return; }
            switch(command.Name) { case "MOVE": output.DrawCommands.Add(new DrawCommand(DrawCommandType.Move,(float)Number(0))); break; case "TURN": output.DrawCommands.Add(new DrawCommand(DrawCommandType.Turn,(float)Number(0))); break; case "WIDTH": output.DrawCommands.Add(new DrawCommand(DrawCommandType.Width,(float)Number(0))); break; case "CIRCLE": output.DrawCommands.Add(new DrawCommand(DrawCommandType.Circle,(float)Number(0))); break; case "RECT": output.DrawCommands.Add(new DrawCommand(DrawCommandType.Rect,(float)Number(0),(float)Number(1))); break; case "COLOR": var value=ExpressionEvaluator.Evaluate(command.Arguments[0],context); if(value.Kind!=ValueKind.String || !ColorUtility.TryParseHtmlString(value.String,out var color)) throw new VCodeRuntimeException("COLOR 需要有效的 #RRGGBB 色碼。"); output.DrawCommands.Add(new DrawCommand(DrawCommandType.Color,color:color)); break; default: throw new VCodeRuntimeException($"不支援的指令 '{command.Name}'。"); }
        }
    }
}
