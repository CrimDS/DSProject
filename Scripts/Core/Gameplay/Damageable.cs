using UnityEngine;

/// <summary>
/// Gives an object health and allows it to take damage.
/// </summary>
public class Damageable : MonoBehaviour
{
    [Tooltip("The maximum health of this object.")]
    public float maxHealth = 100f;

    public float CurrentHealth { get; private set; }

    void Awake()
    {
        CurrentHealth = maxHealth;
    }
    
    // OnEnable is a good place to reset health for pooled objects
    void OnEnable()
    {
        CurrentHealth = maxHealth;
    }

    /// <summary>
    /// Applies damage to this object.
    /// </summary>
    /// <param name="damageAmount">The amount of damage to take.</param>
    public void TakeDamage(float damageAmount)
    {
        CurrentHealth -= damageAmount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // For now, we deactivate the object. If it's part of a pool, it will be returned.
        // If not, it will just be disabled in the scene.
        gameObject.SetActive(false);
    }
}
