# Indent Writer

## Introduction

`Indent Writer` is a lightweight and concise implementation of an indented string builder using string interpolation. The main advantage of this approach is that it avoids the need for a lot of boilerplate code, such as `AddIndent()`, `RemoveIndent()`, `Append()`, and `using var block = typicalIndentBuilderImplementation.CreateBlock(); { ... }`.

## Features

- **Minimal Boilerplate**: No need to manually manage indentation levels. The indentation is automatically handled, reducing the amount of boilerplate code.
- **String Interpolation**: Uses string interpolation to handle indentation, making the code more readable and concise.
- **Efficient**: The input string interpolation is streamed directly to the inner `StringBuilder` of the `IndentStackWriter` instance, avoiding concatenation overhead.
- **Less Cognitive Load**: You don't need to manually specify the indentation. The indentation is part of your output, making it easier to visualize the final structure of the string.

## Example Usage

### Code

Here's an example of how to use `IndentWriter` to generate a C# extension method for printing a class instance to the console in a JSON-like format, as demonstrated in the `Program.cs` file of this repository:

```csharp
using IndentWriter;
using System.Text;
using System.Collections.Generic;

var target = new StringBuilder();

var writer = new IndentStackWriter(target);

var someNamespace = new SomeNamespace() { DisplayString = "Example" };
var someType = new SomeTypeSnapshot()
{
    ContainingNamespace = true ? someNamespace : null,
    Name = "ExampleClass",
    Properties = new List<SomePropertySnapshot>
    {
        new SomePropertySnapshot() { Name = "Integer1", TypeDisplayString = "int" },
        new SomePropertySnapshot() { Name = "String1", TypeDisplayString = "string" },
        new SomePropertySnapshot() { Name = "DateTime", TypeDisplayString = nameof(DateTime) },
        new SomePropertySnapshot() { Name = "Smth", TypeDisplayString = "object" },
    }
};

GenerateConsolePrinterExtension(writer, someType);

var code = target.ToString();

Console.WriteLine(code);

Console.Read();

IndentedInterpolatedStringHandler GenerateConsolePrinterExtension(IndentStackWriter _, SomeTypeSnapshot type)
{
    if (type.ContainingNamespace != null)
    {
        return writer[$$"""
        using System;

        namespace {{type.ContainingNamespace.DisplayString}}
        {
            {{_.Scope[AppendExtensionClass(_, type)]}}
        }
        """];
    }

    return writer[$$"""
        using System;

        {{_.Scope[AppendExtensionClass(_, type)]}}
        """];
}

IndentedInterpolatedStringHandler AppendExtensionClass(IndentStackWriter _, SomeTypeSnapshot type)
{
    return _[$$"""
    internal static partial class {{type.Name}}Extensions
    {
        {{_[AppendConsolePrinter(_, type)]}}
    }
    """];
}

IndentedInterpolatedStringHandler AppendConsolePrinter(IndentStackWriter _, SomeTypeSnapshot type)
{
    var properties = type.Properties.Take(type.Properties.Count - 1);
    var lastProperty = type.Properties.Last();

    return _.Scope[$$"""
    public static void PrintToConsole(this {{type.DisplayString}} target)
    {
        Console.WriteLine("class {{type.DisplayString}}");
        Console.WriteLine("{");

        {{_.Scope.ForEach(
            properties,
            (_, prop) => _[$$"""Console.WriteLine("    \"{{prop.Name}}\" : \"{0}\",", target.{{prop.Name}});"""],
            joinBy: "\n")
        }}
        {{_[$$"""Console.WriteLine("    \"{{lastProperty.Name}}\" : \"{0}\"", target.{{lastProperty.Name}});"""]}}

        Console.WriteLine("}");
    }
    """];
}

file sealed class SomeTypeSnapshot
{
    public List<SomePropertySnapshot> Properties = new List<SomePropertySnapshot>();

    public SomeNamespace? ContainingNamespace;

    public string Name { get; set; }

    public string DisplayString => ContainingNamespace != null
        ? ContainingNamespace.DisplayString + "." + Name
        : Name;
}

file sealed class SomePropertySnapshot
{
    public string Name { get; set; }

    public string TypeDisplayString { get; set; } = "int";
}

file sealed class SomeNamespace
{
    public string DisplayString { get; set; } = nameof(System);
}
```

### Output

If the model has a namespace, the generated output will include the namespace. For example, if the model has a namespace `Example`, the output will be:

```csharp
using System;

namespace Example
{
    internal static partial class ExampleClassExtensions
    {
        public static void PrintToConsole(this Example.ExampleClass target)
        {
            Console.WriteLine("class Example.ExampleClass");
            Console.WriteLine("{");

            Console.WriteLine("    \"Integer1\" : \"{0}\",", target.Integer1);
            Console.WriteLine("    \"String1\" : \"{0}\",", target.String1);
            Console.WriteLine("    \"DateTime\" : \"{0}\",", target.DateTime);
            Console.WriteLine("    \"Smth\" : \"{0}\"", target.Smth);

            Console.WriteLine("}");
        }
    }
}
```

If the model does not have a namespace, the output will be:

```csharp
using System;

internal static partial class ExampleClassExtensions
{
    public static void PrintToConsole(this Example.ExampleClass target)
    {
        Console.WriteLine("class Example.ExampleClass");
        Console.WriteLine("{");

        Console.WriteLine("    \"Integer1\" : \"{0}\",", target.Integer1);
        Console.WriteLine("    \"String1\" : \"{0}\",", target.String1);
        Console.WriteLine("    \"DateTime\" : \"{0}\",", target.DateTime);
        Console.WriteLine("    \"Smth\" : \"{0}\"", target.Smth);

        Console.WriteLine("}");
    }
}
```
