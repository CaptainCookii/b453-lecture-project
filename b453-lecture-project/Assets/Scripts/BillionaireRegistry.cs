using System.Collections.Generic;
using UnityEngine;

public static class BillionaireRegistry
{
    private static readonly List<GameObject> billions = new List<GameObject>();

    public static void Register(GameObject billion)
    {
        if (billion != null && !billions.Contains(billion))
            billions.Add(billion);
    }

    public static void Unregister(GameObject billion)
    {
        if (billion != null)
            billions.Remove(billion);
    }

    public static IEnumerable<GameObject> All => billions;
}