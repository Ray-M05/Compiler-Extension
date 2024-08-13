using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Compiler;

namespace ListExtensions;
public static class Extensions
{
    public static List<T> Find<T>(this List<T> list, Compiler.Expression pred)
    {
        if (pred is Predicate predicate)
        {
            List<T> custom = new();
            foreach (var item in list)
            {
                if ((bool)predicate.Evaluate(null, item))
                    custom.Add(item);
            }
            return custom;
        }
        else
            throw new Exception("Find Argument must be a predicate");
    }

    public static void Shuffle<T>(this List<T> list)
    {
        //TODO: aqui va una comprobacion del deck
        int n = list.Count;
        System.Random random = new System.Random();
        while (n > 0)
        {
            n--;
            int k = random.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

    public static T Pop<T>(this List<T> list)
    {
        if (list.Count > 0)
        {
            T obj = list[list.Count - 1];
            list.Remove(obj);
            return obj;
        }
        throw new Exception("Trying to Pop from an empty List");
    }

    public static void Push<T>(this List<T> list, T item)
    {
        list.Add(item);
    }

    public static void SendBottom<T>(this List<T> list, T item)
    {
        list.Add(item);
        list.RemoveAt(list.Count - 1);
        list.Insert(0, item);
    }

    //TODO:
    public static void Addcard()
    {

    }

    public static void Removecard()
    {

    }
    
    public static bool? PlayerOwner;
    public static bool? AddPosibility;
    public static string MyName="";
    public static int? MaxElements;
}

