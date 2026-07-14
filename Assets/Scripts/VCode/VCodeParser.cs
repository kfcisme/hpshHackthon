using System.Collections.Generic;
using System.Globalization;

namespace GlitchCompiler.VCode
{
    public sealed class VCodeParser
    {
        private List<Token> tokens;
        private int current;
        private VCodeParseResult result;

        public VCodeParseResult Parse(string source)
        {
            var lexer = new VCodeLexer();
            tokens = lexer.Lex(source);
            current = 0;
            result = new VCodeParseResult();
            result.Diagnostics.AddRange(lexer.Diagnostics);

            if (result.Diagnostics.Count > 0)
            {
                return result;
            }

            var program = new ProgramNode();
            while (!AtEnd())
            {
                if (Match(TokenType.Func))
                {
                    program.Functions.Add(ParseFunction());
                }
                else
                {
                    program.Statements.Add(ParseStatement());
                }
            }

            result.Program = result.Diagnostics.Count == 0 ? program : null;
            return result;
        }

        private FunctionNode ParseFunction()
        {
            var functionNameToken = Consume(TokenType.Identifier, "FUNC 後需要函數名稱。");
            var function = new FunctionNode { Name = functionNameToken.Lexeme };

            Consume(TokenType.LeftParen, "函數名稱後需要 '('");
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    function.Parameters.Add(Consume(TokenType.Identifier, "參數必須是名稱。").Lexeme);
                }
                while (Match(TokenType.Comma));
            }

            Consume(TokenType.RightParen, "函數參數後需要 ')'");
            function.Body = ParseBlock();
            return function;
        }

        private List<StatementNode> ParseBlock()
        {
            Consume(TokenType.LeftBrace, "需要 '{'。");
            var statements = new List<StatementNode>();
            while (!Check(TokenType.RightBrace) && !AtEnd())
            {
                statements.Add(ParseStatement());
            }

            Consume(TokenType.RightBrace, "區塊需要 '}'。");
            return statements;
        }

        private StatementNode ParseStatement()
        {
            if (Match(TokenType.Let))
            {
                var letToken = Previous();
                var variableNameToken = Consume(TokenType.Identifier, "LET 後需要變數名稱。");
                Consume(TokenType.Equal, "變數名稱後需要 '='。");
                var value = Expression();
                Consume(TokenType.Semicolon, "LET 陳述式後需要 ';'。");
                return new LetNode { Token = letToken, Name = variableNameToken.Lexeme, Value = value };
            }

            if (Match(TokenType.Loop))
            {
                var loopToken = Previous();
                Consume(TokenType.LeftParen, "LOOP 後需要 '('。");
                var count = Expression();
                Consume(TokenType.RightParen, "LOOP 條件後需要 ')'。");
                return new LoopNode { Token = loopToken, Count = count, Body = ParseBlock() };
            }

            if (Match(TokenType.If))
            {
                var ifToken = Previous();
                Consume(TokenType.LeftParen, "IF 後需要 '('。");
                var condition = Expression();
                Consume(TokenType.RightParen, "IF 條件後需要 ')'。");
                var conditional = new IfNode { Token = ifToken, Condition = condition, Then = ParseBlock() };
                if (Match(TokenType.Else))
                {
                    conditional.Else = ParseBlock();
                }

                return conditional;
            }

            var firstToken = Advance();
            if (firstToken.Type == TokenType.System)
            {
                Consume(TokenType.Dot, "SYSTEM 後需要 '.'。");
                Consume(TokenType.Reset, "只支援 SYSTEM.RESET()。");
                Consume(TokenType.LeftParen, "RESET 後需要 '('。");
                Consume(TokenType.RightParen, "RESET 後需要 ')'。");
                Consume(TokenType.Semicolon, "指令後需要 ';'。");
                return new CommandNode { Token = firstToken, Name = "SYSTEM.RESET" };
            }

            if (!IsCallable(firstToken.Type))
            {
                Error(firstToken, "預期指令、函數呼叫或控制結構。");
                Synchronize();
                return new CommandNode { Token = firstToken, Name = "INVALID" };
            }

            Consume(TokenType.LeftParen, "指令名稱後需要 '('。");
            var arguments = Arguments();
            Consume(TokenType.RightParen, "指令參數後需要 ')'。");
            Consume(TokenType.Semicolon, "指令後需要 ';'。");

            var callableName = firstToken.Lexeme.ToUpperInvariant();
            var isCommand = firstToken.Type != TokenType.Identifier;
            return isCommand
                ? new CommandNode { Token = firstToken, Name = callableName, Arguments = arguments }
                : new CallNode { Token = firstToken, Name = firstToken.Lexeme, Arguments = arguments };
        }

        private List<ExpressionNode> Arguments()
        {
            var arguments = new List<ExpressionNode>();
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    arguments.Add(Expression());
                }
                while (Match(TokenType.Comma));
            }

            return arguments;
        }

        private ExpressionNode Expression() => Equality();

        private ExpressionNode Equality()
        {
            var expression = Comparison();
            while (Match(TokenType.EqualEqual, TokenType.BangEqual))
            {
                var operation = Previous();
                expression = new BinaryNode { Token = operation, Left = expression, Operator = operation.Type, Right = Comparison() };
            }

            return expression;
        }

        private ExpressionNode Comparison()
        {
            var expression = Term();
            while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                var operation = Previous();
                expression = new BinaryNode { Token = operation, Left = expression, Operator = operation.Type, Right = Term() };
            }

            return expression;
        }

        private ExpressionNode Term()
        {
            var expression = Factor();
            while (Match(TokenType.Plus, TokenType.Minus))
            {
                var operation = Previous();
                expression = new BinaryNode { Token = operation, Left = expression, Operator = operation.Type, Right = Factor() };
            }

            return expression;
        }

        private ExpressionNode Factor()
        {
            var expression = Unary();
            while (Match(TokenType.Star, TokenType.Slash))
            {
                var operation = Previous();
                expression = new BinaryNode { Token = operation, Left = expression, Operator = operation.Type, Right = Unary() };
            }

            return expression;
        }

        private ExpressionNode Unary()
        {
            if (Match(TokenType.Minus, TokenType.Bang))
            {
                var operation = Previous();
                return new UnaryNode { Token = operation, Operator = operation.Type, Value = Unary() };
            }

            return Primary();
        }

        private ExpressionNode Primary()
        {
            var token = Advance();
            if (token.Type == TokenType.Number && double.TryParse(token.Lexeme, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                return new NumberNode { Token = token, Value = number };
            }

            if (token.Type == TokenType.String)
            {
                return new StringNode { Token = token, Value = token.Lexeme };
            }

            if (token.Type == TokenType.True || token.Type == TokenType.False)
            {
                return new BoolNode { Token = token, Value = token.Type == TokenType.True };
            }

            if (token.Type == TokenType.Identifier)
            {
                return new VariableNode { Token = token, Name = token.Lexeme };
            }

            if (token.Type == TokenType.LeftParen)
            {
                var expression = Expression();
                Consume(TokenType.RightParen, "運算式後需要 ')'。");
                return expression;
            }

            Error(token, "預期數值、字串、布林值或變數。");
            return new NumberNode { Token = token, Value = 0 };
        }

        private bool IsCallable(TokenType tokenType) =>
            tokenType == TokenType.Identifier || tokenType == TokenType.Move || tokenType == TokenType.Turn ||
            tokenType == TokenType.Color || tokenType == TokenType.Width || tokenType == TokenType.Circle ||
            tokenType == TokenType.Rect || tokenType == TokenType.Shield;

        private Token Consume(TokenType expectedType, string message)
        {
            if (Check(expectedType))
            {
                return Advance();
            }

            Error(Peek(), message);
            return new Token(expectedType, string.Empty, Peek().Line, Peek().Column);
        }

        private void Error(Token token, string message) => result.Diagnostics.Add(new VCodeDiagnostic(token.Line, token.Column, message));

        private void Synchronize()
        {
            while (!AtEnd() && !Check(TokenType.Semicolon) && !Check(TokenType.RightBrace))
            {
                Advance();
            }

            if (Check(TokenType.Semicolon))
            {
                Advance();
            }
        }

        private bool Match(params TokenType[] expectedTypes)
        {
            foreach (var expectedType in expectedTypes)
            {
                if (Check(expectedType))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType expectedType) => !AtEnd() && Peek().Type == expectedType;
        private bool AtEnd() => Peek().Type == TokenType.End;

        private Token Advance()
        {
            if (!AtEnd())
            {
                current++;
            }

            return Previous();
        }

        private Token Peek() => tokens[current];
        private Token Previous() => tokens[current - 1];
    }
}
