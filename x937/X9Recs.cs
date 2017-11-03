using System.Collections;

namespace x937
{
    public class X9Recs : CollectionBase
    {
        public int Add(X9Rec newX9Rec)
        {
            return List.Add(newX9Rec);
        }

        public void Remove(int index = -1)
        {
            if (index == -1)
            {
                index = List.Count - 1;
            }
            if (index >= 0 && index < List.Count)
            {
                List.RemoveAt(index);
            }
        }

        public X9Rec this[int index] => (X9Rec) (List[index]);
    }
}