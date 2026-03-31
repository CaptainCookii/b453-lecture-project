using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlagManager : MonoBehaviour
{
    public static FlagManager Instance;
    public GameObject flagPrefab;
    public List<Flag> flags = new List<Flag>();
    private KeyCode yellowKey = KeyCode.E;
    private KeyCode greenKey = KeyCode.Q;

    private Camera cam;


    void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    public void Update()
    {
        if (Input.GetKeyDown(yellowKey))
        {
            placeFlag(Team.yellow);
        }
        if (Input.GetKeyDown(greenKey))
        {
            placeFlag(Team.green);
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
        flags.Add(flag);
        flag.team = team;

    }

    public Flag GetNearestFlag(Vector2 position, Team team)
    {
        Flag nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var flag in flags)
        {
            if (flag.team != team) continue;

            float dist = Vector2.Distance(position, flag.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = flag;
            }
        }

        return nearest;
    }
}