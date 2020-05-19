using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace algorithms {
    public class LinkedList<T> : IEnumerable<T> {
        private int length;
        private int version;
        private Node<T> head;

        public LinkedList() {
            length = 0;
            version = 0;
            head = null;
        }

        public void Clear() {
            length = 0;
            ++version;
            head = null;
        }

        public void Add(T value) {
            if (head == null) {
                head = new Node<T>(value);
            } else {
                var current = head;
                for (; current.Next != null; current = current.Next);
                var node = new Node<T>(value);
                current.Next = node;
            }

            ++length;
            ++version;
        }

        public void AddRange(IEnumerable<T> collection) {
            foreach (var t in collection) {
                Add(t);
            }
        }

        public void Insert(int index, T value) {
            var curr = head;
            for (var i = 0; i < index; i++) {
                curr = curr.Next;
            }
            var node = new Node<T>(value);
            curr.Next = node;
            var next = curr.Next;
            node.Next = next;
            ++length;
            ++version;
        }

        public void InsertRange(int index, IEnumerable<T> collection) {
            var idx = 0;
            foreach (var item in collection) {
                Insert(idx+index, item);
                idx++;
            }
        }

        public void RemoveAt(int index) {
            if(index >= length) return;
            if (index == 0) {
                head = head.Next;
                return;
            }
            var prev = head;
            var curr = prev.Next;
            for (var i = 0; i < index-1; i++) {
                prev = prev.Next;
                curr = curr.Next;
            }

            prev.Next = curr.Next;
            --length;
            ++version;
        }

        public bool Contains(T value) {
            return Enumerable.Contains(this, value);
        }

        public int Count => length;
        
        public T this[int index] {
            get {
                if(index >= length) throw new IndexOutOfRangeException();
                var curr = head;
                for (var i = 0; i < index; i++) {
                    curr = curr.Next;
                }

                return curr.Data;
            }
            set {
                if(index >= length) throw new IndexOutOfRangeException();
                var curr = head;
                for (var i = 0; i < index; i++) {
                    curr = curr.Next;
                }

                curr.Data = value;
                ++version;
            }
        }
        public IEnumerator<T> GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new Enumerator(this);
        }
        
        internal class Node<T> {
            internal T Data;
            internal Node<T> Next;

            public Node(T data) {
                Data = data;
                Next = null;
            }
        }

        public struct Enumerator : IEnumerator, IDisposable, IEnumerator<T> {
            private LinkedList<T> list;
            private int index;
            private int version;
            private T current;
            
            internal Enumerator(LinkedList<T> list)
            {
                this.list = list;
                index = 0;
                version = list.version;
                current = default;
            }
            
            public bool MoveNext() {
                if (version != list.version || (uint)index >= (uint)list.length) return MoveNextRare();
                current = list[index];
                ++index;
                return true;
            }

            private bool MoveNextRare() {
                if(version != list.version)           
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute");

                index = list.length + 1;
                current = default;
                return false;
            }

            public void Reset() {
                if(version != list.version)           
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute");
                index = 0;
                current = default;
            }

            public T Current {
                get {
                    if (index == 0 || index == list.length + 1)
                        throw new InvalidOperationException();
                    return current;
                }
            }
            object IEnumerator.Current => Current;

            public void Dispose() {
            }
        } 
    }
}