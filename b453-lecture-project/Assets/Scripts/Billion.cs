using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Billion : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float separationRadius = 0.5f;
    public float separationForce = 5f;
    public LayerMask billionLayer;
    public SpriteRenderer sr;
    public Team team;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        ApplySeparation();
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

    public void SetColor()
    {
        switch (team)
        {
            case Team.blue:
                sr.color = Color.blue;
                break;
            case Team.yellow:
                sr.color = Color.yellow;
                break;
            case Team.red:
                sr.color = Color.red;
                break;
            case Team.green:
                sr.color = Color.green;
                break;
        }
    }
}
