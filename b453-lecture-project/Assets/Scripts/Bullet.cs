using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    //constant variables
    private Rigidbody2D rb;
    private GameObject owner;
    private Team team;
    public LayerMask wallLayers;
    public SpriteRenderer sr;
    public Sprite blueBullet;
    public Sprite yellowBullet;
    public Sprite redBullet;
    public Sprite greenBullet;

    //other variables
    private Vector2 spawnPosition;
    public float speed = 12f;
    public float maxTravelDistance = 7f;
    private int damage = 1;
    private bool initialized;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(GameObject owner, Team team, int damage)
    {
        this.owner = owner;
        this.team = team;
        this.damage = damage;

        spawnPosition = transform.position;
        rb.linearVelocity = transform.right * speed;
        SetSprite();

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        float dist = Vector2.Distance(spawnPosition, transform.position);
        if (dist >= maxTravelDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized)
            return;

        Billion hitBillion = other.GetComponentInParent<Billion>();
        if (hitBillion != null)
        {
            if (hitBillion == owner || hitBillion.team == team)
                return;

            hitBillion.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (other.GetComponentInParent<BillionaireBase>() != null)
        {
            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & wallLayers.value) != 0)
        {
            Destroy(gameObject);
            return;
        }

    }

    public void SetSprite()
    {
        switch (team)
        {
            case Team.blue:
                sr.sprite = blueBullet;
                break;
            case Team.yellow:
                sr.sprite = yellowBullet;
                break;
            case Team.red:
                sr.sprite = redBullet;
                break;
            case Team.green:
                sr.sprite = greenBullet;
                break;
        }
    }
}
