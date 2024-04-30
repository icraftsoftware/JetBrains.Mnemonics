module CSharp

open Types

let csharpStructureContext =
   TemplatesExportTemplateContextCSharpContext(context = "TypeMember, TypeAndNamespace", minimumLanguageVersion = 2.0M)

let csharpMemberContext =
   TemplatesExportTemplateContextCSharpContext(context = "TypeMember", minimumLanguageVersion = 2.0M)

let csharpTypes =
   [ ("b", "bool", "default")
     ("c", "char", "default")
     ("f", "float", "default")
     ("by", "byte", "default")
     ("d", "double", "default")
     ("i", "int", "default")
     ("m", "decimal", "default")
     ("s", "string", "\"\"")
     ("l", "long", "default")
     ("u", "uint", "default")
     ("g", "System.Guid", "System.Guid.NewGuid()")
     ("t", "System.DateTime", "System.DateTime.UtcNow")
     ("sb", "System.Text.StringBuilder", "new System.Text.StringBuilder()") ]

let cSharpStructureTemplates =
   [ ("c",
      [ Text "A class" ],
      [ Text "public class "
        Constant("CLASSNAME", "MyClass")
        Scope [ endConstant ] ])
     ("a",
      [ Text "An abstract class" ],
      [ Text "public abstract class "
        Constant("CLASSNAME", "MyClass")
        Scope [ endConstant ] ])
     ("C",
      [ Text "A static class" ],
      [ Text "public static class "
        Constant("CLASSNAME", "MyClass")
        Scope [ endConstant ] ])
     ("i",
      [ Text "An interface" ],
      [ Text "public interface "
        Constant("INTERFACENAME", "IMyInterface")
        Scope [ endConstant ] ])
     ("s",
      [ Text "A struct" ],
      [ Text "public struct "
        Constant("STRUCTNAME", "MyStruct")
        Scope [ endConstant ] ])
     ("e", [ Text "An enum" ], [ Text "public enum "; Constant("ENUMNAME", "MyEnum"); Scope [ endConstant ] ]) ]

let cSharpMemberTemplates =
   [ ("f",
      [ Text "A field of type "; FixedType ],
      [ Text "private "
        FixedType
        Text " "
        Constant("fieldname", "fieldname")
        semiColon ])
     ("fn",
      [ Text "A field of type "
        FixedType
        Text " initialized to the default value." ],
      [ Text "private "
        FixedType
        Text " "
        Constant("fieldname", "fieldname")
        Text " = "
        DefaultValue
        semiColon ])
     ("fr",
      [ Text "A readonly field of type "; FixedType ],
      [ Text "private readonly "
        Constant("type", "type")
        Text " "
        Constant("fieldname", "fieldname")
        semiColon ])
     ("frn",
      [ Text "A readonly field of type "
        FixedType
        Text " initialized to the default value." ],
      [ Text "private readonly "
        FixedType
        Text " "
        Constant("fieldname", "fieldname")
        Text " = "
        DefaultValue
        semiColon ])
     ("F",
      [ Text "A static field of type "; FixedType ],
      [ Text "private static "
        FixedType
        Text " "
        Constant("fieldname", "fieldname")
        semiColon ])
     ("m",
      [ Text "A method that returns a "; FixedType ],
      [ Text "public"
        space
        FixedType
        space
        Constant("methodname", "MyMethod")
        Text "()"
        Scope [ endConstant ] ])
     ("M",
      [ Text "A static method that returns a "; FixedType ],
      [ Text "public static "
        FixedType
        space
        Constant("methodname", "MyMethod")
        Text "()"
        Scope [ endConstant ] ])
     ("p",
      [ Text "An automatic property of type "; FixedType ],
      [ Text "public "
        FixedType
        space
        Constant("propname", "MyProperty")
        Text "{ get; set; }"
        endConstant ])
     ("pr",
      [ Text "An automatic property of type "
        FixedType
        Text " with a private setter" ],
      [ Text "public "
        FixedType
        space
        Constant("propname", "MyProperty")
        Text "{ get; private set; }"
        endConstant ])
     ("pg",
      [ Text "An automatic property of type "
        FixedType
        Text " with an empty getter and no setter" ],
      [ Text "public "
        FixedType
        Text " "
        Constant("propname", "MyProperty")
        Scope [ Text "get "; Scope [ endConstant ] ] ]) ]
