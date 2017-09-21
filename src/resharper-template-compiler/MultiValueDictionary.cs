using System.Collections.Generic;
using System.Linq;

namespace CitizenMatt.ReSharper.TemplateCompiler
{
    public class MultiValueDictionary<TKey,TValue>
    {
        private readonly Dictionary<TKey, HashSet<TValue>> store = new Dictionary<TKey, HashSet<TValue>>();
        
        public void Add(TKey key, TValue value)
        {
            if (!store.TryGetValue(key, out var values))
            {
                values = new HashSet<TValue>();
                store.Add(key, values);
            }
            values.Add(value);
        }

        public int Count => store.Keys.Count;
        public IEnumerable<TKey> Keys => store.Keys;

        public IEnumerable<TValue> this[TKey key] =>
            store.TryGetValue(key, out var values) ? values : Enumerable.Empty<TValue>();
    }
}