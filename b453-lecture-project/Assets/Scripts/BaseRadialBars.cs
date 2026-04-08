using UnityEngine;
using UnityEngine.UI;

public class BaseRadialBars : MonoBehaviour
{
    public Image healthBar;
    public Image experienceBar;

    public void SetHealth(float normalized)
    {
        if (healthBar != null)
            healthBar.fillAmount = Mathf.Clamp01(normalized);
    }

    public void SetExperience(float normalized)
    {
        if (experienceBar != null)
            experienceBar.fillAmount = Mathf.Clamp01(normalized);
    }
}