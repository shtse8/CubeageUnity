using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public abstract class SerializableDictionaryBase
{
	public abstract class Storage { }

	protected class Dictionary<TKey, TValue> : System.Collections.Generic.Dictionary<TKey, TValue>
	{
		public Dictionary() { }
		public Dictionary(IDictionary<TKey, TValue> dict) : base(dict) { }
		public Dictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}

[Serializable]
public abstract class SerializableDictionaryBase<TKey, TValue, TValueStorage> : SerializableDictionaryBase, IDictionary<TKey, TValue>, IDictionary, ISerializationCallbackReceiver, IDeserializationCallback, ISerializable
{
	private Dictionary<TKey, TValue> _mDict;
	[SerializeField] private TKey[] mKeys;
	[SerializeField] private TValueStorage[] mValues;

	public SerializableDictionaryBase()
	{
		_mDict = new Dictionary<TKey, TValue>();
	}

	public SerializableDictionaryBase(IDictionary<TKey, TValue> dict)
	{
		_mDict = new Dictionary<TKey, TValue>(dict);
	}

	protected abstract void SetValue(TValueStorage[] storage, int i, TValue value);
	protected abstract TValue GetValue(TValueStorage[] storage, int i);

	public void CopyFrom(IDictionary<TKey, TValue> dict)
	{
		_mDict.Clear();
		foreach (var kvp in dict)
		{
			_mDict[kvp.Key] = kvp.Value;
		}
	}

	public void OnAfterDeserialize()
	{
		if (mKeys != null && mValues != null && mKeys.Length == mValues.Length)
		{
			_mDict.Clear();
			int n = mKeys.Length;
			for (int i = 0; i < n; ++i)
			{
				_mDict[mKeys[i]] = GetValue(mValues, i);
			}

			mKeys = null;
			mValues = null;
		}
	}

	public void OnBeforeSerialize()
	{
		int n = _mDict.Count;
		mKeys = new TKey[n];
		mValues = new TValueStorage[n];

		int i = 0;
		foreach (var kvp in _mDict)
		{
			mKeys[i] = kvp.Key;
			SetValue(mValues, i, kvp.Value);
			++i;
		}
	}

	#region IDictionary<TKey, TValue>

	public ICollection<TKey> Keys { get { return ((IDictionary<TKey, TValue>)_mDict).Keys; } }
	public ICollection<TValue> Values { get { return ((IDictionary<TKey, TValue>)_mDict).Values; } }
	public int Count { get { return ((IDictionary<TKey, TValue>)_mDict).Count; } }
	public bool IsReadOnly { get { return ((IDictionary<TKey, TValue>)_mDict).IsReadOnly; } }

	public TValue this[TKey key]
	{
		get { return ((IDictionary<TKey, TValue>)_mDict)[key]; }
		set { ((IDictionary<TKey, TValue>)_mDict)[key] = value; }
	}

	public void Add(TKey key, TValue value)
	{
		((IDictionary<TKey, TValue>)_mDict).Add(key, value);
	}

	public bool ContainsKey(TKey key)
	{
		return ((IDictionary<TKey, TValue>)_mDict).ContainsKey(key);
	}

	public bool Remove(TKey key)
	{
		return ((IDictionary<TKey, TValue>)_mDict).Remove(key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		return ((IDictionary<TKey, TValue>)_mDict).TryGetValue(key, out value);
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		((IDictionary<TKey, TValue>)_mDict).Add(item);
	}

	public void Clear()
	{
		((IDictionary<TKey, TValue>)_mDict).Clear();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		return ((IDictionary<TKey, TValue>)_mDict).Contains(item);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		((IDictionary<TKey, TValue>)_mDict).CopyTo(array, arrayIndex);
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		return ((IDictionary<TKey, TValue>)_mDict).Remove(item);
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return ((IDictionary<TKey, TValue>)_mDict).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IDictionary<TKey, TValue>)_mDict).GetEnumerator();
	}

	#endregion

	#region IDictionary

	public bool IsFixedSize { get { return ((IDictionary)_mDict).IsFixedSize; } }
	ICollection IDictionary.Keys { get { return ((IDictionary)_mDict).Keys; } }
	ICollection IDictionary.Values { get { return ((IDictionary)_mDict).Values; } }
	public bool IsSynchronized { get { return ((IDictionary)_mDict).IsSynchronized; } }
	public object SyncRoot { get { return ((IDictionary)_mDict).SyncRoot; } }

	public object this[object key]
	{
		get { return ((IDictionary)_mDict)[key]; }
		set { ((IDictionary)_mDict)[key] = value; }
	}

	public void Add(object key, object value)
	{
		((IDictionary)_mDict).Add(key, value);
	}

	public bool Contains(object key)
	{
		return ((IDictionary)_mDict).Contains(key);
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return ((IDictionary)_mDict).GetEnumerator();
	}

	public void Remove(object key)
	{
		((IDictionary)_mDict).Remove(key);
	}

	public void CopyTo(Array array, int index)
	{
		((IDictionary)_mDict).CopyTo(array, index);
	}

	#endregion

	#region IDeserializationCallback

	public void OnDeserialization(object sender)
	{
		((IDeserializationCallback)_mDict).OnDeserialization(sender);
	}

	#endregion

	#region ISerializable

	protected SerializableDictionaryBase(SerializationInfo info, StreamingContext context)
	{
		_mDict = new Dictionary<TKey, TValue>(info, context);
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		((ISerializable)_mDict).GetObjectData(info, context);
	}

	#endregion
}


[Serializable]
public static class SerializableDictionary
{
	public class Storage<T> : SerializableDictionaryBase.Storage
	{
		public T Data;
	}
}


[Serializable]
public class SerializableDictionary<TKey, TValue> : SerializableDictionaryBase<TKey, TValue, TValue>
{
	public SerializableDictionary() { }
	public SerializableDictionary(IDictionary<TKey, TValue> dict) : base(dict) { }
	protected SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

	protected override TValue GetValue(TValue[] storage, int i)
	{
		return storage[i];
	}

	protected override void SetValue(TValue[] storage, int i, TValue value)
	{
		storage[i] = value;
	}
}


[Serializable]
public class SerializableDictionary<TKey, TValue, TValueStorage> : SerializableDictionaryBase<TKey, TValue, TValueStorage> where TValueStorage : SerializableDictionary.Storage<TValue>, new()
{
	public SerializableDictionary() { }
	public SerializableDictionary(IDictionary<TKey, TValue> dict) : base(dict) { }
	protected SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

	protected override TValue GetValue(TValueStorage[] storage, int i)
	{
		return storage[i].Data;
	}

	protected override void SetValue(TValueStorage[] storage, int i, TValue value)
	{
		storage[i] = new TValueStorage();
		storage[i].Data = value;
	}
}