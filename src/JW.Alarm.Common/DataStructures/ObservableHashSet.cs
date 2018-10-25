using Advanced.Algorithms.DataStructures.Foundation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace JW.Alarm.Common.DataStructures
{
    public class ObservableHashSet<T> : INotifyCollectionChanged,
                                        IList<T>,
                                        IList where T : IComparable
    {
        private readonly SortedHashSet<T> sortedHashSet = new SortedHashSet<T>();

        public T this[int i]
        {
            get => sortedHashSet[i];
            set => throw new NotSupportedException();
        }

        object IList.this[int i]
        {
            get => this[i];
            set => this[i] = (T)value;
        }

        public int Count => sortedHashSet.Count;

        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public bool IsSynchronized => false;

        public object SyncRoot => throw new NotImplementedException();

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
            sortedHashSet.Add(item);
            onNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, sortedHashSet.IndexOf(item)));
            return sortedHashSet.IndexOf(item);
        }

        public void Clear()
        {
            sortedHashSet.Clear();
            onNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(object value)
        {
            return Contains((T)value);
        }

        public bool Contains(T item)
        {
            return sortedHashSet.Contains(item);
        }

        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        public int IndexOf(T item)
        {
            return sortedHashSet.IndexOf(item);
        }

        public void Insert(int i, object value)
        {
            Insert(i, (T)value);
        }

        public void Insert(int i, T item)
        {
            throw new NotSupportedException();
        }

        public void Remove(object value)
        {
            Remove((T)value);
        }

        public bool Remove(T item)
        {
            var index = sortedHashSet.IndexOf(item);
            var result = sortedHashSet.Remove(item);
            if (result)
            {
                onNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }

            return result;
        }

        public void RemoveAt(int i)
        {
            var element = sortedHashSet.RemoveAt(i);
            onNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, element));
        }

        private void onNotifyCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        public void CopyTo(Array array, int i)
        {
            CopyTo((T[])array, i);
        }

        public void CopyTo(T[] array, int i)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return sortedHashSet.GetEnumerator();
        }

    }
}
