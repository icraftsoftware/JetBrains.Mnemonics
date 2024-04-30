module VB

open Types

let vbContext =
   new TemplatesExportTemplateContextVBContext(context = "TypeMember, TypeAndNamespace", minimumLanguageVersion = 2.0M)

let vbTypes =
   [ ("b", "Boolean", "False")
     ("c", "Char", "''")
     ("f", "Single", "0.0f")
     ("by", "Byte", "0")
     ("d", "Double", "0.0")
     ("i", "Integer", "0")
     ("m", "Decimal", "0M")
     ("s", "String", "\"\"")
     ("l", "Long", "0")
     ("u", "UInteger", "0")
     ("g", "System.Guid", "System.Guid.NewGuid()")
     ("t", "System.DateTime", "System.DateTime.UtcNow") ]

// note: vb structures are self-closing, so it makes sense to just print the header
let vbStructureTemplates =
   [ ("c", [ Text "A Class" ], [ Text "Public Class "; Constant("CLASSNAME", "SomeClass") ])
     ("a", [ Text "A MustInherit Class" ], [ Text "Public MustInherit Class "; Constant("CLASSNAME", "SomeClass") ])
     ("C", [ Text "A Module" ], [ Text "Public Module "; Constant("MODULENAME", "SomeModule") ])
     ("i", [ Text "An Interface" ], [ Text "Public Interface "; Constant("INTERFACENAME", "ISomeInterface") ])
     ("s", [ Text "A Structure" ], [ Text "Public Structure "; Constant("STRUCTNAME", "SomeStructure") ])
     ("e", [ Text "An Enum" ], [ Text "Public Enum "; Constant("ENUMNAME", "SomeEnum") ]) ]

let vbMemberTemplates =
   [ ("v", [ Text "A field of type "; FixedType ], [ Text "Private "; Constant("fieldname", "fieldname"); Text " As "; FixedType ])
     ("vr",
      [ Text "A readonly field of type "; FixedType ],
      [ Text "Private ReadOnly "
        Constant("fieldname", "fieldname")
        Text " As "
        FixedType ])
     ("V",
      [ Text "A shared field of type "; FixedType ],
      [ Text "Private Shared "
        Constant("fieldname", "fieldname")
        Text " As "
        FixedType ])
     ("n",
      [ Text "A field of type "
        FixedType
        Text " initialized to the default value." ],
      [ Text "Private "
        Constant("fieldname", "fieldname")
        Text " As "
        FixedType
        Text " = "
        DefaultValue ])
     ("o",
      [ Text "A readonly field of type "
        FixedType
        Text " initialized to the default value." ],
      [ Text "Private ReadOnly "
        Constant("fieldname", "fieldname")
        Text " As "
        FixedType
        Text " = "
        DefaultValue ])
     ("m", [ Text "A subroutine." ], [ Text "Public Sub "; Constant("methodname", "SomeMethod"); Text "()" ])
     ("M", [ Text "A shared subroutine." ], [ Text "Public Shared Sub "; Constant("methodname", "SomeMethod"); Text "()" ])
     (* methods in VB.NET branch into Functions and Subs this covers only functions*)
     ("m",
      [ Text "A method that returns a "; FixedType ],
      [ Text "Public Function "
        Constant("methodname", "SomeMethod")
        Text "() As "
        FixedType ])
     ("M",
      [ Text "A shared method that returns a "; FixedType ],
      [ Text "Public Shared Function "
        Constant("methodname", "SomeMethod")
        Text "() As "
        FixedType ])
     ("p",
      [ Text "An automatic property of type "; FixedType ],
      [ Text "Public Property "
        Constant("propname", "SomeProperty")
        Text " As "
        FixedType ]) ]
