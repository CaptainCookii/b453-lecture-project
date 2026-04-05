using UnityEditor.UI;
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
    public Sprite redBase;
    public Sprite blueBase;
    public Sprite yellowBase;
    public Sprite greenBase;
    public Team team;

    public float spawnInterval = 1f;
    public int maxBillions = 10;
    public float spawnRadius = 0.5f;

    public int currentCount;
    private float timer;

    void Start()
    {
        switch (team)
        {
            case Team.blue:
                sr.sprite = blueBase;
                break;
            case Team.yellow:
                sr.sprite = yellowBase;
                break;
            case Team.red:
                sr.sprite = redBase;
                break;
            case Team.green:
                sr.sprite = greenBase;
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
        billion.SetSprite();
        Vector2 direction = Random.insideUnitCircle.normalized;
        billion.SetDirection(direction);
        billion.owningBase = this;

        currentCount++;
    }
}
