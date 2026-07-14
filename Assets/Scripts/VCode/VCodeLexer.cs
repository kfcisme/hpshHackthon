using System;
using System.Collections.Generic;
using GlitchCompiler.Core;

namespace GlitchCompiler.VCode
{
    public sealed class VCodeLexer
    {
        private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>(StringComparer.OrdinalIgnoreCase) {
            { "FUNC", TokenType.Func }, { "LET", TokenType.Let }, { "LOOP", TokenType.Loop }, { "IF", TokenType.If }, { "ELSE", TokenType.Else },
            { "MOVE", TokenType.Move }, { "TURN", TokenType.Turn }, { "COLOR", TokenType.Color }, { "WIDTH", TokenType.Width }, { "CIRCLE", TokenType.Circle }, { "RECT", TokenType.Rect },
            { "SHIELD", TokenType.Shield }, { "SYSTEM", TokenType.System }, { "RESET", TokenType.Reset }, { "true", TokenType.True }, { "false", TokenType.False }
        };
        public List<VCodeDiagnostic> Diagnostics { get; } = new List<VCodeDiagnostic>();
        public List<Token> Lex(string source)
        {
            var result = new List<Token>(); Diagnostics.Clear();
            if (source == null) source = string.Empty;
            if (source.Length > RuntimeLimits.MaxSourceCharacters) { Diagnostics.Add(new VCodeDiagnostic(1, 1, "程式碼超過長度限制。")); return result; }
            int i=0, line=1, column=1;
            while (i < source.Length) {
                char c=source[i]; int start=column;
                if (char.IsWhiteSpace(c)) { if (c=='\n') { line++; column=1; } else column++; i++; continue; }
                if (c=='/' && i+1<source.Length && source[i+1]=='/') { while (i<source.Length && source[i]!='\n') { i++; column++; } continue; }
                if (char.IsLetter(c) || c=='_') { int begin=i; while (i<source.Length && (char.IsLetterOrDigit(source[i]) || source[i]=='_')) { i++; column++; } string word=source.Substring(begin,i-begin); result.Add(new Token(Keywords.TryGetValue(word,out var type)?type:TokenType.Identifier,word,line,start)); continue; }
                if (char.IsDigit(c) || (c=='.' && i+1<source.Length && char.IsDigit(source[i+1]))) { int begin=i; while (i<source.Length && (char.IsDigit(source[i]) || source[i]=='.')) { i++; column++; } result.Add(new Token(TokenType.Number,source.Substring(begin,i-begin),line,start)); continue; }
                if (c=='"') { i++; column++; int begin=i; while(i<source.Length && source[i]!='"' && source[i]!='\n') { i++; column++; } if(i>=source.Length || source[i]!='"') { Diagnostics.Add(new VCodeDiagnostic(line,start,"未結束的字串。")); continue; } result.Add(new Token(TokenType.String,source.Substring(begin,i-begin),line,start)); i++; column++; continue; }
                TokenType type=TokenType.End; bool known=true; switch(c) { case '(': type=TokenType.LeftParen; break; case ')': type=TokenType.RightParen; break; case '{': type=TokenType.LeftBrace; break; case '}': type=TokenType.RightBrace; break; case ',': type=TokenType.Comma; break; case ';': type=TokenType.Semicolon; break; case '.': type=TokenType.Dot; break; case '+': type=TokenType.Plus; break; case '-': type=TokenType.Minus; break; case '*': type=TokenType.Star; break; case '/': type=TokenType.Slash; break; case '!': type=(i+1<source.Length&&source[i+1]=='=')?TokenType.BangEqual:TokenType.Bang; break; case '=': type=(i+1<source.Length&&source[i+1]=='=')?TokenType.EqualEqual:TokenType.Equal; break; case '>': type=(i+1<source.Length&&source[i+1]=='=')?TokenType.GreaterEqual:TokenType.Greater; break; case '<': type=(i+1<source.Length&&source[i+1]=='=')?TokenType.LessEqual:TokenType.Less; break; default: known=false; break; }
                if(!known) Diagnostics.Add(new VCodeDiagnostic(line,column,$"不支援的字元 '{c}'。")); else { result.Add(new Token(type,c.ToString(),line,column)); if ((type==TokenType.BangEqual||type==TokenType.EqualEqual||type==TokenType.GreaterEqual||type==TokenType.LessEqual)) { i++; column++; } } i++; column++;
            }
            result.Add(new Token(TokenType.End,string.Empty,line,column)); return result;
        }
    }
}
