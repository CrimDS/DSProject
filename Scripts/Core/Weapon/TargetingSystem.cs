using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages finding and cycling through potential targets in the scene.
/// </summary>
public class TargetingSystem : MonoBehaviour
{
    public Transform CurrentTarget { get; private set; }
    
    private List<Damageable> allTargets = new List<Damageable>();
    private int currentTargetIndex = -1;

    void Start()
    {
        // Find all initial targets in the scene.
        allTargets = FindObjectsByType<Damageable>(FindObjectsSortMode.None).ToList();
    }

    /// <summary>
    /// Cycles to the next available target in the list.
    /// </summary>
    public void CycleTarget()
    {
        // Refresh the list of targets in case some have been destroyed
        allTargets = allTargets.Where(t => t != null && t.gameObject.activeInHierarchy).ToList();

        if (allTargets.Count == 0)
        {
            CurrentTarget = null;
            return;
        }

        currentTargetIndex++;
        if (currentTargetIndex >= allTargets.Count)
        {
            currentTargetIndex = 0;
        }

        CurrentTarget = allTargets[currentTargetIndex].transform;
    }
}
