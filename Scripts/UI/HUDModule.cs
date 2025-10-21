using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the UI display for a single weapon module on the HUD.
/// </summary>
public class HUDModule : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI groupText;
    public Image flashImage;

    [Header("Flash Effect")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.2f;

    private WeaponBase trackedWeapon;

    public void Setup(WeaponBase weapon)
    {
        if (nameText == null || groupText == null || flashImage == null)
        {
            Debug.LogError("HUDModule: One or more UI references are not assigned in the prefab!", this);
            gameObject.SetActive(false);
            return;
        }

        trackedWeapon = weapon;
        if (trackedWeapon == null)
        {
            gameObject.SetActive(false);
            return;
        }

        nameText.text = trackedWeapon.gameObject.name; 
        groupText.text = trackedWeapon.fireGroup.ToString();
        trackedWeapon.OnWeaponFired += HandleWeaponFired;
        flashImage.color = Color.clear;
    }

    private void OnDestroy()
    {
        if (trackedWeapon != null)
        {
            trackedWeapon.OnWeaponFired -= HandleWeaponFired;
        }
    }

    private void HandleWeaponFired()
    {
        StopAllCoroutines();
        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < flashDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / flashDuration);
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        flashImage.color = Color.clear;
    }
}

