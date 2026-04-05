using UnityEngine;

public class BillionHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth = 5;

    [SerializeField] private Transform innerCircle;
    [SerializeField, Range(0.05f, 0.95f)] private float minInnerScale = 0.30f;

    private void Awake()
    {
        if (currentHealth <= 0)
            currentHealth = maxHealth;

        currentHealth = Mathf.Clamp(currentHealth, 1, maxHealth);
        UpdateVisual();
    }

    private void OnValidate()
    {
        if (maxHealth < 1)
            maxHealth = 1;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (!Application.isPlaying)
            UpdateVisual();
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(2))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
            return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
            return;
        }

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (innerCircle == null)
            return;

        float health01 = (float)currentHealth / maxHealth;
        float scale = Mathf.Lerp(minInnerScale, 1f, health01);
        innerCircle.localScale = new Vector3(scale, scale, 1f);
    }
}
