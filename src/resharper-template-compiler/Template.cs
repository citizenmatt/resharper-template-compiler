using System;
using System.Collections.Generic;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
    public enum TemplateType
    {
        Live,
        Surround,
        File
    }

    public class Template
    {
        public Guid Guid;
        public TemplateType Type;
        public string Shortcut;
        public string Description;
        public string Text;
        public bool Reformat;
        public bool ShortenQualifiedReferences;
        public IDictionary<string, string> CustomProperties = new Dictionary<string, string>();
        public IList<string> Categories = new List<string>();
        public IList<Scope> Scopes = new List<Scope>(); 
        public IList<Field> Fields = new List<Field>();
        public string InputFile;
    }

    public class Scope
    {
        public Guid Guid;
        public string Type;
        public IDictionary<string, string> Parameters = new Dictionary<string, string>();

        public Scope()
        {
            Guid = Guid.NewGuid();
        }
    }

    public class Field
    {
        public string Name;
        public bool Editable = true;
        public string Expression;
    }
}
