using System.Collections.Generic;
using System.IO;
using CommonMark.Syntax;

namespace CitizenMatt.ReSharper.TemplateCompiler.Markdown
{
    public class MetadataVisitor : DocumentVisitor
    {
        private readonly IDictionary<string, string> dictionary;
        private bool first = true;
        private bool collect = true;

        public MetadataVisitor(IDictionary<string, string> dictionary)
        {
            this.dictionary = dictionary;
        }

        protected override void VisitHorizontalRuler()
        {
            if (!first)
                collect = false;
            first = false;
        }

        protected override void VisitParagraph(Inline inlineContent)
        {
            if (!collect)
                return;

            var inlineVisitor = new MetadataInlineVisitor(dictionary);
            inlineVisitor.Accept(inlineContent);
        }

        private class MetadataInlineVisitor : InlineContentVisitor
        {
            private readonly IDictionary<string, string> dictionary;

            public MetadataInlineVisitor(IDictionary<string, string> dictionary)
            {
                this.dictionary = dictionary;
            }

            protected override void VisitString(string literalContent)
            {
                if (!literalContent.Contains(":"))
                    throw new InvalidDataException($"Expected ':' in YAML front matter. Got '{literalContent}'");
                var x = literalContent.Split(':');
                var key = x[0].Trim();
                var value = x[1].Trim();

                dictionary.Add(key, value);
            }
        }
    }
}