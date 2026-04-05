using System.Collections.Generic;

public static class BillionRegistry
{
    private static readonly List<Billion> billions = new List<Billion>();

    public static void Register(Billion billion)
    {
        if (billion != null && !billions.Contains(billion))
            billions.Add(billion);
    }

    public static void Unregister(Billion billion)
    {
        if (billion != null)
            billions.Remove(billion);
    }

    public static IEnumerable<Billion> All => billions;
}