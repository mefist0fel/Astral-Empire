using System;
using System.Collections;
using System.Collections.Generic;

namespace Model {
    public sealed class PriorityQueueCustom<P, T>: IEnumerable<P> where T : IComparable {

        private readonly List<P> itemList;
        private readonly List<T> orderList;
        public int Count { get { return orderList.Count; } }
        public readonly bool IsDescending = false;

        public PriorityQueueCustom() {
            orderList = new List<T>();
            itemList = new List<P>();
        }

        public void Enqueue(P item, T order) {
            itemList.Add(item);
            orderList.Add(order);
            int i = Count - 1;

            while (i > 0) {
                int p = (i - 1) / 2;
                if ((IsDescending ? -1 : 1) * orderList[p].CompareTo(order) <= 0) break;

                orderList[i] = orderList[p];
                itemList[i] = itemList[p];
                i = p;
            }

            if (Count > 0) {
                itemList[i] = item;
                orderList[i] = order;
            }
        }

        public P Dequeue() {
            P target = Peek();
            var count = Count;
            T root = orderList[count - 1];
            P rootItem = itemList[count - 1];
            orderList.RemoveAt(count - 1);
            itemList.RemoveAt(count - 1);

            int i = 0;
            while (i * 2 + 1 < Count) {
                int a = i * 2 + 1;
                int b = i * 2 + 2;
                int c = b < Count && (IsDescending ? -1 : 1) * orderList[b].CompareTo(orderList[a]) < 0 ? b : a;

                if ((IsDescending ? -1 : 1) * orderList[c].CompareTo(root) >= 0)
                    break;
                orderList[i] = orderList[c];
                itemList[i] = itemList[c];
                i = c;
            }

            if (Count > 0) {
                orderList[i] = root;
                itemList[i] = rootItem;
            }
            return target;
        }

        public P Peek() {
            if (Count == 0) throw new InvalidOperationException("Queue is empty.");
            return itemList[0];
        }

        public void Clear() {
            orderList.Clear();
        }

        public IEnumerator<P> GetEnumerator() {
            return itemList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return itemList.GetEnumerator();
        }
    }
}