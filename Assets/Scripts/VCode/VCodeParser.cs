using System;
using System.Collections.Generic;
using System.Globalization;

namespace GlitchCompiler.VCode
{
    public sealed class VCodeParser
    {
        private List<Token> tokens; private int current; private VCodeParseResult result;
        public VCodeParseResult Parse(string source)
        {
            var lexer = new VCodeLexer(); tokens = lexer.Lex(source); current = 0; result = new VCodeParseResult(); result.Diagnostics.AddRange(lexer.Diagnostics);
            if (result.Diagnostics.Count > 0) return result;
            var program = new ProgramNode();
            while (!AtEnd()) { if (Match(TokenType.Func)) program.Functions.Add(ParseFunction()); else program.Statements.Add(ParseStatement()); }
            result.Program = result.Diagnostics.Count == 0 ? program : null; return result;
        }
        private FunctionNode ParseFunction()
        {
            var name=Consume(TokenType.Identifier,"FUNC 後需要函數名稱。"); var node=new FunctionNode { Name=name.Lexeme };
            Consume(TokenType.LeftParen,"函數名稱後需要 '('"); if(!Check(TokenType.RightParen)) do { node.Parameters.Add(Consume(TokenType.Identifier,"參數必須是名稱。").Lexeme); } while(Match(TokenType.Comma)); Consume(TokenType.RightParen,"函數參數後需要 ')'"); node.Body=ParseBlock(); return node;
        }
        private List<StatementNode> ParseBlock() { Consume(TokenType.LeftBrace,"需要 '{'。"); var statements=new List<StatementNode>(); while(!Check(TokenType.RightBrace)&&!AtEnd()) statements.Add(ParseStatement()); Consume(TokenType.RightBrace,"區塊需要 '}'。"); return statements; }
        private StatementNode ParseStatement()
        {
            if(Match(TokenType.Let)) { var token=Previous(); var name=Consume(TokenType.Identifier,"LET 後需要變數名稱。"); Consume(TokenType.Equal,"變數名稱後需要 '='。"); var value=Expression(); Consume(TokenType.Semicolon,"LET 陳述式後需要 ';'。"); return new LetNode { Token=token,Name=name.Lexeme,Value=value }; }
            if(Match(TokenType.Loop)) { var token=Previous(); Consume(TokenType.LeftParen,"LOOP 後需要 '('。"); var count=Expression(); Consume(TokenType.RightParen,"LOOP 條件後需要 ')'。"); return new LoopNode { Token=token,Count=count,Body=ParseBlock() }; }
            if(Match(TokenType.If)) { var token=Previous(); Consume(TokenType.LeftParen,"IF 後需要 '('。"); var condition=Expression(); Consume(TokenType.RightParen,"IF 條件後需要 ')'。"); var node=new IfNode { Token=token,Condition=condition,Then=ParseBlock() }; if(Match(TokenType.Else)) node.Else=ParseBlock(); return node; }
            var first=Advance();
            if(first.Type==TokenType.System) { Consume(TokenType.Dot,"SYSTEM 後需要 '.'。"); Consume(TokenType.Reset,"只支援 SYSTEM.RESET()。"); Consume(TokenType.LeftParen,"RESET 後需要 '('。"); Consume(TokenType.RightParen,"RESET 後需要 ')'。"); Consume(TokenType.Semicolon,"指令後需要 ';'。"); return new CommandNode { Token=first,Name="SYSTEM.RESET" }; }
            if (!IsCallable(first.Type)) { Error(first,"預期指令、函數呼叫或控制結構。"); Synchronize(); return new CommandNode { Token=first,Name="INVALID" }; }
            Consume(TokenType.LeftParen,"指令名稱後需要 '('。"); var args=Arguments(); Consume(TokenType.RightParen,"指令參數後需要 ')'。"); Consume(TokenType.Semicolon,"指令後需要 ';'。");
            string name=first.Lexeme.ToUpperInvariant(); bool command=first.Type!=TokenType.Identifier;
            if(command) return new CommandNode { Token=first,Name=name,Arguments=args }; return new CallNode { Token=first,Name=first.Lexeme,Arguments=args };
        }
        private List<ExpressionNode> Arguments() { var args=new List<ExpressionNode>(); if(!Check(TokenType.RightParen)) do { args.Add(Expression()); } while(Match(TokenType.Comma)); return args; }
        private ExpressionNode Expression() => Equality();
        private ExpressionNode Equality() { var e=Comparison(); while(Match(TokenType.EqualEqual,TokenType.BangEqual)) { var op=Previous(); e=new BinaryNode { Token=op,Left=e,Operator=op.Type,Right=Comparison() }; } return e; }
        private ExpressionNode Comparison() { var e=Term(); while(Match(TokenType.Greater,TokenType.GreaterEqual,TokenType.Less,TokenType.LessEqual)) { var op=Previous(); e=new BinaryNode { Token=op,Left=e,Operator=op.Type,Right=Term() }; } return e; }
        private ExpressionNode Term() { var e=Factor(); while(Match(TokenType.Plus,TokenType.Minus)) { var op=Previous(); e=new BinaryNode { Token=op,Left=e,Operator=op.Type,Right=Factor() }; } return e; }
        private ExpressionNode Factor() { var e=Unary(); while(Match(TokenType.Star,TokenType.Slash)) { var op=Previous(); e=new BinaryNode { Token=op,Left=e,Operator=op.Type,Right=Unary() }; } return e; }
        private ExpressionNode Unary() { if(Match(TokenType.Minus,TokenType.Bang)) { var op=Previous(); return new UnaryNode { Token=op,Operator=op.Type,Value=Unary() }; } return Primary(); }
        private ExpressionNode Primary() { var token=Advance(); if(token.Type==TokenType.Number && double.TryParse(token.Lexeme,NumberStyles.Float,CultureInfo.InvariantCulture,out var n)) return new NumberNode { Token=token,Value=n }; if(token.Type==TokenType.String) return new StringNode { Token=token,Value=token.Lexeme }; if(token.Type==TokenType.True||token.Type==TokenType.False) return new BoolNode { Token=token,Value=token.Type==TokenType.True }; if(token.Type==TokenType.Identifier) return new VariableNode { Token=token,Name=token.Lexeme }; if(token.Type==TokenType.LeftParen) { var value=Expression(); Consume(TokenType.RightParen,"運算式後需要 ')'。"); return value; } Error(token,"預期數值、字串、布林值或變數。"); return new NumberNode { Token=token,Value=0 }; }
        private bool IsCallable(TokenType type) => type==TokenType.Identifier||type==TokenType.Move||type==TokenType.Turn||type==TokenType.Color||type==TokenType.Width||type==TokenType.Circle||type==TokenType.Rect||type==TokenType.Shield;
        private Token Consume(TokenType type,string message) { if(Check(type)) return Advance(); Error(Peek(),message); return new Token(type,string.Empty,Peek().Line,Peek().Column); }
        private void Error(Token token,string message) { result.Diagnostics.Add(new VCodeDiagnostic(token.Line,token.Column,message)); }
        private void Synchronize() { while(!AtEnd()&&!Check(TokenType.Semicolon)&&!Check(TokenType.RightBrace)) Advance(); if(Check(TokenType.Semicolon)) Advance(); }
        private bool Match(params TokenType[] types) { foreach(var type in types) if(Check(type)) { Advance(); return true; } return false; }
        private bool Check(TokenType type) => !AtEnd()&&Peek().Type==type;
        private bool AtEnd() => Peek().Type==TokenType.End; private Token Advance() { if(!AtEnd()) current++; return Previous(); } private Token Peek()=>tokens[current]; private Token Previous()=>tokens[current-1];
    }
}
