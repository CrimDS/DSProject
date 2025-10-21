using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Creates a smooth engine trail using a Line Renderer.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class EngineTrail_LineRenderer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The ship's controller to get velocity data from.")]
    public ShipController shipController;

    [Header("Trail Properties")]
    [Tooltip("The maximum length of the trail in world units.")]
    public float maxTrailLength = 50f;
    [Tooltip("How many points make up the trail. More points = smoother curve.")]
    public int trailResolution = 50;

    private LineRenderer lineRenderer;
    private List<Vector3> trailPoints;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        trailPoints = new List<Vector3>();

        if (shipController == null)
        {
            Debug.LogError("EngineTrail: Ship Controller is not assigned!", this);
            enabled = false;
        }
    }

    void Update()
    {
        float speedPercent = shipController.CurrentVelocityPercent;
        float currentTrailLength = maxTrailLength * speedPercent;

        // Add the current position to the start of the list
        trailPoints.Insert(0, transform.position);

        // Trim the list to maintain the maximum length
        while (trailPoints.Count > trailResolution)
        {
            trailPoints.RemoveAt(trailPoints.Count - 1);
        }

        // Trim the list based on actual world distance
        float totalDistance = 0;
        for (int i = 0; i < trailPoints.Count - 1; i++)
        {
            totalDistance += Vector3.Distance(trailPoints[i], trailPoints[i + 1]);
            if (totalDistance > currentTrailLength)
            {
                // Remove all points beyond the max length
                trailPoints.RemoveRange(i + 1, trailPoints.Count - (i + 1));
                break;
            }
        }

        // Update the Line Renderer
        lineRenderer.positionCount = trailPoints.Count;
        lineRenderer.SetPositions(trailPoints.ToArray());
    }
}
