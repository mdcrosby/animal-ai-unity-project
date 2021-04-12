using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Tooltip("Maximum amount of health")]
    public float maxHealth = 10f;
    public float healthDecrement = 0.001f;

    // [Tooltip("Health ratio at which the critical health vignette starts appearing")]
    // public float criticalHealthRatio = 0.3f;

    // public UnityAction<float, GameObject> onDamaged;
    // public UnityAction<float> onHealed;
    // public UnityAction onDie;

    public float currentHealth { get; set; }
    // public bool invincible { get; set; }
    // public bool canPickup() => currentHealth < maxHealth;

    // public float getRatio() => currentHealth / maxHealth;
    // public bool isCritical() => getRatio() <= criticalHealthRatio;

    // bool m_IsDead;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        currentHealth -= healthDecrement;
    }
}
