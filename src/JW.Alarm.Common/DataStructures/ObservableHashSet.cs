using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace JW.Alarm.Common.DataStructures
{
    public class ObservableHashSet<T> : INotifyCollectionChanged,
                                        IReadOnlyList<T>,
                                        IList<T>,
                                        IList
    {
        private readonly HashSet<T> hashSet = new HashSet<T>();

        private int index;
        private readonly Dictionary<int, T> forwardLookUp = new Dictionary<int, T>();
        private readonly Dictionary<T, int> reverseLookUp = new Dictionary<T, int>();

        public T this[int index] { get => forwardLookUp[index]; set => throw new NotImplementedException(); }

        public int Count => hashSet.Count;

        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public bool IsSynchronized => false;

        public object SyncRoot => throw new NotImplementedException();

        object IList.this[int index]
        {
            get => this[index];
            set
            {
                hashSet.Remove((T)value);
                hashSet.Add((T)value);
                forwardLookUp[index] = (T)value;
                reverseLookUp[(T)value] = index;
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public int Add(object value)
        {
            return add((T)value);
        }

        public void Add(T item)
        {
            add(item);
        }

        private int add(T item)
        {
            hashSet.Add(item);
            forwardLookUp[index] = item;
            reverseLookUp[item] = index;
            Insert(index, item);
            index++;

            return index - 1;
        }

        public void Clear()
        {
            hashSet.Clear();
            forwardLookUp.Clear();
            reverseLookUp.Clear();
            index = 0;
        }

        public bool Contains(object value)
        {
            return Contains((T)value);
        }

        public bool Contains(T item)
        {
            return hashSet.Contains(item);
        }

        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        public int IndexOf(T item)
        {
            return reverseLookUp[item];
        }

        public void Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        public void Insert(int index, T item)
        {
            onNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public void Remove(object value)
        {
            Remove((T)value);
        }

        public bool Remove(T item)
        {
            return hashSet.Remove(item);
        }

        public void RemoveAt(int index)
        {
            Remove(forwardLookUp[index]);
        }

        private void onNotifyCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return hashSet.GetEnumerator();
        }

    }
}
