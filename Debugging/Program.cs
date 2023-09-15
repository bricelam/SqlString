using System.Diagnostics.CodeAnalysis;

Sql(@"
    SELECT COUNT(*)
    FROM Developers
    WHERE Name = 'Brice'
");

void Sql([StringSyntax("Sql")] string sql)
{
}
