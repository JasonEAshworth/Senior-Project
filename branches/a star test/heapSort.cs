using System;
using System.Collections.Generic;

public class heapSort <T> where T : System.IComparable<T>
{
    private List<T> data;
	public int Count;

	public heapSort ()
	{
        data = new List<T>();
	}

    public void Add (T toAdd) 
    {
        data.Add(toAdd);

        int pos = data.Count;
        while (true)
        {
            int div2 = pos>>1;
            if (div2 - 1 < 0) {
                break;
            }
            if (data[pos-1].CompareTo(data[div2-1]) < 0)
            {
                T temp = data[pos-1];
                data[pos-1] = data[div2-1];
                data[div2-1] = temp;
            }
            else 
            {
                break;
            }
        }
		Count = data.Count;
    }

    public T Pop () 
    {
        T toReturn = data[0];
        data[0] = data[data.Count - 1];
        data[data.Count - 1] = toReturn;
        data.RemoveAt(data.Count - 1);
        Count = data.Count;

        if (data.Count == 0) 
        {
            return toReturn;
        }

        int pos = 1;
        while(true) 
        {
            int pow2 = (pos<<1);
            if (data.Count <= pow2) 
            {
                break;
            }
            else if (data[pos].CompareTo(data[pow2 - 1]) > 0)
            {
                int newPos = pow2 - 1;
                if ( data[pos<<1].CompareTo(data[pow2]) > 0)
                {
                    newPos = pow2;
                }
                T temp = data[pos];
                data[pos] = data[newPos];
                data[newPos] = temp;
                pos = newPos;
            }
            else if (data[pos].CompareTo(data[pow2]) > 0)
            {
                T temp = data[pos];
                data[pos] = data[pow2];
                data[pow2] = temp;
                pos = pow2;
            }
            else 
            {
                break;
            }
		}

        return toReturn;
    }

	public void replace(int nodeToReplace, T replaceValue)
	{
		data [nodeToReplace] = replaceValue;
	}

    public void reheap() 
    {
        List<T> newData = new List<T>();
        while(data.Count > 0) 
        {
            newData.Add(data[data.Count - 1]);
            data.RemoveAt(data.Count - 1);
        }
		data = newData;
		Count = data.Count;
    }

	public bool Contains(T toFind)
	{
		return data.Contains (toFind);
	}

	public T Find(Predicate<T> toFind)
	{
		return data.Find (toFind);
	}

	public int FindIndex(Predicate<T> toFind)
	{
		return data.FindIndex(toFind);
	}

}


