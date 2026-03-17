using Microsoft.CodeAnalysis;
using DotNetTool.Core.Models;
using ClassInfo = DotNetTool.Core.Models.ClassInfo;

namespace DotNetTool.Core.Analysis;

public static class ClassExtractor
{
    public static IReadOnlyList<ClassInfo> Extract(Compilation compilation)
    {
        var results = new List<ClassInfo>();
        var stack = new Stack<INamespaceOrTypeSymbol>();

        // Use compilation.Assembly.GlobalNamespace to enumerate only the types
        // defined in THIS project, not in any referenced assembly.
        stack.Push(compilation.Assembly.GlobalNamespace);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (current is INamedTypeSymbol typeSymbol)
            {
                if (typeSymbol.TypeKind == TypeKind.Class
                    && typeSymbol.DeclaredAccessibility == Accessibility.Public)
                {
                    var fqn = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    // Strip the "global::" prefix from FullyQualifiedFormat output
                    if (fqn.StartsWith("global::", StringComparison.Ordinal))
                        fqn = fqn["global::".Length..];

                    var isNested = typeSymbol.ContainingType is not null;
                    var containingTypeName = isNested ? typeSymbol.ContainingType!.Name : null;

                    results.Add(new ClassInfo(
                        Name: typeSymbol.Name,
                        FullyQualifiedName: fqn,
                        IsNested: isNested,
                        ContainingTypeName: containingTypeName));

                    // Also traverse nested types
                    foreach (var member in typeSymbol.GetTypeMembers())
                        stack.Push(member);
                }
                else if (typeSymbol.TypeKind == TypeKind.Class)
                {
                    // Non-public class — still recurse into nested types that could be public
                    foreach (var member in typeSymbol.GetTypeMembers())
                        stack.Push(member);
                }
            }
            else if (current is INamespaceSymbol nsSymbol)
            {
                foreach (var member in nsSymbol.GetMembers())
                    stack.Push(member);
            }
        }

        return results;
    }
}
