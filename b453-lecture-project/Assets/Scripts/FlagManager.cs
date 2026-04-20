using System.Collections.Generic;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    // reference variables
    public static FlagManager Instance;
    private Camera cam;
    public GameObject flagPrefab;
    public Team playerTeam;

    // flag variables
    public List<Flag> redFlags = new List<Flag>();
    public List<Flag> yellowFlags = new List<Flag>();
    public List<Flag> greenFlags = new List<Flag>();
    public List<Flag> blueFlags = new List<Flag>();
    private KeyCode playerKey = KeyCode.E;
    private int playerFlagCount = 0;

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    public void Update()
    {
        if (Input.GetKeyDown(playerKey) && playerFlagCount < 2)
        {
            placeFlag(playerTeam);
            playerFlagCount++;
        }
        else if (Input.GetKeyDown(playerKey))
        {
            MoveNearestFlag(playerTeam);
        }
    }

    private void placeFlag (Team team)
    {
        Vector3 spawnPos = cam.ScreenToWorldPoint(Input.mousePosition);
        spawnPos.z = 0;

        GameObject newFlag = Instantiate(
            flagPrefab,
            spawnPos,
            Quaternion.identity
        );

        Flag flag = newFlag.GetComponent<Flag>();
        flag.Initialize(team);

        AddFlagToList(flag, team);
    }

    public Flag GetNearestFlag(Vector2 position, Team team)
    {
        Flag nearest = null;
        float minDist = Mathf.Infinity;
        List<Flag> Flags = new List<Flag>();

        switch (team)
        {
            case Team.yellow:
                Flags = yellowFlags;
                break;
            case Team.green:
                Flags = greenFlags;
                break;
            case Team.blue:
                Flags = blueFlags;
                break;
            case Team.red:
                Flags = redFlags;
                break;
        }

        foreach (var flag in Flags)
        {
            if (flag.team != team) continue;

            float dist = Vector2.Distance(position, flag.focusPoint.position);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = flag;
            }
        }

        return nearest;
    }

    public void MoveNearestFlag(Team team)
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Flag nearestFlag = GetNearestFlag(mousePos, team);

        nearestFlag.transform.position = mousePos;
    }

    public void AddFlagToList(Flag flag, Team team)
    {
        List<Flag> Flags = new List<Flag>();

        switch (team)
        {
            case Team.yellow:
                Flags = yellowFlags;
                break;
            case Team.green:
                Flags = greenFlags;
                break;
            case Team.blue:
                Flags = blueFlags;
                break;
            case Team.red:
                Flags = redFlags;
                break;
        }

        Flags.Add(flag);
    }

    public void RemoveFlagFromList(Flag flag, Team team)
    {
        List<Flag> Flags = new List<Flag>();

        switch (team)
        {
            case Team.yellow:
                Flags = yellowFlags;
                break;
            case Team.green:
                Flags = greenFlags;
                break;
            case Team.blue:
                Flags = blueFlags;
                break;
            case Team.red:
                Flags = redFlags;
                break;
        }

        Flags.Remove(flag);
    }
}