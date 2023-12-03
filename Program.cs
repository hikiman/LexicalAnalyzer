using System;
using System.Collections.Generic;

namespace LexicalAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputText = "(*Comment*)program ошибка%checkCase;\n var\n grade: integer;\n begin\ngrade := 5;\ncase (grade) of\n5: writeln(5);\n end.";
             
            List<string> keywords = new List<string>
            {
                "PROGRAM",
                "BEGIN",
                "END",
                "VAR",
                "INTEGER",
                "REAL",
                "CHAR",
                "STRING",
                "BOOLEAN",
                "IF",
                "THEN",
                "ELSE",
                "CASE",
                "OF",
                "DEFAULT",
                "DIV",
                "MOD",
                "WRITELN",
                "READLN",
                "=:"
            };
            List<string> libraryFunctions = new List<string> { "SQRT", "LOG", "LN", "NEARBY" };
            List<string> identifiers = new List<string> ();
            List<string> constants = new List<string>();
            List<char> separators = new List<char>();
            inputText = inputText.ToUpper();

            List<Token>[] tokenLists = Tokenize(inputText, keywords, libraryFunctions, identifiers, constants, separators);
            List<Token> tokens = tokenLists[0];
            List<Token> errorTokens = tokenLists[1];

            Console.WriteLine("Tokens:");
            PrintTokens(tokens);

            Console.WriteLine("\nErrors:");
            PrintErrorTokens(errorTokens);
        }

        static List<Token>[] Tokenize(string inputText, List<string> keywords, List<string> libraryFunctions, List<string> identifiers, List<string> constants, List<char> separators)
        {
            List<Token> tokens = new List<Token>();
            List<Token> errorTokens = new List<Token>();
            int currentPosition = 0;
            int textLength = inputText.Length;
            bool isInsideComment = false;

            while (currentPosition < textLength)
            {
                char currentChar = inputText[currentPosition];
                string currentLexeme = currentChar.ToString();

                if (!isInsideComment)
                {
                    if (currentChar == '(' && currentPosition + 1 < textLength && inputText[currentPosition + 1] == '*')
                    {
                        isInsideComment = true;
                        currentPosition++;
                    }
                    else if (char.IsLetter(currentChar) && currentChar < 128)
                    {
                        while (currentPosition + 1 < textLength && IsIdentifierPart(inputText[currentPosition + 1]))
                        {
                            currentPosition++;
                            currentLexeme += inputText[currentPosition];
                        }

                        string upperLexeme = currentLexeme.ToUpper();

                        if (keywords.Contains(upperLexeme))
                        {
                            tokens.Add(new Token("Keyword", upperLexeme));
                        }
                        else if (libraryFunctions.Contains(upperLexeme))
                        {
                            tokens.Add(new Token("Function", upperLexeme));
                        }
                        else
                        {
                            tokens.Add(new Token("Identifier", currentLexeme));
                            identifiers.Add(currentLexeme);
                        }
                    }
                    else if (char.IsDigit(currentChar))
                    {
                        while (currentPosition + 1 < textLength && char.IsDigit(inputText[currentPosition + 1]))
                        {
                            currentPosition++;
                            currentLexeme += inputText[currentPosition];
                        }

                        tokens.Add(new Token("Constant", currentLexeme));
                        constants.Add(currentLexeme);
                    }
                    else if (IsSeparator(currentChar))
                    {
                        if (currentChar == ':' && currentPosition + 1 < textLength && inputText[currentPosition + 1] == '=')
                        {
                            currentLexeme += inputText[currentPosition + 1];
                            currentPosition++;
                            tokens.Add(new Token("Keyword", currentLexeme));
                        }
                        else
                        {
                            tokens.Add(new Token("Separator", currentLexeme));
                            separators.Add(currentChar);
                        }
                    }
                    else if (!char.IsWhiteSpace(currentChar))
                    {
                        errorTokens.Add(new Token("Error", currentLexeme));
                    }
                }
                else
                {
                    if (currentChar == '*' && currentPosition + 1 < textLength && inputText[currentPosition + 1] == ')')
                    {
                        isInsideComment = false;
                        currentPosition++;
                    }
                }

                currentPosition++;
            }

            tokens.RemoveAll(token => token.Type == "Comment");

            return new List<Token>[] { tokens, errorTokens };
        }

        static bool IsIdentifierPart(char character)
        {
            return char.IsLetterOrDigit(character) || character == '_';
        }

        static bool IsSeparator(char character)
        {
            string separators = "+-*/(){}=<>,.![];:\"' ";
            return separators.Contains(character);
        }

        static void PrintTokens(List<Token> tokens)
        {
            foreach (Token token in tokens)
            {
                Console.Write($"({token.Type}) ");
            }
        }
        static void PrintErrorTokens(List<Token> tokens)
        {
            foreach (Token token in tokens)
            {
                Console.WriteLine($"({token.Type}) \t {token.Value} ");
            }
        }
    }

    class Token
    {
        public string Type { get; }
        public string Value { get; }

        public Token(string type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}