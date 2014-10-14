using System;
using System.Linq;

namespace AckAck
{
    public class RingBuffer<T>
    {
        readonly T[] _items;
        int _next;
        int _tail;

        public RingBuffer(int capacity, Func<T> constructor)
        {
            _next = _tail = 0;
            _items = Enumerable.Range(1, capacity).Select(_ => constructor.Invoke()).ToArray();
            Capacity = _items.Length;
        }

        public int Next()
        {
            //Or we could block, 
            if ( Count ==  Capacity) throw new Exception("No slots free");
            
            //Don't let the counters grow unbounded
            if (_next >= Capacity && _tail >= Capacity)
            {
                _tail %= Capacity;
                _next %= Capacity;
            }
            return _next++ % Capacity;
        }

        public readonly int Capacity;

        public bool HasAvailable
        {
            get { return Capacity > Count; }
        }

        public int Count
        {
            get
            { return _next - _tail; }
        }
        
        public T this[int slot]
        {
            get { return _items[slot]; }
        }

        public void Release(int slot)
        {
            if (slot != _tail) throw new Exception("Can only release at the tail");
            _tail++;
        }
    }
}