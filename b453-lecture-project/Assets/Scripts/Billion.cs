using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Billion : MonoBehaviour
{
    // constant variables
    private Rigidbody2D rb;
    private Flag targetFlag;
    public SpriteRenderer sr;
    public Sprite blueBillion;
    public Sprite yellowBillion;
    public Sprite redBillion;
    public Sprite greenBillion;
    public Team team;
    public LayerMask billionLayer;

    // movement variables
    public float moveSpeed = 3f;
    public float separationRadius = 0.5f;
    public float separationForce = 5f;
    public float maxSpeed = 5f;
    public float acceleration = 10f;
    public float decelerationRadius = 1.5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        ApplySeparation();
        UpdateTarget();
        MoveTowardFlag();
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

    public void SetDirection(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * .1f;
    }

    void UpdateTarget()
    {
        targetFlag = FlagManager.Instance.GetNearestFlag(transform.position, team);
    }

    void MoveTowardFlag()
    {
        if (targetFlag == null) return;

        Vector2 toTarget = (Vector2)targetFlag.transform.position - rb.position;
        float distance = toTarget.magnitude;

        if (distance < 0.05f) return;

        Vector2 desiredDirection = toTarget.normalized;

        // Slow down when close
        float speedFactor = Mathf.Clamp01(distance / decelerationRadius);

        float targetSpeed = maxSpeed * speedFactor;

        Vector2 desiredVelocity = desiredDirection * targetSpeed;

        Vector2 steering = desiredVelocity - rb.linearVelocity;

        rb.AddForce(steering * acceleration);
    }

    public void SetSprite()
    {
        switch (team)
        {
            case Team.blue:
                sr.sprite = blueBillion;
                break;
            case Team.yellow:
                sr.sprite = yellowBillion;
                break;
            case Team.red:
                sr.sprite = redBillion;
                break;
            case Team.green:
                sr.sprite = greenBillion;
                break;
        }
    }
}
