using UnityEngine;

public enum Team {
    green,
    yellow,
    red,
    blue
}

public class BillionaireBase : MonoBehaviour
{
    public GameObject billionPrefab;
    public SpriteRenderer sr;
    public Team team;
    public float spawnInterval = 1f;
    public int maxBillions = 10;
    public float spawnRadius = 0.5f;

    private int currentCount;
    private float timer;

    void Start()
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
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval && currentCount < maxBillions)
        {
            SpawnBillion();
            timer = 0f;
        }
    }

    void SpawnBillion()
    {
        Vector2 spawnPos = (Vector2)transform.position + new Vector2(1, 0);

        GameObject newBillion = Instantiate(
            billionPrefab,
            spawnPos,
            Quaternion.identity
        );

        Billion billion = newBillion.GetComponent<Billion>();

        billion.team = team;
        billion.SetColor();

        // Example direction (you can modify this logic)
        Vector2 direction = Random.insideUnitCircle.normalized;
        billion.SetDirection(direction);

        currentCount++;
    }
}
