using CodeGenerator;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using System.Collections.Immutable;
using System.Reflection;
using Xunit;

namespace Avro.CodeGenerator.Tests
{
    public class GeneratorTests
    {
        [Fact]
        public void AvroGeneratorGetCodeNamespaceShouldGenerateCorrectNamespace()
        {
            var testBaseNamespace = "test";
            var testFolderPathTree = @"this\should\create\namespaces";
            var testFolderPath = @$"C:\test\{testFolderPathTree}";

            var expectedCodeNamespace = $"{testBaseNamespace}.{testFolderPathTree.Replace('\\', '.')}";

            var codeNamespace = AvroGenerator.GetCodeNamespace(testBaseNamespace, testFolderPath);

            using (var assertionScope = new AssertionScope())
            {
                codeNamespace.Should().Be(expectedCodeNamespace);
            }
        }

        [Fact]
        public void AvroGeneratorGetCodeNamespaceShouldReturnBaseNamespaceWhenBaseNamespaceDoesNotExistInPath()
        {
            var testBaseNamespace = "test";
            var testFolderPathTree = @"this\should\create\namespaces";
            var testFolderPath = @$"C:\{testFolderPathTree}";

            var codeNamespace = AvroGenerator.GetCodeNamespace(testBaseNamespace, testFolderPath);

            using (var assertionScope = new AssertionScope())
            {
                codeNamespace.Should().Be(testBaseNamespace);
            }
        }

        [Fact]
        public void AvroGeneratorGetCodeNamespaceShouldReturnBaseNamespaceWhenBaseNamespaceIsPath()
        {
            var testBaseNamespace = "test";
            var testFolderPathTree = @"test";
            var testFolderPath = @$"C:\{testFolderPathTree}";

            var codeNamespace = AvroGenerator.GetCodeNamespace(testBaseNamespace, testFolderPath);

            using (var assertionScope = new AssertionScope())
            {
                codeNamespace.Should().Be(testBaseNamespace);
            }
        }

        [Fact]
        public void AvroGeneratorAvroGenShouldReturnGeneratedClass()
        {
            var testAvroSchema = @"{""type"":""record"",""name"":""test-schema"",""namespace"":""test.avro"",""fields"":[{""name"":""username"",""doc"":""Name of the user account on Thecodebuzz.com"",""type"":""string""},{""name"":""email"",""doc"":""The email of the user logging message on the blog"",""type"":""string""},{""name"":""timestamp"",""doc"":""time in seconds"",""type"":""long""}],""doc:"":""A basic schema for storing thecodebuzz blogs messages""}";
            var testCodeNamespace = "test.code.@namespace";

            var avroClass = AvroGenerator.AvroGen(testAvroSchema, testCodeNamespace);

            using (var assertionScope = new AssertionScope())
            {
                avroClass.Should().Contain($"namespace {testCodeNamespace}{Environment.NewLine}");
                avroClass.Should().Contain(AvroGenerator.Header);
            }
        }

        [Fact]
        public void AvroGeneratorAvroGenShouldThrowExceptionWhenNamespaceNotSupplied()
        {
            var testAvroSchema = @"{""type"":""record"",""name"":""test-schema"",""fields"":[{""name"":""username"",""doc"":""Name of the user account on Thecodebuzz.com"",""type"":""string""},{""name"":""email"",""doc"":""The email of the user logging message on the blog"",""type"":""string""},{""name"":""timestamp"",""doc"":""time in seconds"",""type"":""long""}],""doc:"":""A basic schema for storing thecodebuzz blogs messages""}";
            var testCodeNamespace = "test.code.@namespace";

            using (var assertionScope = new AssertionScope())
            {
                Assert.Throws<Avro.CodeGenException>(() => AvroGenerator.AvroGen(testAvroSchema, testCodeNamespace));
            }
        }

        [Fact]
        public void AvroGeneratorInitializeDoesNothing()
        {
            var testGeneratorInitializationContext = new Microsoft.CodeAnalysis.GeneratorInitializationContext() { };

            new AvroGenerator().Initialize(testGeneratorInitializationContext);

            Assert.True(true);
        }

        [Fact]
        public void AvroGeneratorExecuteProcessesAvroFile()
        {
            var path = "testSchema.avro";
            var testSchemaNamespace = "compilation";
            var testSchemaClass = "TestSchema";

            Action<string, string> processFunction = (path, avroClass) =>
            {
                using (var assertionScope = new AssertionScope())
                {
                    path.Should().Be($"{path}.g.cs");
                    avroClass.Should().Contain($"class {testSchemaClass}");
                    avroClass.Should().Contain($"namespace {testSchemaNamespace}");
                }
            };

            var inputCompilation = CreateCompilation(GetTestCompilationSource());

            AvroGenerator.ProcessAvroGenResult = processFunction;

            var generator = new AvroGenerator();

            var driver = CSharpGeneratorDriver
                .Create(generator)
                .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(new SchemaAdditionalText(path)));

            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Mock.VerifyAll();
        }

        [Fact]
        public void AvroGeneratorExecuteProcessesAvroFileWithFolderNamespace()
        {
            var path = "TestNamespace/testSchema.avro";
            var testSchemaNamespace = "compilation.TestNamespace";
            var testSchemaClass = "TestSchema";

            Action<string, string> processFunction = (path, avroClass) =>
            {
                using (var assertionScope = new AssertionScope())
                {
                    path.Should().Be($"{path}.g.cs");
                    avroClass.Should().Contain($"class {testSchemaClass}");
                    avroClass.Should().Contain($"namespace {testSchemaNamespace}");
                }
            };

            var inputCompilation = CreateCompilation(GetTestCompilationSource(), testSchemaNamespace);

            AvroGenerator.ProcessAvroGenResult = processFunction;

            var generator = new AvroGenerator();

            var driver = CSharpGeneratorDriver
                .Create(generator)
                .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(new SchemaAdditionalText(path)));

            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Mock.VerifyAll();
        }

        [Fact]
        public void AvroGeneratorExecuteShouldNotProcessesAvroFileWhenGeneratedFileExists()
        {
            var path = "testSchema.exists.avro";

            Action<string, string> processFunction = (path, avroClass) => { throw new Exception($"test failed to recognized existing file {path}.g.cs"); };

            var inputCompilation = CreateCompilation(GetTestCompilationSource());

            AvroGenerator.ProcessAvroGenResult = processFunction;

            var generator = new AvroGenerator();

            var driver = CSharpGeneratorDriver
                .Create(generator)
                .AddAdditionalTexts(ImmutableArray.Create<AdditionalText>(new SchemaAdditionalText(path)));

            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Mock.VerifyAll();
        }

        private static Compilation CreateCompilation(string source, string assemblyName = "compilation")
            => CSharpCompilation.Create(assemblyName,
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        private static string GetTestCompilationSource()
        {
            return
@"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}";
        }
    }
}
