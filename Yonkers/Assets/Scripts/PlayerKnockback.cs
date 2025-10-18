using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class PlayerKnockback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CharacterController cc;
    [SerializeField] Rigidbody rb;
    [Tooltip("Non-trigger capsule used ONLY during knockback physics.")]
    [SerializeField] CapsuleCollider physicsCollider;
    [Tooltip("Scripts to disable during knockback (Player Controller, etc.).")]
    [SerializeField] MonoBehaviour[] controlToDisable;

    [Header("Landing Detection")]
    [Tooltip("Which layers count as being the ground?")]
    [SerializeField] LayerMask groundMask = ~0;
    [Tooltip("Vertical offset from the player pivot down toward the feet for the ground probe.")]
    [SerializeField] Vector3 feetOffset = new Vector3(0f, 0.20f, 0f);
    [Tooltip("Radius of the feet probe sphere.")]
    [SerializeField] float groundRadius = 0.25f;
    [Tooltip("Max surface angle (degrees) that will be considered as 'ground'.")]
    [SerializeField, Range(0f, 60f)] float maxGroundAngle = 55f;
    [Tooltip("Control is returned when speed is below this (m/s).")]
    [SerializeField] float landSpeedThreshold = 0.6f;
    [Tooltip("Always stay airborne at least this long (seconds).")]
    [SerializeField] float minAirTime = 0.25f;
    [Tooltip("Failsafe timeout (seconds) to hand back control if something goes wrong.")]
    [SerializeField] float maxAirTime = 3.0f;

    [Header("Rigidbody Setup")]
    [Tooltip("If enabled, player will stay upright when being knocked back.")]
    [SerializeField] bool freezeXZRotation = true;
    [Tooltip("Limit the the maximum velocity that the player can achieve when knocked back (turn this up only if player is phasing through objects at high velocity).")]
    [SerializeField] float maxAirVelocity = 0f;

    bool inKnockback;

    static Collider[] _groundHits = new Collider[12];

    void Reset()
    {
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        physicsCollider = GetComponent<CapsuleCollider>();
    }

    void Awake()
    {
        // Ensure all components are present
        if (!cc) cc = GetComponent<CharacterController>();
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!physicsCollider) physicsCollider = GetComponent<CapsuleCollider>();

        // Make sure the physics collider matches the character controller
        SyncPhysicsColliderToCC();

        // Make sure the physics collider starts disabled
        if (physicsCollider) physicsCollider.enabled = false;

        // RB should be kinematic during CC control
        if (rb)
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            if (freezeXZRotation)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }
    }
  void SyncPhysicsColliderToCC()
    {
        if (cc && physicsCollider)
        {
            physicsCollider.center = cc.center;
            physicsCollider.radius = cc.radius;
            physicsCollider.height = cc.height;
            physicsCollider.direction = 1;
            physicsCollider.isTrigger = false;
        }
    }

    /// <summary>
    /// Begin the temporary-physics knockback. While active:
    /// - CC is disabled, RB is non-kinematic, physics collider is enabled
    /// - control scripts are disabled
    /// - control returns after landing + slowing down (or timeout)
    /// </summary>
    public void BeginPhysicsKnockback(Vector3 explosionPos, float force, float radius, float upModifier = 0.5f)
    {
        if (!isActiveAndEnabled || inKnockback) return;
        StartCoroutine(KnockRoutine(explosionPos, force, radius, upModifier));
    }

    IEnumerator KnockRoutine(Vector3 pos, float force, float radius, float up)
    {
        inKnockback = true;

        // Disable player controls
        SetControls(false);

        // RB prep
        if (freezeXZRotation)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
        }
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Enable physics
        if (physicsCollider) 
            physicsCollider.enabled = true;
        cc.enabled = false;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Apply force
        rb.AddExplosionForce(force, pos, radius, up, ForceMode.Impulse);

        // Enforce a minimum airtime so tiny bumps don't give the player instant control
        float start = Time.time;
        if (minAirTime > 0f) yield return new WaitForSeconds(minAirTime);

        // Stay in ragdoll until the player is grounded and moving slowly, or the max airtime is reached
        while (Time.time - start < maxAirTime)
        {
            // Use max air velocity if set
            if (maxAirVelocity > 0f)
            {
                float sq = rb.linearVelocity.sqrMagnitude;
                float maxSq = maxAirVelocity * maxAirVelocity;
                if (sq > maxSq)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * maxAirVelocity;
                }
            }

            bool grounded = IsGroundedRB();
            bool slow = rb.linearVelocity.sqrMagnitude <= (landSpeedThreshold * landSpeedThreshold);

            if (grounded && slow) break;
            yield return null;
        }

        // Snap-to-ground to avoid hovering right above colliders
        if (Physics.Raycast(rb.position + Vector3.up * 0.5f, Vector3.down, out var hit, 2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            rb.position = hit.point + Vector3.up * 0.02f;
        }

        // Turn off physics and return controls to the player
        Vector3 snapPos = rb.position;
        rb.isKinematic = true;
        cc.enabled = true;
        cc.Move((snapPos - cc.transform.position) + Vector3.up * 0.02f);
        if (physicsCollider) 
            physicsCollider.enabled = false;
        SetControls(true);

        inKnockback = false;
    }

    void SetControls(bool enabled)
    {
        if (controlToDisable != null)
        {
            for (int i = 0; i < controlToDisable.Length; i++)
            {
                var mb = controlToDisable[i];
                if (mb) mb.enabled = enabled;
            }
        }
    }

    bool IsGroundedRB()
    {
        Vector3 feet = transform.position - feetOffset;

        int count = Physics.OverlapSphereNonAlloc(
            feet,
            groundRadius,
            _groundHits,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < count; i++)
        {
            var c = _groundHits[i];
            if (!c) continue;

            // Player collision ignores itself
            if (c.attachedRigidbody == rb) continue;
            if (c.transform.root == transform.root) continue;

            // Test for slopes. If the slope is too steep, don't count it as ground.
            Ray ray = new Ray(feet + Vector3.up * 0.05f, Vector3.down);
            if (Physics.Raycast(ray, out var hit, 0.30f, groundMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == c)
                {
                    float angle = Vector3.Angle(hit.normal, Vector3.up);
                    if (angle <= maxGroundAngle) return true;
                    else continue;
                }
            }
            else
            {
                // If a collider was touched but raycast couldn't hit it, treat it as ground anyway.
                return true;
            }
        }

        return false;
    }
}

