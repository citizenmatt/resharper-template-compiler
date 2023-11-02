using System.IO;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
  public static class StreamWriterExtensions
  {
    public static void SetNewLine(this StreamWriter streamWriter, NewLine newLine)
    {
      if (newLine == NewLine.OS)
        return;

      streamWriter.NewLine = newLine == NewLine.Unix ? "\n" : "\r\n";
    }
  }
}
