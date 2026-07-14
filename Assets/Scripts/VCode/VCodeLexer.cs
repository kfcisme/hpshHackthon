using System;
using System.Collections.Generic;
using GlitchCompiler.Core;

namespace GlitchCompiler.VCode
{
    public sealed class VCodeLexer
    {
        private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>(StringComparer.OrdinalIgnoreCase)
        {
            { "FUNC", TokenType.Func }, { "LET", TokenType.Let }, { "LOOP", TokenType.Loop }, { "IF", TokenType.If }, { "ELSE", TokenType.Else },
            { "MOVE", TokenType.Move }, { "TURN", TokenType.Turn }, { "COLOR", TokenType.Color }, { "WIDTH", TokenType.Width }, { "CIRCLE", TokenType.Circle }, { "RECT", TokenType.Rect },
            { "SHIELD", TokenType.Shield }, { "SYSTEM", TokenType.System }, { "RESET", TokenType.Reset }, { "true", TokenType.True }, { "false", TokenType.False }
        };

        public List<VCodeDiagnostic> Diagnostics { get; } = new List<VCodeDiagnostic>();

        public List<Token> Lex(string source)
        {
            var result = new List<Token>();
            Diagnostics.Clear();
            source = source ?? string.Empty;

            if (source.Length > RuntimeLimits.MaxSourceCharacters)
            {
                Diagnostics.Add(new VCodeDiagnostic(1, 1, "程式碼超過長度限制。"));
                return result;
            }

            var index = 0;
            var line = 1;
            var column = 1;
            while (index < source.Length)
            {
                var character = source[index];
                var startColumn = column;

                if (char.IsWhiteSpace(character))
                {
                    if (character == '\n')
                    {
                        line++;
                        column = 1;
                    }
                    else
                    {
                        column++;
                    }

                    index++;
                    continue;
                }

                if (character == '/' && index + 1 < source.Length && source[index + 1] == '/')
                {
                    while (index < source.Length && source[index] != '\n')
                    {
                        index++;
                        column++;
                    }

                    continue;
                }

                if (char.IsLetter(character) || character == '_')
                {
                    var startIndex = index;
                    while (index < source.Length && (char.IsLetterOrDigit(source[index]) || source[index] == '_'))
                    {
                        index++;
                        column++;
                    }

                    var word = source.Substring(startIndex, index - startIndex);
                    var tokenType = Keywords.TryGetValue(word, out var keywordTokenType) ? keywordTokenType : TokenType.Identifier;
                    result.Add(new Token(tokenType, word, line, startColumn));
                    continue;
                }

                if (char.IsDigit(character) || (character == '.' && index + 1 < source.Length && char.IsDigit(source[index + 1])))
                {
                    var startIndex = index;
                    while (index < source.Length && (char.IsDigit(source[index]) || source[index] == '.'))
                    {
                        index++;
                        column++;
                    }

                    result.Add(new Token(TokenType.Number, source.Substring(startIndex, index - startIndex), line, startColumn));
                    continue;
                }

                if (character == '"')
                {
                    index++;
                    column++;
                    var startIndex = index;
                    while (index < source.Length && source[index] != '"' && source[index] != '\n')
                    {
                        index++;
                        column++;
                    }

                    if (index >= source.Length || source[index] != '"')
                    {
                        Diagnostics.Add(new VCodeDiagnostic(line, startColumn, "未結束的字串。"));
                        continue;
                    }

                    result.Add(new Token(TokenType.String, source.Substring(startIndex, index - startIndex), line, startColumn));
                    index++;
                    column++;
                    continue;
                }

                var symbolType = TokenType.End;
                var knownSymbol = true;
                switch (character)
                {
                    case '(': symbolType = TokenType.LeftParen; break;
                    case ')': symbolType = TokenType.RightParen; break;
                    case '{': symbolType = TokenType.LeftBrace; break;
                    case '}': symbolType = TokenType.RightBrace; break;
                    case ',': symbolType = TokenType.Comma; break;
                    case ';': symbolType = TokenType.Semicolon; break;
                    case '.': symbolType = TokenType.Dot; break;
                    case '+': symbolType = TokenType.Plus; break;
                    case '-': symbolType = TokenType.Minus; break;
                    case '*': symbolType = TokenType.Star; break;
                    case '/': symbolType = TokenType.Slash; break;
                    case '!': symbolType = index + 1 < source.Length && source[index + 1] == '=' ? TokenType.BangEqual : TokenType.Bang; break;
                    case '=': symbolType = index + 1 < source.Length && source[index + 1] == '=' ? TokenType.EqualEqual : TokenType.Equal; break;
                    case '>': symbolType = index + 1 < source.Length && source[index + 1] == '=' ? TokenType.GreaterEqual : TokenType.Greater; break;
                    case '<': symbolType = index + 1 < source.Length && source[index + 1] == '=' ? TokenType.LessEqual : TokenType.Less; break;
                    default: knownSymbol = false; break;
                }

                if (!knownSymbol)
                {
                    Diagnostics.Add(new VCodeDiagnostic(line, column, $"不支援的字元 '{character}'。"));
                }
                else
                {
                    result.Add(new Token(symbolType, character.ToString(), line, column));
                    if (symbolType == TokenType.BangEqual || symbolType == TokenType.EqualEqual || symbolType == TokenType.GreaterEqual || symbolType == TokenType.LessEqual)
                    {
                        index++;
                        column++;
                    }
                }

                index++;
                column++;
            }

            result.Add(new Token(TokenType.End, string.Empty, line, column));
            return result;
        }
    }
}
