using System;
using System.Collections;
using System.Collections.Generic;

namespace Flutter {
	public class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue> 
		where TValue : class
		{ 

		Dictionary<TKey, WeakReference<TValue>> dictionary = new Dictionary<TKey, WeakReference<TValue>> ();
		public TValue this [TKey key] {
			//
			get {
				TValue target = null;
				var success = dictionary.TryGetValue (key, out var val) && val.TryGetTarget (out target);
				return target;
			}
			set => Add (key, value);
		}

		public ICollection<TKey> Keys => dictionary.Keys;

		public ICollection<TValue> Values => throw new NotImplementedException ();

		public int Count => dictionary.Count;

		public bool IsReadOnly => false;

		public void Add (TKey key, TValue value)
			=> dictionary [key] = new WeakReference<TValue> (value);

		public void Add (KeyValuePair<TKey, TValue> item) => Add (item.Key, item.Value);

		public void Clear () => dictionary.Clear ();

		public bool Contains (KeyValuePair<TKey, TValue> item) => ContainsKey (item.Key);

		public bool ContainsKey (TKey key) => dictionary.ContainsKey (key);

		public void CopyTo (KeyValuePair<TKey, TValue> [] array, int arrayIndex)
		{
			throw new NotImplementedException ();
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public bool Remove (TKey key) => dictionary.Remove (key);

		public bool Remove (KeyValuePair<TKey, TValue> item) => Remove (item.Key);

		public bool TryGetValue (TKey key, out TValue value)
		{
			TValue target = null;
			var success = dictionary.TryGetValue (key, out var val) && val.TryGetTarget (out target);
			value =  target;
			return success;
		}

		IEnumerator IEnumerable.GetEnumerator () => dictionary.GetEnumerator ();
	}
}
