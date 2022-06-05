using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTuple<T1, T2> : MonoBehaviour
{
    NetworkTuple() { }

    public T1 Item1 { get; set; }
    public T2 Item2 { get; set; }

    public static implicit operator NetworkTuple<T1, T2>((T1, T2) t)
    {
         return new NetworkTuple<T1, T2>(){
                      Item1 = t.Item1,
                      Item2 = t.Item2
                    };
    }

    public static implicit operator (T1, T2)(NetworkTuple<T1, T2> t)
    {
        return (t.Item1, t.Item2);
    }
}