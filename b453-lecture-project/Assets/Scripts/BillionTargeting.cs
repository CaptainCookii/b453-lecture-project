using UnityEngine;

public static class BillionTargeting
{
    public static GameObject FindNearestEnemy(Team team, Vector2 origin)
    {
        GameObject target = null;
        float closestDist = float.PositiveInfinity;
        

        foreach (var candidate in BillionaireRegistry.All)
        {
            Team candidateTeam;
            if (candidate.GetComponent<Billion>() != null)
            {
                candidateTeam = candidate.GetComponent<Billion>().team;
            }
            else
            {
                candidateTeam = candidate.GetComponent<BillionaireBase>().team;
            }
            if (candidate == null || candidateTeam == team)
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
