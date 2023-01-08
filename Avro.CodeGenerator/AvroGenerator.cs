﻿using Avro;
using Microsoft.CodeAnalysis;
using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CodeGenerator
{
    [Generator]
    public class AvroGenerator : ISourceGenerator
    {
        private const string Header = "//This file was auto-generated by Avro.CodeGenerator";

        private const string EnableLoggingBuildProperty = "Avro_CodeGenerator_EnableLogging";

        private static readonly DiagnosticDescriptor ErrorNoApacheAvroReference = new DiagnosticDescriptor(id: "Avro.CodeGenerator001", title: "Couldn't not find Apache.Avro nuget reference", messageFormat: "Couldn't find reference to required package Apache.Avro in project", category: "Avro.CodeGenerator", DiagnosticSeverity.Error, isEnabledByDefault: true);

        public void Execute(GeneratorExecutionContext context)
        {
            // check that the users compilation references the expected library 
            if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals("Avro", StringComparison.OrdinalIgnoreCase)))
            {
                context.ReportDiagnostic(Diagnostic.Create(ErrorNoApacheAvroReference, Location.None));
            }

            // global logging from project file
            var emitLoggingGlobal = false;
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.{EnableLoggingBuildProperty}", out var emitLoggingSwitch))
            {
                emitLoggingGlobal = emitLoggingSwitch.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            var avroFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.avro", SearchOption.AllDirectories);

            var baseNamespace = context.Compilation.AssemblyName;

            foreach (var avroFile in avroFiles)
            {
                var folderPath = GetFolderPath(baseNamespace, avroFile);

                var @namespace = GetNamespace(baseNamespace, folderPath);

                var avroClass = AvroGen(avroFile, @namespace);

                var avroFileName = $"{Path.GetFileName(avroFile)}.g.cs";

                var generatedFileName = Path.Combine(folderPath, avroFileName);

                context.AddSource(avroFileName, avroClass);
                //File.WriteAllText(generatedFileName, avroClass);
            }
        }

        private static string GetNamespace(string baseNamespace, string folderPath)
        {
            if (folderPath.Length > 0)
            {
                return $"{baseNamespace}.{folderPath.Replace("\\", ".")}";
            }

            return baseNamespace;
        }

        internal string GetFolderPath(string baseNamespace, string avroFile)
        {
            var folder = Path.GetDirectoryName(avroFile);

            var startindex = folder.LastIndexOf(baseNamespace);

            var folderPathIndex = folder.IndexOf('\\', startindex);

            if(folderPathIndex == -1)
            {
                return string.Empty;
            }

            return folder.Substring(folderPathIndex + 1);
        }

        private static string AvroGen(string avroFile, string @namespaceOverride)
        {
            var schemaText = File.ReadAllText(avroFile);

            var json = JsonSerializer.Deserialize<AvroSchema>(schemaText);

            var codeGen = new CodeGen();
            codeGen.AddSchema(schemaText, new Dictionary<string, string> { { json.Namespace, @namespaceOverride } });

            codeGen.GenerateCode();

            var cSharpCodeProvider = new CSharpCodeProvider();

            var opts = new CodeGeneratorOptions();
            opts.BracingStyle = "C";
            opts.IndentString = "    ";
            opts.BlankLinesBetweenMembers = true;

            var namespaces = codeGen.CompileUnit.Namespaces;

            var @namespace = namespaces.Count > 0 ? @namespaces[0] : null;
            if (@namespace == null)
            {
                return null; ;
            }
            var newNamespace = new CodeNamespace(@namespaceOverride);

            newNamespace.Comments.Add(CodeGenUtil.Instance.FileComment);

            foreach (var namespaceImport in CodeGenUtil.Instance.NamespaceImports)
            {
                newNamespace.Imports.Add(namespaceImport);
            }

            var codeTypeDeclaration = @namespace.Types.Count > 0 ? @namespace.Types[0] : null;
            if (codeTypeDeclaration == null)
            {
                return null;
            }
            
            newNamespace.Types.Add(codeTypeDeclaration);

            using (var writer = new StringWriter())
            {
                cSharpCodeProvider.GenerateCodeFromNamespace(newNamespace, writer, opts);

                writer.Flush();

                return string.Concat(Header, Environment.NewLine, writer.ToString().Replace("global::", string.Empty));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
//#if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
//#endif 
        }
    }
}