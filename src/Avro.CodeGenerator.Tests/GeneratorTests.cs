using CodeGenerator;
using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            using(var assertionScope = new AssertionScope())
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
    }
}
