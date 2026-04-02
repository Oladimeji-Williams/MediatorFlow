using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace MediatorFlow.Generator.Generators;

[Generator]
public class MediatorFlowGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var compilation = context.Compilation;

        var handlerInterface = compilation.GetTypeByMetadataName("MediatorFlow.Core.Abstractions.IRequestHandler`2");
        if (handlerInterface == null) return;

        var handlers = compilation.SyntaxTrees
            .SelectMany(tree => compilation.GetSemanticModel(tree).SyntaxTree.GetRoot().DescendantNodes())
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
            .Select(cls => compilation.GetSemanticModel(cls.SyntaxTree).GetDeclaredSymbol(cls))
            .OfType<INamedTypeSymbol>()
            .Where(type => type.AllInterfaces.Any(i =>
                i.OriginalDefinition.Equals(handlerInterface, SymbolEqualityComparer.Default)))
            .ToList();

        var sb = new StringBuilder();

        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using MediatorFlow.Core.Contracts;");
        sb.AppendLine("using MediatorFlow.Core.Abstractions;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine();
        sb.AppendLine("namespace MediatorFlow.Generated;");
        sb.AppendLine();
        sb.AppendLine("public class GeneratedDispatcher : MediatorFlow.Core.Abstractions.IDispatcher");
        sb.AppendLine("{");
        sb.AppendLine("public async Task<object?> Dispatch(object request, IServiceProvider provider, CancellationToken cancellationToken)");
        sb.AppendLine("    {");

        foreach (var handler in handlers)
        {
            foreach (var iface in handler.AllInterfaces.Where(i =>
                i.OriginalDefinition.Equals(handlerInterface, SymbolEqualityComparer.Default)))
            {
                var requestType = iface.TypeArguments[0];
                var responseType = iface.TypeArguments[1];

                sb.AppendLine($@"
                if (request is {requestType.ToDisplayString()} req_{requestType.Name})
                {{
                    var handler = provider.GetRequiredService<IRequestHandler<{requestType.ToDisplayString()}, {responseType.ToDisplayString()}>>();

                    RequestHandlerDelegate<{responseType.ToDisplayString()}> next =
                        () => handler.Handle(req_{requestType.Name}, cancellationToken);

                    var behaviors = provider.GetServices<IPipelineBehavior<{requestType.ToDisplayString()}, {responseType.ToDisplayString()}>>();

                    var behaviorArray = behaviors as IPipelineBehavior<{requestType.ToDisplayString()}, {responseType.ToDisplayString()}>[] 
                                        ?? behaviors.ToArray();

                    for (int i = 0; i < behaviorArray.Length; i++)
                    {{
                        var behavior = behaviorArray[i];
                        var current = next;
                        next = () => behavior.Handle(req_{requestType.Name}, cancellationToken, current);
                    }}

                    return await next();
                }}
                ");
            }
        }

        sb.AppendLine(@"
        throw new InvalidOperationException($""No handler found for {request.GetType().Name}"");
    }
}");

        context.AddSource("MediatorFlow.Dispatcher.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}