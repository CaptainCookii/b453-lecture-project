using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TeamFlagController : MonoBehaviour
{
    // reference variables
    private Tilemap wallTilemap;
    private Team team;
    private Transform flagTransform;

    // navigation variables
    [SerializeField] private float holdTimeAtWaypoint;
    [SerializeField] private int waypointSkip;
    [SerializeField] private float targetSnapDistance;
    [SerializeField] private float initialHold;

    private readonly List<Vector3> pathWaypoints = new List<Vector3>();
    private int waypointIndex;
    private float holdTimer;
    private Transform currentTarget;

    private bool initialized = false;

    public void Initialize(Flag flag, Team team, Tilemap wallTileMap)
    {
        this.flagTransform = flag.transform;
        this.team = team;
        this.wallTilemap = wallTileMap;
        
        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (initialHold >= 0)
        {
            initialHold -= Time.deltaTime;
            return;
        }

        if (holdTimer > 0f)
        {
            holdTimer -= Time.deltaTime;
            return;
        }

        Transform desiredTarget = ResolveTarget();
        if (desiredTarget != currentTarget)
        {
            currentTarget = desiredTarget;
            RebuildPath();
        }

        FollowPath();
    }

    private Transform ResolveTarget()
    {
        // if a home base is destroyed target home capture point
        CapturePoint ownCapturePoint = CapturePointRegistry.GetHomeCapturePoint(team);
        BillionaireBase aliveHomeBase = FindAliveBaseForTeam(team);
        if (aliveHomeBase == null && ownCapturePoint != null)
        {
            return ownCapturePoint.transform;
        }

        // if enemy capture point exists target it
        CapturePoint enemyCapturePoint = CapturePointRegistry.GetNearestEnemyCapturePoint(team, flagTransform.position);
        if (enemyCapturePoint != null)
            return enemyCapturePoint.transform;

        // target nearest base if no capture points exist
        BillionaireBase nearestEnemyBase = FindNearestEnemyBase(flagTransform.position);
        if (nearestEnemyBase != null)
            return nearestEnemyBase.transform;

        // none of the above exist
        return null;
    }

    private BillionaireBase FindAliveBaseForTeam(Team t)
    {
        foreach (GameObject baseObj in BillionaireRegistry.All)
        {
            if (baseObj == null) 
                continue;
            if (baseObj.GetComponent<Billion>())
                continue;

            if (baseObj.GetComponent<BillionaireBase>().team == t)
                return baseObj.GetComponent<BillionaireBase>();
        }

        return null;
    }

    private BillionaireBase FindNearestEnemyBase(Vector2 fromPosition)
    {
        BillionaireBase nearest = null;
        float bestDistSqr = float.PositiveInfinity;

        foreach (GameObject baseObj in BillionaireRegistry.All)
        {
            if (baseObj == null)
                continue;
            if (baseObj.GetComponent<Billion>())
                continue;
            if (baseObj.GetComponent<BillionaireBase>().team == team)
                continue;

            float distSqr = ((Vector2)baseObj.transform.position - fromPosition).sqrMagnitude;
            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                nearest = baseObj.GetComponent<BillionaireBase>();
            }
        }

        return nearest;
    }

    private void RebuildPath()
    {
        pathWaypoints.Clear();
        waypointIndex = 0;
        holdTimer = 0f;

        if (flagTransform == null || currentTarget == null || wallTilemap == null)
            return;

        Debug.Log("Test");
        List<Vector2Int> rawPath = ArenaPathfinder.GetPath(
            wallTilemap,
            flagTransform.position,
            currentTarget.position
        );

        if (rawPath == null || rawPath.Count == 0)
        {
            Debug.Log(rawPath == null ? "Raw path is null" : "Raw path is empty");
            return;
        }
            

        
        for (int i = 0; i < rawPath.Count; i += Mathf.Max(1, waypointSkip))
        {
            Vector2Int cell = rawPath[i];
            pathWaypoints.Add(wallTilemap.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0)));
        }

        if (pathWaypoints.Count == 0 || Vector2.Distance(pathWaypoints[pathWaypoints.Count - 1], currentTarget.position) > targetSnapDistance)
            pathWaypoints.Add(currentTarget.position);
    }

    private void FollowPath()
    {
        if (flagTransform == null || pathWaypoints.Count == 0)
            return;
            
        if (waypointIndex >= pathWaypoints.Count)
            return;

        Vector3 targetPos = pathWaypoints[waypointIndex];
        flagTransform.position = targetPos;
        waypointIndex++;
        holdTimer = holdTimeAtWaypoint;
    }
}
