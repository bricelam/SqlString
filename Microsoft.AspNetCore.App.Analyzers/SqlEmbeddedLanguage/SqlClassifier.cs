using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.ExternalAccess.AspNetCore.EmbeddedLanguages;

namespace Microsoft.AspNetCore.Analyzers.SqlEmbeddedLanguage;

[ExportAspNetCoreEmbeddedLanguageClassifier("Sql", LanguageNames.CSharp)]
internal class SqlClassifier : IAspNetCoreEmbeddedLanguageClassifier
{
    public void RegisterClassifications(AspNetCoreEmbeddedLanguageClassificationContext context)
    {
        // TODO
        context.AddClassification(ClassificationTypeNames.RegexAnchor, context.SyntaxToken.Span);
    }
}
