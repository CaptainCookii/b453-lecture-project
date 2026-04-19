using System.Collections.Generic;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    // reference variables
    public static FlagManager Instance;
    private Camera cam;
    public GameObject flagPrefab;

    // flag variables
    public List<Flag> greenFlags = new List<Flag>();
    public List<Flag> yellowFlags = new List<Flag>();
    private KeyCode greenKey = KeyCode.Q;
    private KeyCode yellowKey = KeyCode.E;
    private int greenFlagCount = 0;
    private int yellowFlagCount = 0;

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    public void Update()
    {
        if (Input.GetKeyDown(yellowKey) && yellowFlagCount < 2)
        {
            placeFlag(Team.yellow);
            yellowFlagCount++;
        }
        else if (Input.GetKeyDown(yellowKey))
        {
            MoveNearestFlag(Team.yellow);
        }
        if (Input.GetKeyDown(greenKey) && greenFlagCount < 2)
        {
            placeFlag(Team.green);
            greenFlagCount++;
        }
        else if (Input.GetKeyDown(greenKey))
        {
            MoveNearestFlag(Team.green);
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
        flag.Initialize(
            team
        );

        switch (team)
        {
            case Team.yellow:
                yellowFlags.Add(flag);
                break;
            case Team.green:
                greenFlags.Add(flag);
                break;
        }
    }

    public Flag GetNearestFlag(Vector2 position, Team team)
    {
        Flag nearest = null;
        float minDist = Mathf.Infinity;
        List<Flag> flags = new List<Flag>();

        switch (team)
        {
            case Team.yellow:
                flags = yellowFlags;
                break;
            case Team.green:
                flags = greenFlags;
                break;
        }

        foreach (var flag in flags)
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
}