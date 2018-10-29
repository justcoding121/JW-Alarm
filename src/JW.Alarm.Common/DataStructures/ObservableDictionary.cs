using Advanced.Algorithms.DataStructures.Foundation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace JW.Alarm.Common.DataStructures
{
    public class ObservableDictionary<TKey, TValue> : INotifyCollectionChanged,
                                                      IList<KeyValuePair<TKey, TValue>>,
                                                      IList where TKey : IComparable
    {
        private readonly OrderedDictionary<TKey, TValue> dictionary;

        public ObservableDictionary()
        {
            dictionary = new OrderedDictionary<TKey, TValue>();
        }

        public TValue GetValue(TKey key)
        {
            return dictionary[key];
        }

        public void SetValue(TKey key, TValue value)
        {
            dictionary[key] = value;
        }

        public KeyValuePair<TKey, TValue> this[int i]
        {
            get => dictionary.ElementAt(i);
            set => throw new NotSupportedException();
        }

        object IList.this[int i]
        {
            get => this[i];
            set => this[i] = (KeyValuePair<TKey, TValue>)value;
        }

        public int Count => dictionary.Count;

        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public bool IsSynchronized => false;

        public object SyncRoot => throw new NotImplementedException();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(TKey key, TValue value)
        {
            add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            add(item);
        }

        public int Add(object value)
        {
            return add((KeyValuePair<TKey, TValue>)value);
        }

        private int add(KeyValuePair<TKey, TValue> item)
        {
            var index = dictionary.Add(item.Key, item.Value);
            onNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            return index;
        }

        public void Clear()
        {
            dictionary.Clear();
            onNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.ContainsKey(item.Key);
        }

        public bool Contains(object value)
        {
            return Contains((KeyValuePair<TKey, TValue>)value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        public int IndexOf(object value)
        {
            return IndexOf((KeyValuePair<TKey, TValue>)value);
        }

        public int IndexOf(KeyValuePair<TKey, TValue> item)
        {
            return dictionary.IndexOf(item.Key);
        }

        public void Insert(int index, KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            Remove((KeyValuePair<TKey, TValue>)value);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var index = dictionary.Remove(item.Key);
            if (index >= 0)
            {
                onNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
                return true;
            }

            return index >= 0;
        }

        public void RemoveAt(int index)
        {
            dictionary.RemoveAt(index);
        }

        private void onNotifyCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
