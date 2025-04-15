using IndentWriter;
using System.Text;

var target = new StringBuilder();

var writer = new IndentStackWriter(target);

var someNamespace = new SomeNamespace() { DisplayString = "Example" };
var someType = new SomeTypeSnapshot()
{
    ContainingNamespace = true ? someNamespace : null,
    Name = "ExampleClass",
    Properties = [
        new SomePropertySnapshot () { Name = "Integer1", TypeDisplayString = "int" },
        new SomePropertySnapshot () { Name = "String1", TypeDisplayString = "string" },
        new SomePropertySnapshot () { Name = "DateTime", TypeDisplayString = nameof(DateTime) },
        new SomePropertySnapshot () { Name = "Smth", TypeDisplayString = "object" },
    ]
};

GenerateConsolePrinterExtension(writer, someType);

var code = target.ToString();

Console.WriteLine(code);

Console.Read();

IndentedInterpolatedStringHandler GenerateConsolePrinterExtension(IndentStackWriter _, SomeTypeSnapshot type)
{
    if(type.ContainingNamespace != null)
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
        """
    ];
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
            (_, prop) =>_[$$"""Console.WriteLine("    \"{{prop.Name}}\" : \"{0}\",", target.{{prop.Name}});"""],
            joinBy: "\n")
        }}
        {{_[$$"""Console.WriteLine("    \"{{lastProperty.Name}}\" : \"{0}\"", target.{{lastProperty.Name}});"""]}}

        Console.WriteLine("}");
    }
    """];
}

file sealed class SomeTypeSnapshot
{
    public List<SomePropertySnapshot> Properties = [];

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