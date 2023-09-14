using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.ExternalAccess.AspNetCore.EmbeddedLanguages;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using AssemblyName = System.Reflection.AssemblyName;

namespace Microsoft.AspNetCore.Analyzers.SqlEmbeddedLanguage;

[ExportAspNetCoreEmbeddedLanguageClassifier("Sql", LanguageNames.CSharp)]
internal class SqlClassifier : IAspNetCoreEmbeddedLanguageClassifier
{
    public void RegisterClassifications(AspNetCoreEmbeddedLanguageClassificationContext context)
    {
        Debugger.Launch();

        AppDomain.CurrentDomain.AssemblyResolve += HandleAssemblyResolve;
        try
        {
            RegisterClassificationsCore(context);
        }
        finally
        {
            AppDomain.CurrentDomain.AssemblyResolve -= HandleAssemblyResolve;
        }

        static Assembly? HandleAssemblyResolve(object? sender, ResolveEventArgs args)
        {
            if (new AssemblyName(args.Name).Name == "Microsoft.SqlServer.TransactSql.ScriptDom")
                return Assembly.LoadFrom(@"C:\Users\brice\.nuget\packages\microsoft.sqlserver.transactsql.scriptdom\161.8905.0\lib\net6.0\Microsoft.SqlServer.TransactSql.ScriptDom.dll");

            return null;
        }
    }

    public void RegisterClassificationsCore(AspNetCoreEmbeddedLanguageClassificationContext context)
    {
        var parser = new TSql80Parser(initialQuotedIdentifiers: true);

        using var reader = new StringReader(context.SyntaxToken.ValueText);

        //var root = parser.Parse(reader, out var errors);
        //root.Accept(new ClassifyingVisitor(context));

        var tokens = parser.GetTokenStream(reader, out var errors);
        foreach (var token in tokens)
        {
            context.AddClassification(
                GetClassificationType(token),
                new(context.SyntaxToken.SpanStart + 1 + token.Offset, token.Text?.Length ?? 0));
        }
    }

    static string GetClassificationType(TSqlParserToken token)
        => token.IsKeyword()
            ? token.TokenType switch
            {
                // TODO: Try, Catch, Throw
                TSqlTokenType.Break or
                TSqlTokenType.Continue or
                TSqlTokenType.Else or
                TSqlTokenType.GoTo or
                TSqlTokenType.If or
                TSqlTokenType.Return or
                TSqlTokenType.While
                    => ClassificationTypeNames.ControlKeyword,

                _
                    => ClassificationTypeNames.Keyword
            }
            : token.TokenType switch
            {
                // Punctuations
                TSqlTokenType.AddEquals or // Op
                TSqlTokenType.Ampersand or
                TSqlTokenType.Bang or
                TSqlTokenType.BitwiseAndEquals or // Op
                TSqlTokenType.BitwiseOrEquals or // Op
                TSqlTokenType.BitwiseXorEquals or // Op
                TSqlTokenType.Circumflex or
                TSqlTokenType.Colon or
                TSqlTokenType.Comma or
                TSqlTokenType.Concat or // Op
                TSqlTokenType.ConcatEquals or // Op
                TSqlTokenType.Divide or // Op
                TSqlTokenType.DivideEquals or
                TSqlTokenType.Dot or
                TSqlTokenType.DoubleColon or
                TSqlTokenType.EqualsSign or // Op when assignment
                TSqlTokenType.GreaterThan or
                TSqlTokenType.LeftCurly or
                TSqlTokenType.LeftParenthesis or
                TSqlTokenType.LeftShift or
                TSqlTokenType.LessThan or
                TSqlTokenType.Minus or
                TSqlTokenType.ModEquals or
                TSqlTokenType.MultiplyEquals or
                TSqlTokenType.PercentSign or
                TSqlTokenType.Plus or
                TSqlTokenType.RightCurly or
                TSqlTokenType.RightOuterJoin or
                TSqlTokenType.RightParenthesis or
                TSqlTokenType.RightShift or
                TSqlTokenType.Semicolon or
                TSqlTokenType.Star or
                TSqlTokenType.SubtractEquals or
                TSqlTokenType.Tilde or
                TSqlTokenType.VerticalLine
                    // TODO: Operator vs Punctuation
                    => ClassificationTypeNames.Operator,

                // Complex tokens
                TSqlTokenType.AsciiStringLiteral or
                TSqlTokenType.AsciiStringOrQuotedIdentifier or
                TSqlTokenType.DollarPartition or
                TSqlTokenType.Go or
                TSqlTokenType.HexLiteral or
                TSqlTokenType.Identifier or
                TSqlTokenType.Integer or
                TSqlTokenType.Label or
                TSqlTokenType.Money or
                TSqlTokenType.Numeric or
                TSqlTokenType.OdbcInitiator or
                TSqlTokenType.ProcNameSemicolon or
                TSqlTokenType.PseudoColumn or
                TSqlTokenType.QuotedIdentifier or
                TSqlTokenType.Real or
                TSqlTokenType.SqlCommandIdentifier or
                TSqlTokenType.UnicodeStringLiteral or
                TSqlTokenType.Variable
                    // TODO: Classify better
                    => ClassificationTypeNames.StringLiteral,

                // Comments
                TSqlTokenType.MultilineComment or
                TSqlTokenType.SingleLineComment
                    => ClassificationTypeNames.Comment,

                TSqlTokenType.WhiteSpace or
                TSqlTokenType.EndOfFile
                    => ClassificationTypeNames.WhiteSpace,

                _
                    => throw new UnreachableException("Unexpected token type: " + token.TokenType),
            };
}
