using UnityEngine;

/// <summary>
/// A smart mine that attempts to maneuver behind its target before striking.
/// </summary>
public class FlankMineProjectile : HomingProjectile
{
    [Header("Flanking Properties")]
    [Tooltip("How far behind and to the side of the target the mine will aim for before turning in.")]
    [SerializeField] private float flankOffset = 50f;
    [Tooltip("How close the mine needs to be to its flanking point before it turns to attack.")]
    [SerializeField] private float flankPointArrivalThreshold = 10f;
    [Tooltip("The speed multiplier applied during the final attack run.")]
    [SerializeField] private float attackDashSpeedMultiplier = 2.0f;

    private enum FlankState { Boosting, Flanking, Attacking }
    private FlankState currentState = FlankState.Boosting;
    private Vector3 flankTargetPoint;

    // --- THE FIX ---
    // This FixedUpdate no longer calls the parent's FixedUpdate, preventing duplicate movement logic.
    protected override void FixedUpdate()
    {
        age += Time.fixedDeltaTime; // Manually track age.

        if (target == null)
        {
            FindNewTarget();
            if (target == null)
            {
                // If there's no target, just fly straight.
                rb.linearVelocity = transform.forward * projectileSpeed;
                return;
            }
        }

        // State machine for flight behavior
        switch (currentState)
        {
            case FlankState.Boosting:
                HandleBoostPhase();
                break;
            case FlankState.Flanking:
                HandleFlankingPhase();
                break;
            case FlankState.Attacking:
                HandleAttackingPhase();
                break;
        }
    }

    private void HandleBoostPhase()
    {
        // During the boost phase, the missile flies straight.
        float currentSpeed = projectileSpeed + BoostThrust;
        rb.linearVelocity = transform.forward * currentSpeed;

        if (age > BoostDuration)
        {
            // Boost is over, calculate the flanking point and switch states.
            CalculateFlankPoint();
            currentState = FlankState.Flanking;
        }
    }

    private void HandleFlankingPhase()
    {
        // Guide the missile towards the calculated flanking point.
        Vector3 directionToFlankPoint = (flankTargetPoint - rb.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToFlankPoint);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, turnRate * Time.fixedDeltaTime));
        rb.linearVelocity = transform.forward * projectileSpeed;

        // If we've reached the flanking point, switch to attack mode.
        if (Vector3.Distance(rb.position, flankTargetPoint) < flankPointArrivalThreshold)
        {
            currentState = FlankState.Attacking;
        }
    }

    private void HandleAttackingPhase()
    {
        // Standard homing logic, but with an added speed boost.
        Vector3 directionToTarget = (target.position - rb.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, turnRate * Time.fixedDeltaTime));
        rb.linearVelocity = transform.forward * (projectileSpeed * attackDashSpeedMultiplier);

        // Check for proximity detonation during the attack run.
        if (proximityFuseRadius > 0 && Vector3.Distance(rb.position, target.position) < proximityFuseRadius)
        {
            Explode();
            Damageable damageable = target.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }
    }

    private void CalculateFlankPoint()
    {
        if (target == null) return;

        // Determine a point behind and to the side of the target, relative to the target's orientation.
        Vector3 behindTarget = target.position - (target.forward * flankOffset);
        Vector3 sideOffset = (Random.value > 0.5f ? target.right : -target.right) * flankOffset;
        flankTargetPoint = behindTarget + sideOffset;
    }
    
    protected override void OnCollisionEnter(Collision collision)
    {
        if (currentState == FlankState.Attacking)
        {
            base.OnCollisionEnter(collision);
        }
    }
}

