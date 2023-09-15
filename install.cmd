@ECHO OFF

COPY "%~dp0Microsoft.CodeAnalysis.ExternalAccess.AspNetCore\bin\Debug\netstandard2.0\Microsoft.CodeAnalysis.ExternalAccess.AspNetCore.dll" "C:\Program Files\Microsoft Visual Studio\2022\Preview\Common7\IDE\CommonExtensions\Microsoft\VBCSharp\LanguageServices\Microsoft.CodeAnalysis.ExternalAccess.AspNetCore.dll"
COPY "%~dp0Microsoft.CodeAnalysis.ExternalAccess.AspNetCore\bin\Debug\netstandard2.0\Microsoft.CodeAnalysis.ExternalAccess.AspNetCore.dll" "C:\Program Files\Microsoft Visual Studio\2022\Preview\Common7\IDE\CommonExtensions\Microsoft\VBCSharp\LanguageServices\Core\Microsoft.CodeAnalysis.ExternalAccess.AspNetCore.dll"
COPY "%~dp0Microsoft.AspNetCore.App.Analyzers\bin\Debug\net8.0\Microsoft.AspNetCore.App.Analyzers.dll" "C:\Program Files\dotnet\packs\Microsoft.AspNetCore.App.Ref\8.0.0-rc.1.23421.29\analyzers\dotnet\cs\Microsoft.AspNetCore.App.Analyzers.dll"

PAUSE
