using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace JW.Alarm.Common.DataStructures
{
    [System.Runtime.InteropServices.ComVisible(false)]
    [Serializable]
    public class ObservableDictionary<TKey, TValue> : INotifyCollectionChanged,
                                                      INotifyPropertyChanged,
                                                      ICollection<KeyValuePair<TKey, TValue>>,
                                                      IEnumerable<KeyValuePair<TKey, TValue>>,
                                                      IEnumerable,
                                                      IDictionary<TKey, TValue>,
                                                      IReadOnlyCollection<KeyValuePair<TKey, TValue>>,
                                                      IReadOnlyDictionary<TKey, TValue>,
                                                      ICollection, IDictionary,
                                                      IDeserializationCallback, ISerializable
    {
        private readonly ExposedDictionary<TKey, TValue> dictionary;

        public ObservableDictionary()
        {
            dictionary = new ExposedDictionary<TKey, TValue>();
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            dictionary = new ExposedDictionary<TKey, TValue>(dictionary);
        }
        public ObservableDictionary(IEqualityComparer<TKey> comparer)
        {
            dictionary = new ExposedDictionary<TKey, TValue>(comparer);
        }

        public ObservableDictionary(int capacity)
        {
            dictionary = new ExposedDictionary<TKey, TValue>(capacity);
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            dictionary = new ExposedDictionary<TKey, TValue>(dictionary, comparer);
        }

        public ObservableDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            dictionary = new ExposedDictionary<TKey, TValue>(capacity, comparer);
        }

        protected ObservableDictionary(SerializationInfo info, StreamingContext context)
        {
            dictionary = new ExposedDictionary<TKey, TValue>(info, context);
        }

        public TValue this[TKey key] { get => dictionary[key]; set => dictionary[key] = value; }
        public object this[object key] { get => dictionary[(TKey)key]; set => dictionary[(TKey)key] = (TValue)value; }

        public int Count => dictionary.Count;

        public bool IsReadOnly => (dictionary as ICollection<KeyValuePair<TKey, TValue>>).IsReadOnly;

        public ICollection<TKey> Keys => dictionary.Keys;

        public ICollection<TValue> Values => dictionary.Values;

        public bool IsSynchronized => (dictionary as ICollection).IsSynchronized;

        public object SyncRoot => (dictionary as ICollection).SyncRoot;

        public bool IsFixedSize => (dictionary as IDictionary).IsFixedSize;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => (dictionary as IReadOnlyDictionary<TKey, TValue>).Keys;

        ICollection IDictionary.Keys => (dictionary as IDictionary).Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => (dictionary as IReadOnlyDictionary<TKey, TValue>).Values;

        ICollection IDictionary.Values => (dictionary as IDictionary).Values;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            (dictionary as ICollection<KeyValuePair<TKey, TValue>>).Add(item);
            onNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            onPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        }

        public void Add(TKey key, TValue value)
        {
            dictionary.Add(key, value);
            onNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
            onPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        }

        public void Add(object key, object value)
        {
            (dictionary as IDictionary).Add(key, value);
            onNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>((TKey)key, (TValue)value)));
            onPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        }

        public void Clear()
        {
            dictionary.Clear();
            onNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            onPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return (dictionary as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);
        }

        public bool Contains(object key)
        {
            return (dictionary as IDictionary).Contains(key);
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            (dictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            (dictionary as IDictionary).CopyTo(array, index);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return (dictionary as ICollection<KeyValuePair<TKey, TValue>>).GetEnumerator();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            dictionary.GetObjectData(info, context);
        }

        public void OnDeserialization(object sender)
        {
            dictionary.OnDeserialization(sender);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var result = (dictionary as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
            if (result)
            {
                onNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                onPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            }

            return result;
        }

        public bool Remove(TKey key)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
            {
                dictionary.Remove(key);
                onNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value)));
                onPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                return true;
            }

            return false;
        }

        public void Remove(object key)
        {
            Remove((TKey)key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        private void onNotifyCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        private void onPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return (dictionary as IDictionary).GetEnumerator();
        }

        //expose protected constructors of Dictionary<TKey, TValue>
        private class ExposedDictionary<Key, Value> : Dictionary<Key, Value>
        {
            internal ExposedDictionary() : base() { }
            internal ExposedDictionary(IDictionary<Key, Value> dictionary) : base(dictionary) { }
            internal ExposedDictionary(IEqualityComparer<Key> comparer) : base(comparer) { }
            internal ExposedDictionary(int capacity) : base(capacity) { }
            internal ExposedDictionary(IDictionary<Key, Value> dictionary, IEqualityComparer<Key> comparer) : base(dictionary, comparer) { }
            internal ExposedDictionary(int capacity, IEqualityComparer<Key> comparer) : base(comparer) { }
            internal ExposedDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
    }
}
