using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    // reference variables
    private Rigidbody2D rb;
    private BillionaireBase owner;
    private Team team;
    public LayerMask wallLayers;
    public SpriteRenderer sr;
    public Sprite blueBullet;
    public Sprite yellowBullet;
    public Sprite redBullet;
    public Sprite greenBullet;
    private Vector2 spawnPosition;
    private bool isBig;

    // bullet variables
    private float maxTravelDistance;
    private float damage;

    private bool initialized = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(BillionaireBase owner, Team team, float damage, float speed, float maxTravelDistance, bool isBig)
    {
        this.owner = owner;
        this.team = team;
        this.damage = damage;
        this.maxTravelDistance = maxTravelDistance;
        this.isBig = isBig;

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
            if (hitBillion.team == team)
                return;

            hitBillion.TakeDamage(damage, owner);
            Destroy(gameObject);
            return;
        }

        BillionaireBase hitBase = other.GetComponentInParent<BillionaireBase>();
        if (hitBase != null)
        {
            if (hitBase.team == team)
                return;

            hitBase.TakeDamage(damage);
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

        if (isBig)
        {
            transform.localScale *= 3;
        }
    }
}
