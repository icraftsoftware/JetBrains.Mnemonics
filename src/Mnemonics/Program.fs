open System
open System.Collections.Generic
open System.IO
open System.IO.Compression
open System.Linq
open System.Text
open System.Xml.Serialization
open Types
open DotNet
open CSharp
open VB
open Java
open Kotlin
open CPlusPlus

let version = "0.5"

type StringBuilder with

   member x.AppendString(s: string) = ignore <| x.Append s

   member x.AppendStrings(ss: string list) =
      for s in ss do
         ignore <| x.Append s

let rec pairs l =
   seq {
      for a in l do
         for b in l do
            yield (a, b)
   }

let newGuid () = Guid.NewGuid().ToString("N").ToUpper()

/// Renders an XML template for C#, VB.NET and F#
let renderReSharper () =
   let te = TemplatesExport(family = "Live Templates")
   let templates = List<TemplatesExportTemplate>()

   // debugging switches :)
   let renderCSharp, renderVBNET = true, false

   let printExpressions expressions (vars: List<TemplatesExportTemplateVariable>) defValue =
      let rec impl exps (builder: StringBuilder) =
         match exps with
         | Text(txt) :: t ->
            builder.AppendString txt
            impl t builder

         | DefaultValue :: t ->
            builder.AppendString defValue
            impl t builder

         | Variable(name, value) :: t ->
            let v = TemplatesExportTemplateVariable()
            v.name <- name
            v.initialRange <- 0
            v.expression <- value
            vars.Add(v)
            builder.AppendStrings [ "$"; name; "$" ]
            impl t builder

         | Constant(name, text) :: t ->
            if name <> "END" then
               begin
                  let v = TemplatesExportTemplateVariable()
                  v.name <- name
                  v.initialRange <- 0
                  v.expression <- "constant(\"" + text + "\")"

                  if not (vars.Any(fun v' -> v.name.Equals(v'.name))) then
                     vars.Add(v)
               end

            builder.AppendStrings [ "$"; name; "$" ]
            impl t builder

         | Scope(content) :: t ->
            builder.AppendString "{"
            impl content builder
            builder.AppendString "}"
            impl t builder

         | FixedType :: t ->
            builder.AppendString "$typename$" // replaced later
            impl t builder

         | [] -> ()

      let sb = StringBuilder()
      impl expressions sb
      sb.ToString()

   // first, process structures
   if renderCSharp then
      for (s, doc, exprs) in cSharpStructureTemplates do
         let t = TemplatesExportTemplate(shortcut = s)
         let vars = List<TemplatesExportTemplateVariable>()
         t.description <- printExpressions doc vars String.Empty
         t.reformat <- "True"
         t.uid <- newGuid ()
         t.text <- printExpressions exprs vars String.Empty
         t.Context <- TemplatesExportTemplateContext(CSharpContext = csharpStructureContext)
         t.Variables <- vars.ToArray()
         templates.Add t

   if renderVBNET then
      for (s, doc, exprs) in vbStructureTemplates do
         let t = TemplatesExportTemplate(shortcut = s)
         let vars = List<TemplatesExportTemplateVariable>()
         t.description <- printExpressions doc vars String.Empty
         t.reformat <- "False" // critical difference with C#!!!
         t.uid <- newGuid ()
         t.text <- printExpressions exprs vars String.Empty
         t.Context <- TemplatesExportTemplateContext(VBContext = vbContext)
         t.Variables <- vars.ToArray()
         templates.Add t

   // now process members
   if renderCSharp then
      for (s, doc, exprs) in cSharpMemberTemplates do
         // simple types; methods can be void
         let types =
            (if Char.ToLower(s.Chars(0)) = 'm' then
                ("", "void", "") :: csharpTypes
             else
                csharpTypes)

         for (tk, tv, defValue) in types do
            let t = TemplatesExportTemplate(shortcut = (s + tk))
            let vars = List<TemplatesExportTemplateVariable>()

            t.description <-
               (printExpressions doc vars defValue)
                  .Replace("$typename$", (if String.IsNullOrEmpty(tv) then "void" else tv))

            t.reformat <- "True"
            t.shortenQualifiedReferences <- "True"

            t.text <-
               (printExpressions exprs vars defValue)
                  .Replace("$typename$", (if String.IsNullOrEmpty(tv) then "void" else tv))

            t.uid <- newGuid ()
            t.Context <- TemplatesExportTemplateContext(CSharpContext = csharpMemberContext)
            t.Variables <- vars.ToArray()
            templates.Add t

         // generically specialized types
         for (gk, gv, genArgCount) in dotNetGenericTypes do
            match genArgCount with
            | 1 ->
               for (tk, tv, _) in csharpTypes do
                  let t0 = TemplatesExportTemplate(shortcut = s + gk + tk)
                  let vars0 = List<TemplatesExportTemplateVariable>()
                  let genericArgs = gv + "<" + tv + ">"
                  let defValue = "new " + genericArgs + "()"
                  t0.description <- (printExpressions doc vars0 defValue).Replace("$typename$", genericArgs)
                  t0.reformat <- "True"
                  t0.shortenQualifiedReferences <- "True"
                  t0.text <- (printExpressions exprs vars0 defValue).Replace("$typename$", genericArgs)
                  t0.uid <- newGuid ()
                  t0.Context <- TemplatesExportTemplateContext(CSharpContext = csharpMemberContext)
                  t0.Variables <- vars0.ToArray()
                  templates.Add t0
            | 2 -> // maybe this is not such a good idea because we get n^2 templates
               for ((tk0, tv0, _), (tk1, tv1, _)) in pairs csharpTypes do
                  let t = TemplatesExportTemplate(shortcut = s + gk + tk0 + tk1)
                  let vars = List<TemplatesExportTemplateVariable>()
                  let genericArgs = gv + "<" + tv0 + "," + tv1 + ">"
                  let defValue = "new " + genericArgs + "()"
                  t.description <- (printExpressions doc vars defValue).Replace("$typename$", genericArgs)
                  t.reformat <- "True"
                  t.shortenQualifiedReferences <- "True"
                  t.text <- (printExpressions exprs vars defValue).Replace("$typename$", genericArgs)
                  t.uid <- newGuid ()
                  t.Context <- TemplatesExportTemplateContext(CSharpContext = csharpMemberContext)
                  t.Variables <- vars.ToArray()
                  templates.Add t
            | _ -> raise <| Exception("We don't support this few/many args")

   if renderVBNET then
      for (s, doc, exprs) in vbMemberTemplates do
         // simple types; methods can be void
         for (tk, tv, defValue) in vbTypes do
            let t = TemplatesExportTemplate(shortcut = (s + tk))
            let vars = List<TemplatesExportTemplateVariable>()

            t.description <-
               (printExpressions doc vars defValue)
                  .Replace("$typename$", (if String.IsNullOrEmpty(tv) then "void" else tv))

            t.reformat <- "True"
            t.shortenQualifiedReferences <- "True"

            t.text <-
               (printExpressions exprs vars defValue)
                  .Replace("$typename$", (if String.IsNullOrEmpty(tv) then "void" else tv))

            t.uid <- newGuid ()
            t.Context <- TemplatesExportTemplateContext(VBContext = vbContext)
            t.Variables <- vars.ToArray()
            templates.Add t

         // generically specialized types
         for (gk, gv, genArgCount) in dotNetGenericTypes do
            match genArgCount with
            | 1 ->
               for (tk, tv, _) in vbTypes do
                  let t0 = TemplatesExportTemplate(shortcut = s + gk + tk)
                  let vars0 = List<TemplatesExportTemplateVariable>()
                  let genericArgs = gv + "(Of " + tv + ")"
                  let defValue = "new " + genericArgs + "()"
                  t0.description <- (printExpressions doc vars0 defValue).Replace("$typename$", genericArgs)
                  t0.reformat <- "True"
                  t0.shortenQualifiedReferences <- "True"
                  t0.text <- (printExpressions exprs vars0 defValue).Replace("$typename$", genericArgs)
                  t0.uid <- newGuid ()
                  t0.Context <- TemplatesExportTemplateContext(VBContext = vbContext)
                  t0.Variables <- vars0.ToArray()
                  templates.Add t0
            | 2 -> // maybe this is not such a good idea because we get n^2 templates
               for ((tk0, tv0, _), (tk1, tv1, _)) in pairs vbTypes do
                  let t = TemplatesExportTemplate(shortcut = s + gk + tk0 + tk1)
                  let vars = List<TemplatesExportTemplateVariable>()
                  let genericArgs = gv + "(Of " + tv0 + ", Of" + tv1 + ")"
                  let defValue = "new " + genericArgs + "()"
                  t.description <- (printExpressions doc vars defValue).Replace("$typename$", genericArgs)
                  t.reformat <- "True"
                  t.shortenQualifiedReferences <- "True"
                  t.text <- (printExpressions exprs vars defValue).Replace("$typename$", genericArgs)
                  t.uid <- newGuid ()
                  t.Context <- TemplatesExportTemplateContext(VBContext = vbContext)
                  t.Variables <- vars.ToArray()
                  templates.Add t
            | _ -> raise <| Exception("We don't support this few/many args")

   te.Template <- templates.ToArray()

   let filename =
      "C:\\Files\\Projects\\be.stateless\\JetBrains.Mnemonics\\downloads\\ReSharperMnemonics.xml"

   File.Delete(filename)
   let xs = XmlSerializer(te.GetType())
   use fs = new FileStream(filename, FileMode.Create, FileAccess.Write)
   xs.Serialize(fs, te)

   printfn $"%A{te.Template.Length} ReSharper templates exported"

/// Renders a JAR for Java, Kotlin, Scala and C++
let renderJava () =
   let javaDeclContext =
      [| templateSetTemplateOption (name = "JAVA_DECLARATION", value = true) |]

   let kotlinDeclContext =
      [| templateSetTemplateOption (name = "KOTLIN_EXPRESSION", value = true) |]

   // unverified
   let cppDeclContext =
      [| templateSetTemplateOption (name = "OC_DECLARATION_CPP", value = true) |]


   let printExpressions expressions (vars: List<templateSetTemplateVariable>) defValue =
      let rec impl exps (builder: StringBuilder) =
         match exps with
         | Text(txt) :: t ->
            builder.AppendString txt
            impl t builder

         | DefaultValue :: t ->
            builder.AppendString defValue
            impl t builder

         | Variable(name, value) :: t ->
            let v = templateSetTemplateVariable ()
            v.name <- name
            v.expression <- value
            v.alwaysStopAt <- true
            vars.Add(v)

            if not (vars.Any(fun v' -> v.name.Equals(v'.name))) then
               vars.Add(v)

            builder.AppendStrings [ "$"; name; "$" ]
            impl t builder

         | Constant(name, text) :: t ->
            if name <> "END" then
               begin
                  let v = templateSetTemplateVariable ()
                  v.name <- name
                  v.defaultValue <- "\"" + text + "\"" // note the quotes
                  v.expression <- String.Empty
                  v.alwaysStopAt <- true

                  if not (vars.Any(fun v' -> v.name.Equals(v'.name))) then
                     vars.Add(v)
               end

            builder.AppendStrings [ "$"; name; "$" ]
            impl t builder

         | Scope(content) :: t ->
            builder.AppendStrings [ ideaLineBreak; "{"; ideaLineBreak ]
            impl content builder
            builder.AppendStrings [ ideaLineBreak; "}" ]
            impl t builder

         | FixedType :: t ->
            builder.AppendString "$typename$" // replaced later
            impl t builder

         | [] -> ()

      let sb = StringBuilder()
      impl expressions sb
      sb.ToString()


   // this saves the template set under a filename
   let saveFile filename ts =
      let xs = XmlSerializer(ts.GetType())
      use sw = new StringWriter()
      xs.Serialize(sw, ts)
      let textToWrite = sw.ToString().Replace("&#xA;", entity10) // .NET knows better :)
      File.WriteAllText(filename, textToWrite)

   Directory.CreateDirectory(".\\jar") |> ignore
   Directory.CreateDirectory(".\\jar\\templates") |> ignore

   (***************** JAVA **********************************************)
   let ts = templateSet ()
   let templates = List<templateSetTemplate>()
   ts.group <- "mnemonics-java" // todo: investigate 'proprietary' groups
   let filename = ".\\jar\\templates\\" + ts.group + ".xml"

   // java structures
   for (s, exprs) in javaStructureTemplates do
      let t = templateSetTemplate (name = s)
      let vars = List<templateSetTemplateVariable>()
      t.description <- String.Empty
      t.toReformat <- true
      t.toShortenFQNames <- true
      t.context <- javaDeclContext
      t.value <- (printExpressions exprs vars String.Empty)
      t.variable <- vars.ToArray()
      templates.Add t

   // java members
   for (s, doc, exprs) in javaMemberTemplates do
      // simple types; methods can be void
      let types =
         if Char.ToLower(s.Chars(0)) = 'm' then
            ("", "void", "") :: javaPrimitiveTypes
         else
            javaPrimitiveTypes

      for (tk, tv, defValue) in types do
         let t = templateSetTemplate ()
         let vars = List<templateSetTemplateVariable>()
         t.name <- s + tk

         t.description <-
            (printExpressions doc vars defValue)
               .Replace("$typename$", (if String.IsNullOrEmpty(tv) then "void" else tv))

         t.toReformat <- true
         t.toShortenFQNames <- true
         t.context <- javaDeclContext

         t.value <-
            (printExpressions exprs vars defValue)
               .Replace("$typename$", (if String.IsNullOrEmpty(tv) then "void" else tv))

         t.variable <- vars.ToArray()
         templates.Add t

   ts.template <- templates.ToArray()
   saveFile filename ts

   (***************** KOTLIN ********************************************)
   let ts = templateSet ()
   let templates = List<templateSetTemplate>()
   ts.group <- "mnemonics-kotlin" // todo: investigate 'proprietary' groups
   let filename = ".\\jar\\templates\\" + ts.group + ".xml"

   // structures
   for (s, exprs) in kotlinStructureTemplates do
      let t = templateSetTemplate (name = s)
      let vars = List<templateSetTemplateVariable>()
      t.description <- String.Empty
      t.toReformat <- true
      t.toShortenFQNames <- true
      t.context <- kotlinDeclContext
      t.value <- (printExpressions exprs vars String.Empty)
      t.variable <- vars.ToArray()
      templates.Add t

   // members
   for (s, doc, exprs) in kotlinMemberTemplates do
      // simple types; methods can be void
      let types =
         if Char.ToLower(s.Chars(0)) = 'm' then
            ("", "Unit", "") :: kotlinPrimitiveTypes
         else
            kotlinPrimitiveTypes

      for (tk, tv, defValue) in types do
         let t = templateSetTemplate ()
         let vars = List<templateSetTemplateVariable>()
         t.name <- s + tk

         t.description <-
            (printExpressions doc vars defValue)
               .Replace("$typename$", (if String.IsNullOrEmpty(tv) then "Unit" else tv))

         t.toReformat <- true
         t.toShortenFQNames <- true
         t.context <- kotlinDeclContext

         t.value <-
            (printExpressions exprs vars defValue)
               .Replace("$typename$", (if String.IsNullOrEmpty(tv) then "Unit" else tv))

         t.variable <- vars.ToArray()
         templates.Add t

   ts.template <- templates.ToArray()
   saveFile filename ts

   (*************************** C++ (be afraid!) *****************************************)
   let ts = templateSet ()
   let templates = List<templateSetTemplate>()
   ts.group <- "mnemonics-cpp"
   let filename = ".\\jar\\templates\\" + ts.group + ".xml"

   // structures (note these end with semicolons)
   for (s, exprs) in cppStructureTemplates do
      let t = templateSetTemplate (name = s)
      let vars = List<templateSetTemplateVariable>()
      t.description <- String.Empty
      t.toReformat <- true
      t.toShortenFQNames <- true
      t.context <- cppDeclContext
      t.value <- (printExpressions exprs vars String.Empty)
      t.variable <- vars.ToArray()
      templates.Add t

   // members
   for (s, doc, exprs) in cppMemberTemplates do
      // simple types; methods can be void
      let types =
         if Char.ToLower(s.Chars(0)) = 'm' then
            ("", "void", "") :: cppTypes
         else
            cppTypes

      for (tk, tv, defValue) in types do
         let t = templateSetTemplate ()
         let vars = List<templateSetTemplateVariable>()
         t.name <- s + tk

         t.description <-
            (printExpressions doc vars defValue)
               .Replace("$typename$", (if String.IsNullOrEmpty(tv) then "void" else tv))

         t.toReformat <- true
         t.toShortenFQNames <- true
         t.context <- cppDeclContext

         t.value <-
            (printExpressions exprs vars defValue)
               .Replace("$typename$", (if String.IsNullOrEmpty(tv) then "void" else tv))

         t.variable <- vars.ToArray()
         templates.Add t

   ts.template <- templates.ToArray()
   saveFile filename ts

   // TODO: java and kotlin generics
   let ideaFileName = "IntelliJ IDEA Global Settings"
   File.WriteAllText(".\\jar\\" + ideaFileName, String.Empty)

   // now wrap it in a jar. use of 3rd-party zipper unavoidable
   let jarFileName = "IdeaMnemonics.jar"
   File.Delete jarFileName
   ZipFile.CreateFromDirectory(".\\jar", jarFileName)

   printfn $"%A{templates.Count} IDEA templates exported"

[<EntryPoint>]
let main _ =
   renderReSharper ()
   // renderJava ()
   //Console.ReadKey() |> ignore
   0
