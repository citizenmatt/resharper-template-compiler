using CommonMark.Syntax;

namespace CitizenMatt.ReSharper.TemplateCompiler.Markdown
{
    public partial class TemplateParser
    {
        private static string ExtractString(Inline inlineContent)
        {
            var inlineVisitor = new ExtractStringInlineVisitor();
            inlineVisitor.Accept(inlineContent);
            return inlineVisitor.Content;
        }

        private class ExtractFirstHeaderVisitor : DocumentVisitor
        {
            public string Header { get; private set; }

            protected override void VisitHeader(int headerLevel, Inline inlineContent)
            {
                if (Header == null && headerLevel == 1)
                    Header = ExtractString(inlineContent);
            }
        }

        private class ExtractFirstParagraphVisitor : DocumentVisitor
        {
            public string Paragraph { get; private set; }

            protected override void VisitParagraph(Inline inlineContent)
            {
                if (Paragraph == null)
                    Paragraph = ExtractString(inlineContent);
            }
        }

        private class ExtractFirstCodeBlockVisitor : DocumentVisitor
        {
            public string CodeBlock { get; private set; }

            protected override void VisitCode(FencedCodeData fencedCodeData, StringContent content)
            {
                if (CodeBlock == null)
                    CodeBlock = content.ToString().Trim();
            }
        }

        private class ExtractStringInlineVisitor : InlineContentVisitor
        {
            public ExtractStringInlineVisitor()
            {
                Content = string.Empty;
            }

            public string Content { get; private set; }

            protected override void VisitString(string literalContent)
            {
                Content = Content + literalContent;
            }
        }
    }
}