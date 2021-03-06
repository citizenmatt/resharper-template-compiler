using System.Collections.Generic;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
    public class Trie
    {
        public class Node
        {
            public object Value;
            public readonly IDictionary<string, Node> Edges = new Dictionary<string, Node>();
        }

        private readonly Node root = new Node();

        public void Add(IEnumerable<string> segments, object value)
        {
            var node = root;
            foreach (var segment in segments)
            {
                if (!node.Edges.TryGetValue(segment, out var next))
                {
                    next = new Node();
                    node.Edges.Add(segment, next);
                }
                node = next;
            }
            node.Value = value;
        }

        public IEnumerable<string> GetChildKeys(IEnumerable<string> segments)
        {
            var node = root;
            foreach (var segment in segments)
            {
                if (!node.Edges.TryGetValue(segment, out var next))
                    return new List<string>();
                node = next;
            }
            return node.Edges.Keys;
        }

        public object GetValue(IList<string> segments)
        {
            var node = GetNode(segments);
            if (node == null)
                throw new KeyNotFoundException($"The key <{"/" + string.Join("/,", segments)}> was not found");
            return node.Value;
        }

        public bool TryGetValue(IEnumerable<string> segments, out object value)
        {
            value = null;
            var node = GetNode(segments);
            if (node != null)
            {
                value = node.Value;
                return true;
            }
            return false;
        }

        private Node GetNode(IEnumerable<string> segments)
        {
            var path = string.Empty;
            var node = root;
            foreach (var segment in segments)
            {
                path = path + "/" + segment;
                if (!node.Edges.TryGetValue(segment, out var next))
                    return null;
                node = next;
            }
            return node;
        }
    }
}