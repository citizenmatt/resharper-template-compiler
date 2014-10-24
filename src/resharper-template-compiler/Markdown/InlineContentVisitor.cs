using CommonMark.Syntax;

namespace CitizenMatt.ReSharper.TemplateCompiler.Markdown
{
    public class InlineContentVisitor
    {
        public void Accept(Inline inline)
        {
            while (inline != null)
            {
                switch (inline.Tag)
                {
                    case InlineTag.String:
                        VisitString(inline.LiteralContent);
                        break;

                    case InlineTag.SoftBreak:
                        VisitSoftBreak();
                        break;

                    case InlineTag.LineBreak:
                        VisitLineBreak();
                        break;

                    case InlineTag.Code:
                        VisitCode(inline.LiteralContent);
                        break;

                    case InlineTag.RawHtml:
                        VisitRawHtml(inline.LiteralContent);
                        break;

                    case InlineTag.Emphasis:
                        VisitEmphasis(inline.FirstChild);
                        break;

                    case InlineTag.Strong:
                        VisitStrong(inline.FirstChild);
                        break;

                    case InlineTag.Link:
                        VisitLink(inline.Linkable);
                        break;

                    case InlineTag.Image:
                        VisitImage(inline.Linkable);
                        break;
                }

                inline = inline.NextSibling;
            }
        }

        protected virtual void VisitString(string literalContent)
        {
        }

        protected virtual void VisitSoftBreak()
        {
        }

        protected virtual void VisitLineBreak()
        {
        }

        protected virtual void VisitCode(string content)
        {
        }

        protected virtual void VisitRawHtml(string content)
        {
        }

        protected virtual void VisitEmphasis(Inline firstChild)
        {
            Accept(firstChild);
        }

        protected virtual void VisitStrong(Inline firstChild)
        {
            Accept(firstChild);
        }

        protected virtual void VisitLink(InlineContentLinkable linkData)
        {
        }

        protected virtual void VisitImage(InlineContentLinkable linkData)
        {
        }
    }
}