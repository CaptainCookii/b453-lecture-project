using System.Collections.Generic;
using UnityEngine;

public static class CapturePointRegistry
{
    private static readonly List<CapturePoint> points = new List<CapturePoint>();

    public static void Register(CapturePoint point)
    {
        if (point != null && !points.Contains(point))
            points.Add(point);
    }

    public static void Unregister(CapturePoint point)
    {
        if (point != null)
            points.Remove(point);
    }

    public static CapturePoint GetHomeCapturePoint(Team team)
    {
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i] != null && points[i].originatingTeam == team)
                return points[i];
        }

        return null;
    }

    public static CapturePoint GetNearestEnemyCapturePoint(Team team, Vector2 fromPosition)
    {
        CapturePoint nearest = null;
        float bestDistSqr = float.PositiveInfinity;

        for (int i = 0; i < points.Count; i++)
        {
            CapturePoint cp = points[i];
            if (cp == null || cp.originatingTeam == team)
                continue;

            float distSqr = ((Vector2)cp.transform.position - fromPosition).sqrMagnitude;
            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                nearest = cp;
            }
        }

        return nearest;
    }
}