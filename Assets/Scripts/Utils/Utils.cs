using System;
using System.Collections.Generic;
using System.Linq;

public static class Utils
{
    public static Dictionary<Guid, long> Combine(Dictionary<Guid,long> a, Dictionary<Guid, long> b)
    {
        var largest =  a.Count > b.Count ? a : b;
        var smallest = a.Count > b.Count ? b : a;
        foreach (var kvpair in smallest)
        {
            if (largest.ContainsKey(kvpair.Key))
            {
                largest[kvpair.Key] += kvpair.Value;
            }
            else
            {
                largest.Add(kvpair.Key, kvpair.Value);
            }
        }

        return largest;
    }
    
    public static Dictionary<Guid, long> Combine(Dictionary<Guid, long> original, float b)
    {
        var keys = original.Keys.ToList();
        foreach (var key in keys)
        {
            original[key] = (long)(original[key] *b);
        }

        return original;
    }
}