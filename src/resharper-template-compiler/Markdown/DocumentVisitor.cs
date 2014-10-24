using CommonMark.Syntax;

namespace CitizenMatt.ReSharper.TemplateCompiler.Markdown
{
    public class DocumentVisitor
    {
        public void Accept(Block block)
        {
            while(block != null)
            {
                switch (block.Tag)
                {
                    case BlockTag.Document:
                        Accept(block.FirstChild);
                        break;

                    case BlockTag.Paragraph:
                        VisitParagraph(block.InlineContent);
                        break;

                    case BlockTag.BlockQuote:
                        VisitBlockQuote(block);
                        break;

                    case BlockTag.List:
                        VisitList(block.ListData, block.FirstChild);
                        break;

                    case BlockTag.ListItem:
                        VisitListItem(block.FirstChild);
                        break;

                    case BlockTag.AtxHeader:
                    case BlockTag.SETextHeader:
                        VisitHeader(block.HeaderLevel, block.InlineContent);
                        break;

                    case BlockTag.IndentedCode:
                        VisitCode(block.StringContent);
                        break;

                    case BlockTag.FencedCode:
                        VisitCode(block.FencedCodeData, block.StringContent);
                        break;

                    case BlockTag.HtmlBlock:
                        VisitHtml(block.StringContent);
                        break;
                    case BlockTag.HorizontalRuler:
                        VisitHorizontalRuler();
                        break;
                }

                block = block.NextSibling;
            }
        }

        protected virtual void VisitHeader(int headerLevel, Inline inlineContent)
        {
        }

        protected virtual void VisitParagraph(Inline inlineContent)
        {
        }

        protected virtual void VisitBlockQuote(Block firstChild)
        {
            Accept(firstChild);
        }

        protected virtual void VisitList(ListData listData, Block firstChild)
        {
            Accept(firstChild);
        }

        protected virtual void VisitListItem(Block firstChild)
        {
            Accept(firstChild);
        }

        protected virtual void VisitHorizontalRuler()
        {
        }

        protected virtual void VisitHtml(StringContent stringContent)
        {
        }

        protected virtual void VisitCode(StringContent stringContent)
        {
        }

        protected virtual void VisitCode(FencedCodeData fencedCodeData, StringContent stringContent)
        {
        }
    }
}