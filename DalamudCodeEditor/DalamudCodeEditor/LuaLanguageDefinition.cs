namespace DalamudCodeEditor;

public class LuaLanguageDefinition : LanguageDefinition
{
    public LuaLanguageDefinition()
    {
        CommentStart = "--[[";
        CommentEnd = "]]";
        SingleLineComment = "--";

        Keywords.AddRange([
            "and", "break", "do", "else", "elseif", "end", "false", "for",
            "function", "goto", "if", "in", "local", "nil", "not", "or",
            "repeat", "return", "then", "true", "until", "while",
        ]);

        var builtins = new[]
        {
            "_G", "_VERSION", "_ENV", "assert", "collectgarbage", "dofile",
            "error", "getmetatable", "ipairs", "load", "loadfile", "next",
            "pairs", "pcall", "print", "rawequal", "rawget", "rawlen", "rawset",
            "select", "setmetatable", "tonumber", "tostring", "type", "xpcall",
            "require", "module", "coroutine", "table", "string", "math", "utf8",
            "io", "os", "debug", "package", "self", "...",
        };

        foreach (var ident in builtins)
        {
            Identifiers[ident] = new Identifier { Declaration = "Built-in" };
        }

        TokenizeLine = TokenizeLuaLine;
    }

    private IEnumerable<Token> TokenizeLuaLine(string line)
    {
        var length = line.Length;
        var i = 0;

        while (i < length)
        {
            var c = line[i];

            // Skip whitespace
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            // Single-line comment: --
            if (c == '-' && i + 1 < length && line[i + 1] == '-')
            {
                // Check if it's a multiline comment start: --[[
                if (i + 3 < length && line[i + 2] == '[' && line[i + 3] == '[')
                {
                    // Multiline comment start
                    var start = i;
                    i += 4;
                    // find end of multiline comment ']]'
                    var endComment = line.IndexOf("]]", i);
                    if (endComment == -1)
                    {
                        endComment = length;
                    }
                    else
                    {
                        endComment += 2;
                    }

                    yield return new Token(start, endComment, PaletteIndex.Comment);
                    i = endComment;
                }
                else
                {
                    // Single line comment to end of line
                    yield return new Token(i, length, PaletteIndex.Comment);
                    yield break; // rest is comment
                }

                continue;
            }

            // Strings: "..." or '...'
            if (c == '"' || c == '\'')
            {
                var start = i;
                var quote = c;
                i++;
                while (i < length)
                {
                    if (line[i] == '\\' && i + 1 < length)
                    {
                        // Skip escaped char
                        i += 2;
                        continue;
                    }

                    if (line[i] == quote)
                    {
                        i++;
                        break;
                    }

                    i++;
                }

                yield return new Token(start, i, PaletteIndex.String);
                continue;
            }

            // Numbers (simple): digit or digit with decimals
            if (char.IsDigit(c))
            {
                var start = i;
                i++;
                var dotFound = false;
                while (i < length && (char.IsDigit(line[i]) || !dotFound && line[i] == '.'))
                {
                    if (line[i] == '.')
                    {
                        dotFound = true;
                    }

                    i++;
                }

                yield return new Token(start, i, PaletteIndex.Number);
                continue;
            }

            // Identifiers / Keywords / Built-ins: letter or '_'
            if (char.IsLetter(c) || c == '_')
            {
                var start = i;
                i++;
                while (i < length && (char.IsLetterOrDigit(line[i]) || line[i] == '_'))
                {
                    i++;
                }

                var word = line.Substring(start, i - start);
                var color = PaletteIndex.Default;

                if (Keywords.Contains(word))
                {
                    color = PaletteIndex.Keyword;
                }
                else if (Identifiers.ContainsKey(word))
                {
                    color = PaletteIndex.Function; // or Variable?
                }

                yield return new Token(start, i, color);
                continue;
            }

            // Operators and punctuation (basic)
            // For simplicity, treat any non-alphanumeric/non-whitespace char as operator/punctuation
            {
                var start = i;
                i++;

                // You can extend this to multi-char operators, e.g. '==', '~=', '>=', etc.
                // Check multi-char operators:
                if (start + 1 < length)
                {
                    var twoChars = line.Substring(start, 2);
                    if (twoChars == "==" || twoChars == "~=" || twoChars == ">=" || twoChars == "<=")
                    {
                        i = start + 2;
                        yield return new Token(start, i, PaletteIndex.Operator);
                        continue;
                    }
                }

                // Single-char operator or punctuation
                var ch = line[start];
                if ("+-*/%^#=<>;:,(){}[]".Contains(ch))
                {
                    yield return new Token(start, i, PaletteIndex.Operator);
                }
                else
                {
                    yield return new Token(start, i, PaletteIndex.OtherLiteral);
                }
            }
        }
    }
}
