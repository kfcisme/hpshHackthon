using System;
using System.Collections.Generic;
using GlitchCompiler.Rendering;
using UnityEngine;

namespace GlitchCompiler.VCode
{
    public sealed class ExecutionResult
    {
        public readonly List<DrawCommand> DrawCommands = new List<DrawCommand>();
        public readonly List<SystemCommand> SystemCommands = new List<SystemCommand>();
        public readonly List<VCodeDiagnostic> Diagnostics = new List<VCodeDiagnostic>();

        public int Instructions;
        public int RecursionDepth;
        public bool Success => Diagnostics.Count == 0;
    }

    public sealed class VCodeInterpreter
    {
        public ExecutionResult Execute(ProgramNode program, IDictionary<string, RuntimeValue> systemVariables = null)
        {
            var output = new ExecutionResult();
            if (program == null)
            {
                output.Diagnostics.Add(new VCodeDiagnostic(0, 0, "沒有可執行的程式。"));
                return output;
            }

            var context = new ExecutionContext();
            try
            {
                foreach (var function in program.Functions)
                {
                    if (context.Functions.ContainsKey(function.Name))
                    {
                        throw new VCodeRuntimeException($"函數 '{function.Name}' 被重複宣告。");
                    }

                    context.Functions.Add(function.Name, function);
                }

                if (systemVariables != null)
                {
                    foreach (var pair in systemVariables)
                    {
                        context.Set(pair.Key, pair.Value);
                    }
                }

                ExecuteBlock(program.Statements, context, output);
            }
            catch (VCodeRuntimeException exception)
            {
                output.Diagnostics.Add(new VCodeDiagnostic(0, 0, exception.Message));
            }

            output.Instructions = context.InstructionCount;
            output.RecursionDepth = context.RecursionDepth;
            return output;
        }

        private void ExecuteBlock(List<StatementNode> statements, ExecutionContext context, ExecutionResult output)
        {
            foreach (var statement in statements)
            {
                context.Step();
                if (statement is LetNode let)
                {
                    context.Set(let.Name, ExpressionEvaluator.Evaluate(let.Value, context));
                }
                else if (statement is LoopNode loop)
                {
                    var count = Number(ExpressionEvaluator.Evaluate(loop.Count, context), "LOOP 次數");
                    if (count < 0 || Math.Floor(count) != count)
                    {
                        throw new VCodeRuntimeException("LOOP 次數必須是非負整數。");
                    }

                    for (var index = 0; index < count; index++)
                    {
                        // Count every iteration, including an empty body. Without
                        // this, LOOP(large_number) { } can freeze Unity forever.
                        context.Step();
                        ExecuteBlock(loop.Body, context, output);
                    }
                }
                else if (statement is IfNode conditional)
                {
                    var branch = ExpressionEvaluator.Evaluate(conditional.Condition, context).AsBool()
                        ? conditional.Then
                        : conditional.Else;
                    ExecuteBlock(branch, context, output);
                }
                else if (statement is CallNode call)
                {
                    ExecuteCall(call, context, output);
                }
                else if (statement is CommandNode command)
                {
                    ExecuteCommand(command, context, output);
                }
            }
        }

        private void ExecuteCall(CallNode call, ExecutionContext context, ExecutionResult output)
        {
            if (!context.Functions.TryGetValue(call.Name, out var function))
            {
                throw new VCodeRuntimeException($"找不到函數 '{call.Name}'。");
            }

            if (function.Parameters.Count != call.Arguments.Count)
            {
                throw new VCodeRuntimeException($"函數 '{call.Name}' 預期 {function.Parameters.Count} 個參數，收到 {call.Arguments.Count} 個。");
            }

            var values = new RuntimeValue[call.Arguments.Count];
            for (var index = 0; index < values.Length; index++)
            {
                values[index] = ExpressionEvaluator.Evaluate(call.Arguments[index], context);
            }

            context.PushScope();
            try
            {
                for (var index = 0; index < values.Length; index++)
                {
                    context.Set(function.Parameters[index], values[index]);
                }

                ExecuteBlock(function.Body, context, output);
            }
            finally
            {
                context.PopScope();
            }
        }

        private void ExecuteCommand(CommandNode command, ExecutionContext context, ExecutionResult output)
        {
            switch (command.Name)
            {
                case "SYSTEM.RESET":
                    RequireArgumentCount(command, 0);
                    output.SystemCommands.Add(new SystemCommand(SystemCommandType.Reset));
                    return;

                case "SHIELD":
                    RequireArgumentCount(command, 1);
                    var shield = ExpressionEvaluator.Evaluate(command.Arguments[0], context);
                    if (shield.Kind != ValueKind.Bool)
                    {
                        throw new VCodeRuntimeException("SHIELD 需要 true 或 false。");
                    }

                    output.SystemCommands.Add(new SystemCommand(SystemCommandType.Shield, shield.Bool));
                    return;

                case "MOVE":
                    RequireArgumentCount(command, 1);
                    output.DrawCommands.Add(new DrawCommand(DrawCommandType.Move, ToFiniteFloat(command, context, 0, "MOVE 距離")));
                    return;

                case "TURN":
                    RequireArgumentCount(command, 1);
                    output.DrawCommands.Add(new DrawCommand(DrawCommandType.Turn, ToFiniteFloat(command, context, 0, "TURN 角度")));
                    return;

                case "WIDTH":
                    RequireArgumentCount(command, 1);
                    var width = ToFiniteFloat(command, context, 0, "WIDTH 寬度");
                    if (width <= 0)
                    {
                        throw new VCodeRuntimeException("WIDTH 必須大於 0。");
                    }

                    output.DrawCommands.Add(new DrawCommand(DrawCommandType.Width, width));
                    return;

                case "CIRCLE":
                    RequireArgumentCount(command, 1);
                    var radius = ToFiniteFloat(command, context, 0, "CIRCLE 半徑");
                    if (radius <= 0)
                    {
                        throw new VCodeRuntimeException("CIRCLE 半徑必須大於 0。");
                    }

                    output.DrawCommands.Add(new DrawCommand(DrawCommandType.Circle, radius));
                    return;

                case "RECT":
                    RequireArgumentCount(command, 2);
                    var rectangleWidth = ToFiniteFloat(command, context, 0, "RECT 寬度");
                    var rectangleHeight = ToFiniteFloat(command, context, 1, "RECT 高度");
                    if (Mathf.Approximately(rectangleWidth, 0) || Mathf.Approximately(rectangleHeight, 0))
                    {
                        throw new VCodeRuntimeException("RECT 的寬度與高度不可為 0。");
                    }

                    output.DrawCommands.Add(new DrawCommand(DrawCommandType.Rect, rectangleWidth, rectangleHeight));
                    return;

                case "COLOR":
                    RequireArgumentCount(command, 1);
                    var colorValue = ExpressionEvaluator.Evaluate(command.Arguments[0], context);
                    if (colorValue.Kind != ValueKind.String || !TryParseColor(colorValue.String, out var color))
                    {
                        throw new VCodeRuntimeException("COLOR 需要 #RRGGBB、#RRGGBBAA 或支援的顏色名稱。");
                    }

                    output.DrawCommands.Add(new DrawCommand(DrawCommandType.Color, color: color));
                    return;

                default:
                    throw new VCodeRuntimeException($"不支援的指令 '{command.Name}'。");
            }
        }

        private static void RequireArgumentCount(CommandNode command, int expectedCount)
        {
            if (command.Arguments.Count != expectedCount)
            {
                throw new VCodeRuntimeException($"{command.Name} 預期 {expectedCount} 個參數，收到 {command.Arguments.Count} 個。");
            }
        }

        private static float ToFiniteFloat(CommandNode command, ExecutionContext context, int argumentIndex, string label)
        {
            var number = Number(ExpressionEvaluator.Evaluate(command.Arguments[argumentIndex], context), label);
            if (double.IsNaN(number) || double.IsInfinity(number) || number > float.MaxValue || number < float.MinValue)
            {
                throw new VCodeRuntimeException($"{label} 必須是有限數值。");
            }

            return (float)number;
        }

        private static double Number(RuntimeValue value, string label)
        {
            if (value.Kind != ValueKind.Number)
            {
                throw new VCodeRuntimeException($"{label} 必須是數值。");
            }

            return value.Number;
        }

        private static bool TryParseColor(string value, out Color color)
        {
            if (ColorUtility.TryParseHtmlString(value, out color))
            {
                return true;
            }

            switch (value.ToLowerInvariant())
            {
                case "black": color = Color.black; return true;
                case "white": color = Color.white; return true;
                case "red": color = Color.red; return true;
                case "green": color = Color.green; return true;
                case "blue": color = Color.blue; return true;
                case "yellow": color = Color.yellow; return true;
                case "cyan": color = Color.cyan; return true;
                case "magenta": color = Color.magenta; return true;
                case "gray":
                case "grey": color = Color.gray; return true;
                case "clear": color = Color.clear; return true;
                default: color = default; return false;
            }
        }
    }
}
