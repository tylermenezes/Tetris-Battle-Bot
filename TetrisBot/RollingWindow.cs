using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrisBot
{
    class RollingWindow<T>
    {
        public readonly int Size;
        private int UsedSize = 0;

        T[] elements;

        public RollingWindow(int size)
        {
            Size = size;
            elements = new T[size];
        }

        public void Add(T elem)
        {
            for (int i = UsedSize; i > 0; i--)
            {
                elements[i] = elements[i - 1];
            }

            elements[0] = elem;

            if (UsedSize < Size - 1)
            {
                UsedSize++;
            }
        }

        public bool HasValue(int i)
        {
            return i < UsedSize;
        }

        public T Get(int i)
        {
            return elements[i];
        }
    }
}
