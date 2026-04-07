using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Flag : MonoBehaviour
{
    // reference variables
    public KeyCode key;
    public Team team;
    private Camera cam;
    public SpriteRenderer sr;
    public Sprite blueFlag;
    public Sprite yellowFlag;
    public Sprite redFlag;
    public Sprite greenFlag;
    public GameObject linePrefab;
    private LineRenderer lr;
    private Vector3 mousePos;

    void Awake()
    {
        cam = Camera.main;

        GameObject lineObj = Instantiate(
            linePrefab,
            new Vector3(0, 0, 0),
            Quaternion.identity
        );

        lr = lineObj.GetComponent<LineRenderer>();
        lr.enabled = false;
    }

    private void Start()
    {
        SetupTeam();
    }

    private void OnMouseDown()
    {
        lr.enabled = true;
        lr.positionCount = 2;
        lr.SetPosition(0, mousePos);
        lr.SetPosition(1, mousePos);
    }

    private void Update()
    {
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        lr.SetPosition(1, mousePos);
    }

    private void OnMouseUp()
    {
        transform.position = mousePos;
        lr.enabled = false;
    }

    public void SetupTeam()
    {
        switch (team)
        {
            case Team.yellow:
                sr.sprite = yellowFlag;
                lr.startColor = Color.yellow;
                lr.endColor = Color.yellow;
                key = KeyCode.Mouse0;
                break;
            case Team.green:
                sr.sprite = greenFlag;
                lr.startColor = Color.green;
                lr.endColor = Color.green;
                key = KeyCode.Mouse1;
                break;
        }
    }
}
