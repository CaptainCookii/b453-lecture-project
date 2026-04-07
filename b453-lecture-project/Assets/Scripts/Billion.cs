using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Billion : MonoBehaviour
{
    // reference variables
    public SpriteRenderer sr;
    public Sprite blueBillion;
    public Sprite yellowBillion;
    public Sprite redBillion;
    public Sprite greenBillion;
    public SpriteRenderer srTurret;
    public Sprite blueTurret;
    public Sprite yellowTurret;
    public Sprite redTurret;
    public Sprite greenTurret;
    public LayerMask billionLayer;
    public Team team;
    private Rigidbody2D rb;
    private Flag targetFlag;
    public Transform innerCircle;
    public Transform firePoint;
    public GameObject bulletPrefab;
    private BillionaireBase owningBase;

    //health variables
    public int maxHealth = 5;
    public int currentHealth = 5;
    public float minInnerScale = 0.30f;

    // movement variables
    public float maxSpeed = 3f;
    public float acceleration = 8f;
    public float decelerationRadius = 1.5f;
    public float separationRadius = .1f;
    public float separationForce = .15f;

    //turret variables
    public float fireInterval = 3f;
    public float attackRange = 3f;
    public int shotDamage = 1;
    public float projSpeed = 12f;
    public float projMaxTravelDist = 7f;
    private float timer;

    private bool initialized = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
    }

    private void OnEnable()
    {
        BillionRegistry.Register(this);
    }

    private void OnDisable()
    {
        BillionRegistry.Unregister(this);
    }

    public void Initialize(BillionaireBase owner, Team team, Vector2 direction)
    {
        owningBase = owner;
        this.team = team;

        SetSprite();
        SetDirection(direction);

        initialized = true;
    }

    private void FixedUpdate()
    {
        if (!initialized)
            return;

        ApplySeparation();
        MoveTowardFlag();
    }

    private void Update()
    {
        if (!initialized)
            return;

        timer += Time.deltaTime;
        Billion target = BillionTargeting.FindNearestEnemy(team, transform.position);
        if (target == null)
            return;

        Vector2 toTarget = target.transform.position - transform.position;
        float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        
        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance > attackRange || timer < fireInterval)
            return;

        Fire();
        timer = 0f;
    }

    private void Fire()
    {
        if (firePoint == null || bulletPrefab == null)
            return;

        GameObject bulletObj = Instantiate(
            bulletPrefab,
            firePoint.position,
            transform.rotation
        );

        Bullet shot = bulletObj.GetComponent<Bullet>();
        shot.Initialize(
                team,
                shotDamage,
                projSpeed,
                projMaxTravelDist,
                false
        );
    }

    private void UpdateVisual()
    {
        if (innerCircle == null)
            return;

        float health01 = (float)currentHealth / maxHealth;
        float scale = Mathf.Lerp(minInnerScale, 1f, health01);
        innerCircle.localScale = new Vector3(scale, scale, 1f);
    }

    private void MoveTowardFlag()
    {
        targetFlag = FlagManager.Instance.GetNearestFlag(transform.position, team);
        if (targetFlag == null) return;
        

        Vector2 toTarget = (Vector2)targetFlag.transform.position - rb.position;       
        float distance = toTarget.magnitude;
        if (distance < 0.05f) return;

        Vector2 desiredDirection = toTarget.normalized;        
        float speedFactor = Mathf.Clamp01(distance / decelerationRadius);       
        float targetSpeed = maxSpeed * speedFactor;        
        Vector2 desiredVelocity = desiredDirection * targetSpeed;       
        Vector2 steering = desiredVelocity - rb.linearVelocity;        
        rb.AddForce(steering * acceleration);
    }

    void ApplySeparation()
    {
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(
            transform.position,
            separationRadius,
            billionLayer
        );

        Vector2 separationVector = Vector2.zero;

        foreach (Collider2D neighbor in neighbors)
        {
            if (neighbor.gameObject == gameObject) continue;

            Vector2 diff = (Vector2)transform.position - (Vector2)neighbor.transform.position;
            float dist = diff.magnitude;

            if (dist > 0)
                separationVector += diff.normalized / dist;
        }

        rb.AddForce(separationVector * separationForce);
    }

    public void TakeDamage(int amount)
    {
        if (!initialized)
            return;

        if (amount <= 0 || currentHealth <= 0)
            return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            owningBase.currentCount--;
            Destroy(gameObject);
            return;
        }

        UpdateVisual();
    }

    public void SetDirection(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * .1f;
    }

    public void SetSprite()
    {
        switch (team)
        {
            case Team.blue:
                sr.sprite = blueBillion;
                srTurret.sprite = blueTurret;
                break;
            case Team.yellow:
                sr.sprite = yellowBillion;
                srTurret.sprite = yellowTurret;
                break;
            case Team.red:
                sr.sprite = redBillion;
                srTurret.sprite = redTurret;
                break;
            case Team.green:
                sr.sprite = greenBillion;
                srTurret.sprite = greenTurret;
                break;
        }
    }
}
