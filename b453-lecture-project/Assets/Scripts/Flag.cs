using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Flag : MonoBehaviour
{
    public Team team;
    public SpriteRenderer sr;
    public Sprite blueFlag;
    public Sprite yellowFlag;
    public Sprite redFlag;
    public Sprite greenFlag;

    public KeyCode key;

    private Camera cam;

    public bool dragging;

    void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        SetupTeam();
    }


    void Update()
    {
        //Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        //mousePos.z = 0;
        //if (Input.GetKeyDown(key) && (mousePos == transform.position))
        //{
        //    dragging = true;
        //}
        //if (Input.GetKeyUp(key))
        //{
        //    transform.position = mousePos;
        //    dragging = false;
        //}
    }

    private void OnMouseDrag() 
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos;
    }

    public void SetupTeam()
    {
        switch (team)
        {
            case Team.yellow:
                sr.sprite = yellowFlag;
                key = KeyCode.Mouse1;
                break;
            case Team.green:
                sr.sprite = greenFlag;
                key = KeyCode.Mouse0;
                break;
        }
    }
}
