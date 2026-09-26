//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR        // keep compilation clean in builds
using UnityEditor;
#endif

/// <summary>
/// Manages the dynamics of the vehicle.
/// </summary>
[DefaultExecutionOrder(-3)]
[AddComponentMenu("BoneCracker Games/Realistic Car Controller Pro/Addons/RCCP Dynamics")]
public class RCCP_AeroDynamics : RCCP_Component {

    /// <summary>
    /// Center of Mass for the vehicle. If not found, automatically creates a "COM" child object.
    /// </summary>
    public Transform COM {

        get {

            if (com == null) {

                if (transform.Find("COM")) {

                    com = transform.Find("COM");
                    return com;

                }

                GameObject newCom = new GameObject("COM");
                newCom.transform.SetParent(transform, false);
                com = newCom.transform;
                RecalculateCOM();

                return com;

            }

            return com;

        }
        set {

            com = value;

        }

    }

    private Transform com;

    [Header("Forces")]
    [Tooltip("Velocity-squared downward force applied at each wheel to improve high-speed grip.")]
    [Min(0f)] public float downForce = 10f;

    /// <summary>
    /// Air resistance applied to the vehicle based on speed. Higher values cause more aerodynamic drag.
    /// </summary>
    [Tooltip("Aerodynamic drag coefficient opposing forward motion at speed.")]
    [Range(0f, 100f)] public float airResistance = 10f;

    /// <summary>
    /// Deceleration applied to the vehicle based on speed. Higher values cause the vehicle to slow down more quickly.
    /// </summary>
    [Tooltip("Rolling resistance that decelerates the vehicle when not accelerating.")]
    [Range(0f, 100f)] public float wheelResistance = 10f;

    /// <summary>
    /// Ignores the rigidbody drag force while accelerating. Used to achieve the maximum speed easily.
    /// </summary>
    [Tooltip("Zeroes rigidbody drag while throttle is applied, making it easier to reach top speed.")]
    public bool ignoreRigidbodyDragOnAccelerate = false;

    /// <summary>
    /// If true, the COM (Center of Mass) will be dynamically updated each physics frame.
    /// </summary>
    [Tooltip("Recalculates the center of mass every physics frame to follow the COM transform.")]
    public bool dynamicCOM = false;

    /// <summary>
    /// How the inertia tensor override is interpreted.
    /// Multiplier scales Unity's auto-computed tensor per axis (scale-invariant, recommended);
    /// AbsoluteOverride freezes exact principal moments in kg·m² (advanced, single known vehicle).
    /// </summary>
    public enum InertiaTensorMode { Multiplier, AbsoluteOverride }

    [Header("Inertia Tensor")]

    /// <summary>
    /// When enabled, RCCP takes control of the Rigidbody inertia tensor (rotational resistance per axis).
    /// When disabled (default), Unity's automatic inertia tensor is used — identical to stock behavior.
    /// </summary>
    [Tooltip("Take control of the Rigidbody inertia tensor (rotational resistance per axis). When OFF (default), Unity's automatic tensor is used — no change from stock behavior.")]
    public bool overrideInertiaTensor = false;

    /// <summary>
    /// Multiplier scales the auto-computed tensor per axis (works on any vehicle, recommended).
    /// AbsoluteOverride freezes exact kg·m² principal moments (advanced).
    /// </summary>
    [Tooltip("Multiplier scales the auto-computed tensor (recommended, scale-invariant). Absolute freezes exact kg·m² values (advanced).")]
    public InertiaTensorMode inertiaTensorMode = InertiaTensorMode.Multiplier;

    /// <summary>
    /// Per-axis multiplier on the auto-computed inertia tensor (X = Pitch, Y = Yaw, Z = Roll).
    /// 1 = stock. Lower = lighter/twitchier about that axis, higher = heavier/more planted.
    /// Lower Yaw makes the car rotate into turns more eagerly. Used in Multiplier mode.
    /// </summary>
    [Tooltip("Per-axis multiplier (X=Pitch, Y=Yaw, Z=Roll). 1 = stock. <1 lighter/twitchier, >1 heavier/planted. Lower Yaw = rotates into turns more eagerly.")]
    public Vector3 inertiaTensorScale = Vector3.one;

    /// <summary>
    /// Absolute principal moments of inertia in kg·m² (X = Pitch, Y = Yaw, Z = Roll). Used in AbsoluteOverride mode.
    /// </summary>
    [Tooltip("Absolute principal moments in kg·m² (X=Pitch, Y=Yaw, Z=Roll). Used only in Absolute Override mode.")]
    public Vector3 inertiaTensorAbsolute = new Vector3(2000f, 2030f, 400f);

    /// <summary>Minimum allowed per-axis tensor component. Unity treats 0 as infinite inertia (locks the axis), so values are clamped above 0.</summary>
    private const float MinInertiaComponent = 0.0001f;

    /// <summary>Last auto-computed (base) tensor captured during the most recent RecomputeInertia(); editor display only.</summary>
    [System.NonSerialized] public Vector3 lastAutoInertiaTensor = Vector3.one;

    /// <summary>Last applied (resulting) tensor from the most recent RecomputeInertia(); editor display only.</summary>
    [System.NonSerialized] public Vector3 lastAppliedInertiaTensor = Vector3.one;

    /// <summary>
    /// If enabled, the vehicle will automatically reset if it flips upside down.
    /// </summary>
    [Header("Auto Reset")]
    [Tooltip("Automatically flips the vehicle upright if it rolls over and stays nearly stationary.")]
    public bool autoReset = true;

    /// <summary>
    /// Time (in seconds) to wait before resetting the vehicle if it's flipped.
    /// </summary>
    [Tooltip("Seconds the vehicle must remain flipped before it is automatically reset.")]
    [Min(0f)] public float autoResetTime = 3f;
    private float autoResetTimer = 0f;

    private float defaultDrag = -1f;

    public override void Start() {

        base.Start();

        // Assigning center of mass position once at the start.
        CarController.Rigid.centerOfMass = transform.InverseTransformPoint(COM.position);

        // Apply (or clear) the inertia-tensor override now that COM is set.
        // Default off → leaves Unity's automatic tensor untouched (identical to stock behavior).
        RecomputeInertia();

    }

    private void FixedUpdate() {

        // Dynamically updates COM if enabled.
        if (dynamicCOM)
            CarController.Rigid.centerOfMass = transform.InverseTransformPoint(COM.position);

        if (defaultDrag < 0)
            defaultDrag = CarController.Rigid.linearDamping;

        if (ignoreRigidbodyDragOnAccelerate)
            CarController.Rigid.linearDamping = defaultDrag * (1f - CarController.throttleInput_V);

        // Local forward speed (z-axis).
        float linearSpeed = transform.InverseTransformDirection(CarController.Rigid.linearVelocity).z;
        float speedMagnitude = Mathf.Abs(linearSpeed);

        if (CarController.IsGrounded) {

            // --------------------------------------------------------------------
            // 1. Downforce (velocity-squared version for more "realistic" feel)
            // --------------------------------------------------------------------
            // If you want to keep it linear, use:
            //   float downforceValue = downForce * speedMagnitude;
            float downforceValue = downForce * (speedMagnitude * speedMagnitude);
            downforceValue *= .15f;

            // Apply downforce in local downward direction.
            RCCP_WheelCollider[] wheelColliders = CarController.AllWheelColliders;

            if (wheelColliders != null && wheelColliders.Length > 0) {

                for (int i = 0; i < wheelColliders.Length; i++) {

                    if (wheelColliders[i] != null)
                        CarController.Rigid.AddForceAtPosition(-transform.up * (downforceValue / (float)wheelColliders.Length), wheelColliders[i].transform.position, ForceMode.Force);

                }

            }

        }

        // --------------------------------------------------------------------
        // 2. Aerodynamic drag (quadratic in speed, monotonic)
        // --------------------------------------------------------------------
        float dragForce = airResistance * 0.025f * speedMagnitude * speedMagnitude;

        Vector3 worldVel = CarController.Rigid.linearVelocity;

        if (worldVel.sqrMagnitude > 0.01f)
            CarController.Rigid.AddForceAtPosition(-worldVel.normalized * dragForce, COM.position, ForceMode.Force);

        // --------------------------------------------------------------------
        // 3. Rolling resistance (constant magnitude, opposes motion)
        // --------------------------------------------------------------------
        if (CarController.IsGrounded && speedMagnitude > 0.1f) {

            float rollingForce = wheelResistance * 20f;
            float rollingSign = Mathf.Sign(linearSpeed);

            CarController.Rigid.AddRelativeForce(-Vector3.forward * rollingForce * rollingSign, ForceMode.Force);

        }

        // --------------------------------------------------------------------
        // 4. Auto-reset if upside down
        // --------------------------------------------------------------------
        if (autoReset)
            CheckUpsideDown();

    }

    /// <summary>
    /// Checks if the vehicle is upside down and resets it after 'autoResetTime' if speed is low.
    /// </summary>
    private void CheckUpsideDown() {

        // If vehicle speed is under 5, not kinematic, and z rotation is between 60 and 300, reset after the timer.
        if (Mathf.Abs(CarController.absoluteSpeed) < 8f && !CarController.Rigid.isKinematic) {

            if (CarController.transform.eulerAngles.z < 300f && CarController.transform.eulerAngles.z > 60f) {

                autoResetTimer += Time.deltaTime;

                if (autoResetTimer > autoResetTime) {

                    CarController.transform.SetPositionAndRotation(

                        new Vector3(CarController.transform.position.x, CarController.transform.position.y + 3f, CarController.transform.position.z),
                        Quaternion.Euler(0f, CarController.transform.eulerAngles.y, 0f)

                    );

                    autoResetTimer = 0f;

                }

            }

        }

    }

    /// <summary>
    /// Resets the timer used for flipping the vehicle.
    /// </summary>
    public void Reload() {

        autoResetTimer = 0f;

    }

    /// <summary>
    /// Sets the COM (Center of Mass) to a specific local position offset.
    /// This overrides any auto-calculated position.
    /// </summary>
    /// <param name="localOffset">Local position offset relative to this component's transform.</param>
    public void SetCOMOffset(Vector3 localOffset) {

        COM.localPosition = localOffset;

        // Apply immediately to rigidbody if available
        if (CarController != null && CarController.Rigid != null)
            CarController.Rigid.centerOfMass = transform.InverseTransformPoint(COM.position);

    }

    /// <summary>
    /// Recomputes and applies the inertia tensor according to the current mode/fields.
    /// In Multiplier mode the auto-computed tensor is re-derived about the current COM, scaled per axis, then frozen.
    /// In AbsoluteOverride mode the exact principal moments are frozen.
    /// When <see cref="overrideInertiaTensor"/> is false, control is returned to Unity's automatic tensor (stock behavior).
    /// Call this after the colliders or COM change (damage, detachable parts, trailer attach) to refresh the frozen tensor —
    /// while the override is active the tensor is NOT recomputed automatically (frozen-tensor policy).
    /// </summary>
    /// <remarks>
    /// The frozen tensor is anchored to the COM that was current at this call. If <see cref="dynamicCOM"/> is also enabled
    /// (the COM moves every physics frame), the override is intended for a static COM — re-call to re-anchor if needed.
    /// </remarks>
    public void RecomputeInertia() {

        if (CarController == null || CarController.Rigid == null)
            return;

        Rigidbody rb = CarController.Rigid;

        // Override disabled: hand control back to Unity's automatic tensor (no-op if already automatic).
        if (!overrideInertiaTensor) {

            if (!rb.automaticInertiaTensor)
                rb.ResetInertiaTensor();

            lastAutoInertiaTensor = rb.inertiaTensor;
            lastAppliedInertiaTensor = rb.inertiaTensor;
            return;

        }

        // Override enabled: re-derive the automatic tensor about the current COM as the base
        // (also used for the editor read-out), then either scale it or replace it, and freeze.
        // Assigning Rigidbody.inertiaTensor implicitly sets automaticInertiaTensor = false (the value stops auto-recomputing).
        rb.ResetInertiaTensor();
        Vector3 baseTensor = rb.inertiaTensor;
        lastAutoInertiaTensor = baseTensor;

        Vector3 target;

        if (inertiaTensorMode == InertiaTensorMode.Multiplier) {

            Vector3 scale = inertiaTensorScale;

            // Guard against legacy/unset serialized data (a fully zeroed multiplier would null all rotation).
            if (scale == Vector3.zero)
                scale = Vector3.one;

            // Clamp each axis to the inspector's slider floor so a stray hand-edited 0/negative on a single
            // axis produces a light-but-drivable axis rather than a degenerate near-zero (or locked) one.
            scale.x = Mathf.Max(0.05f, scale.x);
            scale.y = Mathf.Max(0.05f, scale.y);
            scale.z = Mathf.Max(0.05f, scale.z);

            target = Vector3.Scale(baseTensor, scale);

        } else {

            target = inertiaTensorAbsolute;

            // Guard against legacy/unset serialized data → fall back to the automatic tensor (no-op).
            if (target == Vector3.zero)
                target = baseTensor;

        }

        rb.inertiaTensor = ClampInertia(target);
        lastAppliedInertiaTensor = rb.inertiaTensor;

    }

    /// <summary>
    /// Clamps each tensor component above zero. Unity interprets a zero component as infinite inertia (locking that axis),
    /// so a zero/negative multiplier or value must never reach the Rigidbody.
    /// </summary>
    private static Vector3 ClampInertia(Vector3 t) {

        return new Vector3(
            Mathf.Max(MinInertiaComponent, t.x),
            Mathf.Max(MinInertiaComponent, t.y),
            Mathf.Max(MinInertiaComponent, t.z)
        );

    }

    /// <summary>
    /// Re-calculates the COM position using renderer bounds plus user-defined
    /// racing biases. Call this after changing the vehicle's meshes.
    /// </summary>
    public void RecalculateCOM() {

        Transform t = COM;

        // --------------------------------------------------------------------
        // 1) Build combined world-space bounds of every visible renderer
        //    (ignores trails / particles so exhaust smoke doesn�t skew the box)
        // --------------------------------------------------------------------
        RCCP_CarController carController = GetComponentInParent<RCCP_CarController>(true);

        // Not part of a vehicle hierarchy yet - use fallback COM
        if (carController == null) {

            if (t) t.localPosition = new Vector3(0f, -.25f, 0f);
            return;

        }

        Renderer[] renderers = carController.GetComponentsInChildren<Renderer>(false);

        Bounds worldBounds = new Bounds();
        bool hasBounds = false;

        foreach (Renderer r in renderers) {

            if (r is TrailRenderer || r is ParticleSystemRenderer)
                continue;

            if (!hasBounds) {

                worldBounds = new Bounds(r.bounds.center, r.bounds.size);
                hasBounds = true;

            } else {

                worldBounds.Encapsulate(r.bounds);

            }

        }

        // Fallback if no renderers were found
        if (!hasBounds) {

            t.localPosition = new Vector3(0f, -.25f, 0f);
            return;

        }

        // --------------------------------------------------------------------
        // 2) Convert to local space so the COM lives neatly under the root
        // --------------------------------------------------------------------
        Vector3 localCenter = transform.InverseTransformPoint(worldBounds.center);
        Vector3 ext = worldBounds.extents;             // half-sizes (world)

        // Because we stay in root space (no scaling on car root in RCCP),
        // using world extents for biasing is sufficiently accurate.

        // --------------------------------------------------------------------
        // 3) Apply race-car biases
        // --------------------------------------------------------------------
        // Vertical: bottom + (height * bias)
        float bottomLocalY = localCenter.y - ext.y;
        localCenter.y = bottomLocalY + (worldBounds.size.y * .3f);

        // Longitudinal (Z): centre � fore/aft bias
        localCenter.z = 0f;

        // Lateral (X): centre � left/right bias
        localCenter.x = 0f;

        // --------------------------------------------------------------------
        // 4) Commit
        // --------------------------------------------------------------------
        t.localPosition = localCenter;

    }


    private void Reset() {

        // Runs when the component is first added from the Inspector.
        RecalculateCOM();

    }

}
