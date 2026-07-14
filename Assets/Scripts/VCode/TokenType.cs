namespace GlitchCompiler.VCode
{
    public enum TokenType
    {
        End, Identifier, Number, String, True, False, Func, Let, Loop, If, Else,
        Move, Turn, Color, Width, Circle, Rect, Shield, System, Reset,
        LeftParen, RightParen, LeftBrace, RightBrace, Comma, Semicolon, Dot,
        Plus, Minus, Star, Slash, Bang, Equal, EqualEqual, BangEqual, Greater, GreaterEqual, Less, LessEqual
    }
}
