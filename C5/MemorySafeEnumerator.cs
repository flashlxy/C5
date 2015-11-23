﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace C5
{
    [Serializable]
    internal abstract class MemorySafeEnumerator<T> : IEnumerator<T>, IEnumerable<T>, IDisposable
    {
        private static int MainThreadId;

        //-1 means an iterator is not in use. 
        protected int IteratorState;

        protected MemoryType MemoryType { get; private set; }
       
        protected static bool IsMainThread
        {
            get { return System.Threading.Thread.CurrentThread.ManagedThreadId == MainThreadId; }
        }

        protected MemorySafeEnumerator(MemoryType memoryType)
        {
             MemoryType = memoryType;
             MainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
             IteratorState = -1;
        }

        protected abstract MemorySafeEnumerator<T> Clone();

        public abstract bool MoveNext();

        public abstract void Reset();

        public T Current { get; protected set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public virtual void Dispose()
        {
            IteratorState = -1;
        }

        public IEnumerator<T> GetEnumerator()
        {
            MemorySafeEnumerator<T> enumerator;

            switch (MemoryType)
            {
                case MemoryType.Normal:
                    enumerator = Clone();
                    break;
                case MemoryType.Safe:
                    if (IsMainThread)
                    {
                        enumerator = IteratorState != -1 ? Clone() : this;

                        IteratorState = 0;
                    }
                    else
                    {
                        enumerator = Clone();
                    }
                    break;
                case MemoryType.Strict:
                    if (!IsMainThread)
                    {
                        throw new ConcurrentEnumerationException("Multithread access detected! In Strict memory mode is not possible to iterate the collection from different threads");
                    }

                    if (IteratorState != -1)
                    {
                        throw new MultipleEnumerationException("Multiple Enumeration detected! In Strict memory mode is not possible to iterate the collection multiple times");
                    }

                    enumerator = this;
                    IteratorState = 0;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
           

            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    //[Serializable]
    //public class WhereEnumerator<T> : EnumerableBase<T>, IEnumerator<T>, IEnumerable<T>
    //{
    //    private ArrayBase<T> _internalList;

    //    private T current;
    //    private int _index;
    //    private int _theStamp;
    //    private int _end;

    //    private int _state;

    //    private Func<T, bool> _predicate;

    //    static int mainThreadId;
    //    // If called in the non main thread, will return false;
    //    static bool IsMainThread
    //    {
    //        get { return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId; }
    //    }

    //    public WhereEnumerator()
    //    {
    //        mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
    //        _state = -2;
    //    }
    //    internal void UpdateReference(ArrayBase<T> list, int start, int end, int theStamp, Func<T, bool> predicate)
    //    {
    //        _predicate = predicate;

    //        _internalList = list;
    //        _index = start;
    //        _end = end;

    //        current = default(T);
    //        _theStamp = theStamp;
    //    }


    //    public void Dispose()
    //    {
    //        //Do nothing
    //    }

    //    public bool MoveNext()
    //    {
    //        var list = _internalList;

    //        if (list.stamp != _theStamp)
    //            throw new CollectionModifiedException();

    //        if (_state == -2)
    //            _wrapperEnumerator = (Enumerator<T>)list.GetEnumerator();
    //        else
    //            _wrapperEnumerator.UpdateReference(list, _index, _end, _theStamp);


    //        if (_index < _end)
    //        {
    //            _state = 1;
    //            while (_wrapperEnumerator.MoveNext())
    //            {
    //                var temp = _wrapperEnumerator.Current;
    //                _index++;
    //                if (_predicate(temp))
    //                {
    //                    current = temp;
    //                    return true;
    //                }
    //            }
    //        }

    //        current = default(T);
    //        return false;
    //    }

    //    public void Reset()
    //    {
    //        _index = 0;
    //        current = default(T);
    //        _end = 0;
    //    }

    //    public T Current { get { return current; } }

    //    object IEnumerator.Current
    //    {
    //        get { return current; }
    //    }

    //    public WhereEnumerator<T> Clone()
    //    {
    //        var enumerator = new WhereEnumerator<T>
    //        {
    //            _internalList = _internalList,
    //            current = default(T),

    //        };
    //        return enumerator;
    //    }

    //    public override IEnumerator<T> GetEnumerator()
    //    {
    //        var enumerator = !IsMainThread ? Clone() : this;
    //        return enumerator;
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return GetEnumerator();
    //    }
    //}
}
