using System;
using System.Collections;
using System.Collections.Generic;

namespace WhiteArrow.Incremental
{
    [Serializable]
    public class NestedList<T> : IEnumerable<NestedList<T>.SubList>
    {
        public List<SubList> List;


        public int Count => List.Count;


        public SubList this[int index]
        {
            get => List[index];
            set => List[index] = value;
        }

        public void ForEach(Action<SubList> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            List.ForEach(e => action(e));
        }

        public IEnumerator<SubList> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return List.GetEnumerator();
        }



        [Serializable]
        public class SubList : IEnumerable<T>
        {
            public List<T> List;

            public int Count => List.Count;


            public T this[int index]
            {
                get => List[index];
                set => List[index] = value;
            }

            public void ForEach(Action<T> action)
            {
                if (action == null)
                    throw new ArgumentNullException(nameof(action));

                List.ForEach(e => action(e));
            }

            public IEnumerator<T> GetEnumerator()
            {
                return List.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return List.GetEnumerator();
            }
        }
    }
}
