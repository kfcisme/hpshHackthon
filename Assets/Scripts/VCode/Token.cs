namespace GlitchCompiler.VCode
{
    public readonly struct Token
    {
        public readonly TokenType Type; public readonly string Lexeme; public readonly int Line; public readonly int Column;
        public Token(TokenType type, string lexeme, int line, int column) { Type = type; Lexeme = lexeme; Line = line; Column = column; }
        public override string ToString() => $"{Type} '{Lexeme}' ({Line},{Column})";
    }
}
