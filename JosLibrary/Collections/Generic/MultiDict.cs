using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosLibrary.Collections.Generic
{
    public class MultiDict<TKey, TValue>  // no (collection) base class
    {
        private Dictionary<TKey, List<TValue>> _data = new Dictionary<TKey, List<TValue>>();

        public Dictionary<TKey, List<TValue>>.ValueCollection Values { get { return _data.Values; } }
        public Dictionary<TKey, List<TValue>>.KeyCollection Keys { get { return _data.Keys; } }

        public void Add(TKey k, TValue v)
        {
            // can be a optimized a little with TryGetValue, this is for clarity
            if (_data.ContainsKey(k))
                _data[k].Add(v);
            else
                _data.Add(k, new List<TValue>() { v });
        }

        public bool Contains(TKey k, TValue v)
        {
            foreach (var item in _data)
            {
                if (item.Key.Equals(k) && item.Value.Equals(v))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ContainsKey(TKey k)
        {
            foreach (var item in _data)
            {
                if (item.Key.Equals(k))
                {
                    return true;
                }
            }
            return false;
        }

        public List<TValue> this[TKey k]
        {
            get { return _data[k]; }
            set { _data[k] = value; }
        }

        public bool TryGetValue(TKey k, out List<TValue> v)
        {
            return _data.TryGetValue(k, out v);
        }

        public void Clear()
        {
            foreach (var item in _data)
            {
                foreach (var valueItem in item.Value)
                {
                    if (valueItem is IDisposable)
                    {
                        ((IDisposable)valueItem).Dispose();
                    }
                }
            }
            _data.Clear();
        }
    }
}
