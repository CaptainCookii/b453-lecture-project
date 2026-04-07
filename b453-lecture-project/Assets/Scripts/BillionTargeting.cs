using UnityEngine;

public static class BillionTargeting
{
    public static Billion FindNearestEnemy(Team team, Vector2 origin)
    {
        Billion target = null;
        float closestDist = float.PositiveInfinity;

        foreach (var candidate in BillionRegistry.All)
        {
            if (candidate == null || candidate.team == team)
                continue;

            float dist = ((Vector2)candidate.transform.position - origin).sqrMagnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                target = candidate;
            }
        }

        return target;
    }
}
