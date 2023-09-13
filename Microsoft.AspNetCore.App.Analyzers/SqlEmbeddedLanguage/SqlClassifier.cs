using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.ExternalAccess.AspNetCore.EmbeddedLanguages;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Microsoft.AspNetCore.Analyzers.SqlEmbeddedLanguage;

[ExportAspNetCoreEmbeddedLanguageClassifier("Sql", LanguageNames.CSharp)]
internal class SqlClassifier : IAspNetCoreEmbeddedLanguageClassifier
{
    public void RegisterClassifications(AspNetCoreEmbeddedLanguageClassificationContext context)
    {
        var parser = new TSql80Parser(initialQuotedIdentifiers: true);

        // TODO
        context.AddClassification(ClassificationTypeNames.RegexAnchor, context.SyntaxToken.Span);
    }
}
