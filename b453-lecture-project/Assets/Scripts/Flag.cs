using UnityEngine;

public class Flag : MonoBehaviour
{
    // reference variables
    private Team playerTeam;
    public Team team;
    private Camera cam;
    public SpriteRenderer sr;
    public Sprite blueFlag;
    public Sprite yellowFlag;
    public Sprite redFlag;
    public Sprite greenFlag;
    public GameObject linePrefab;
    public Transform focusPoint;
    private LineRenderer lr;
    private Vector3 mousePos;

    void Awake()
    {
        cam = Camera.main;
    }

    public void Initialize(Team team)
    {
        this.team = team;

        GameObject lineObj = Instantiate(
            linePrefab,
            new Vector3(0, 0, 0),
            Quaternion.identity
        );

        lr = lineObj.GetComponent<LineRenderer>();
        lr.enabled = false;

        this.playerTeam = FlagManager.Instance.playerTeam;
        SetupTeam();
    }

    private void Update()
    {
        if (team.Equals(playerTeam))
        {
            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            lr.SetPosition(1, mousePos);
        }
    }

    private void OnMouseDown()
    {
        if (team.Equals(playerTeam))
        {
            lr.enabled = true;
            lr.positionCount = 2;
            lr.SetPosition(0, mousePos);
            lr.SetPosition(1, mousePos);
        }
    }

    private void OnMouseUp()
    {
        if (team.Equals(playerTeam))
        {
            transform.position = mousePos;
            lr.enabled = false;
        }
    }

    public void SetupTeam()
    {
        switch (team)
        {
            case Team.yellow:
                sr.sprite = yellowFlag;
                lr.startColor = Color.yellow;
                lr.endColor = Color.yellow;
                break;
            case Team.green:
                sr.sprite = greenFlag;
                lr.startColor = Color.green;
                lr.endColor = Color.green;
                break;
            case Team.blue:
                sr.sprite = blueFlag;
                lr.startColor = Color.blue;
                lr.endColor = Color.blue;
                break;
            case Team.red:
                sr.sprite = redFlag;
                lr.startColor = Color.red;
                lr.endColor = Color.red;
                break;
        }
    }
}
