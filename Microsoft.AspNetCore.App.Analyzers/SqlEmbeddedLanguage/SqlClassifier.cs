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
                return Assembly.LoadFrom(
                    Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.nuget\packages\microsoft.sqlserver.transactsql.scriptdom\161.8905.0\lib\net6.0\Microsoft.SqlServer.TransactSql.ScriptDom.dll"));

            return null;
        }
    }

    public void RegisterClassificationsCore(AspNetCoreEmbeddedLanguageClassificationContext context)
    {
        var parser = new TSql80Parser(initialQuotedIdentifiers: true);

        using var reader = new StringReader(context.SyntaxToken.ValueText);

        var tokens = parser.GetTokenStream(reader, out var errors);
        foreach (var token in tokens)
        {
            var classification = GetClassificationType(token);
            if (classification is null) continue;

            context.AddClassification(
                classification,
                // TODO: Handle other kinds of string literals
                new(context.SyntaxToken.SpanStart + 2 + token.Offset, token.Text?.Length ?? 0));
        }
    }

    static string? GetClassificationType(TSqlParserToken token)
        => token.IsKeyword()
            ? token.TokenType switch
            {
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
                TSqlTokenType.AddEquals or
                TSqlTokenType.Ampersand or
                TSqlTokenType.Bang or
                TSqlTokenType.BitwiseAndEquals or
                TSqlTokenType.BitwiseOrEquals or
                TSqlTokenType.BitwiseXorEquals or
                TSqlTokenType.Circumflex or
                TSqlTokenType.Concat or
                TSqlTokenType.ConcatEquals or
                TSqlTokenType.Divide or
                TSqlTokenType.DivideEquals or
                TSqlTokenType.Dot or
                TSqlTokenType.DoubleColon or
                TSqlTokenType.EqualsSign or
                TSqlTokenType.GreaterThan or
                TSqlTokenType.LeftShift or
                TSqlTokenType.LessThan or
                TSqlTokenType.Minus or
                TSqlTokenType.ModEquals or
                TSqlTokenType.MultiplyEquals or
                TSqlTokenType.PercentSign or
                TSqlTokenType.Plus or
                TSqlTokenType.RightOuterJoin or
                TSqlTokenType.RightShift or
                TSqlTokenType.Star or
                TSqlTokenType.SubtractEquals or
                TSqlTokenType.Tilde or
                TSqlTokenType.VerticalLine
                    => ClassificationTypeNames.Operator,

                TSqlTokenType.Colon or
                TSqlTokenType.Comma or
                TSqlTokenType.LeftCurly or
                TSqlTokenType.LeftParenthesis or
                TSqlTokenType.RightCurly or
                TSqlTokenType.RightParenthesis or
                TSqlTokenType.Semicolon
                    => ClassificationTypeNames.Punctuation,

                TSqlTokenType.AsciiStringLiteral or
                TSqlTokenType.UnicodeStringLiteral
                    => ClassificationTypeNames.StringLiteral,

                TSqlTokenType.AsciiStringOrQuotedIdentifier or
                TSqlTokenType.QuotedIdentifier or
                TSqlTokenType.Identifier or
                TSqlTokenType.Variable
                    => ClassificationTypeNames.Identifier,

                TSqlTokenType.Go
                    => ClassificationTypeNames.PreprocessorKeyword,

                TSqlTokenType.HexLiteral or
                TSqlTokenType.Integer or
                TSqlTokenType.Money or
                TSqlTokenType.Numeric or
                TSqlTokenType.Real
                    => ClassificationTypeNames.NumericLiteral,

                TSqlTokenType.Label
                    => ClassificationTypeNames.LabelName,

                TSqlTokenType.MultilineComment or
                TSqlTokenType.SingleLineComment
                    => ClassificationTypeNames.Comment,

                TSqlTokenType.WhiteSpace or
                TSqlTokenType.EndOfFile
                    => null,

                // TODO: Classify these
                TSqlTokenType.DollarPartition or
                TSqlTokenType.OdbcInitiator or
                TSqlTokenType.ProcNameSemicolon or
                TSqlTokenType.PseudoColumn or
                TSqlTokenType.SqlCommandIdentifier
                    => null,

                _
                    => throw new UnreachableException("Unexpected token type: " + token.TokenType),
            };
}
