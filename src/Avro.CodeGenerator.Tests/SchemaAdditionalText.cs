using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Avro.CodeGenerator.Tests
{
    internal class SchemaAdditionalText : AdditionalText
    {
        private readonly string _text;

        public override string Path { get; }

        public SchemaAdditionalText(string path)
        {
            Path = path;
            _text = System.IO.File.ReadAllText(path);
        }

        public override SourceText GetText(CancellationToken cancellationToken = new CancellationToken())
        {
            return SourceText.From(_text);
        }
    }
}