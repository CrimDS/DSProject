using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("Ship References")]
    public GameObject playerShip;

    [Header("UI Elements")]
    public TextMeshProUGUI velocityText;
    public TextMeshProUGUI headingText;
    public TextMeshProUGUI targetNameText;
    public Slider hullBar;
    public Slider energyBar;
    
    [Header("Weapon Module UI")]
    [Tooltip("The UI prefab for a single weapon module.")]
    public GameObject modulePrefab;
    [Tooltip("The parent object with a GridLayoutGroup where modules will be placed.")]
    public Transform moduleGridParent;

    private ShipController shipController;
    private Damageable shipDamageable;
    private TargetingSystem shipTargetingSystem;
    private WeaponSystem shipWeaponSystem;

    void Awake()
    {
        if (playerShip == null)
        {
            Debug.LogError("HUDManager: Player Ship GameObject is not assigned!", this);
            enabled = false;
            return;
        }

        shipController = playerShip.GetComponent<ShipController>();
        shipDamageable = playerShip.GetComponent<Damageable>();
        shipTargetingSystem = playerShip.GetComponent<TargetingSystem>();
        shipWeaponSystem = playerShip.GetComponent<WeaponSystem>();

        if (shipController == null || shipDamageable == null || shipTargetingSystem == null || shipWeaponSystem == null)
        {
            Debug.LogError("HUDManager: Player Ship is missing one or more required components.", this);
            enabled = false;
        }
    }
    
    void Start()
    {
        CreateWeaponModules();
    }

    void Update()
    {
        if (velocityText != null) UpdateVelocityDisplay();
        if (headingText != null) UpdateHeadingDisplay();
        if (targetNameText != null) UpdateTargetDisplay();
        if (hullBar != null) UpdateHullDisplay();
        if (energyBar != null) UpdateEnergyDisplay();
    }

    private void CreateWeaponModules()
    {
        if (modulePrefab == null || moduleGridParent == null)
        {
            return;
        }

        foreach (var weapon in shipWeaponSystem.GetAllWeapons())
        {
            GameObject moduleGO = Instantiate(modulePrefab, moduleGridParent);
            HUDModule moduleUI = moduleGO.GetComponent<HUDModule>();
            if (moduleUI != null)
            {
                moduleUI.Setup(weapon);
            }
        }
    }

    private void UpdateVelocityDisplay()
    {
        velocityText.text = $"Velocity: {shipController.CurrentVelocity:F1} / {shipController.GetTargetVelocity():F1}";
    }

    private void UpdateHeadingDisplay()
    {
        headingText.text = $"Heading: {shipController.CurrentHeadingDegrees:F0}° / {shipController.TargetHeadingDegrees:F0}°";
    }

    private void UpdateTargetDisplay()
    {
        targetNameText.text = shipTargetingSystem.CurrentTarget != null ? $"Target: {shipTargetingSystem.CurrentTarget.name}" : "Target: None";
    }

    private void UpdateHullDisplay()
    {
        hullBar.value = shipDamageable.CurrentHealth / shipDamageable.maxHealth;
    }

    private void UpdateEnergyDisplay()
    {
        energyBar.value = shipController.CurrentEnergy / shipController.maxEnergy;
    }
}

