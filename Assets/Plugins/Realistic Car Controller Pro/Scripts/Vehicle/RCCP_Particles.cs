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

/// <summary>
/// Central particle manager for a vehicle, handling contact/scratch particles during collisions 
/// and wheel slip particles (sparks/smoke) for each wheel.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller Pro/Addons/RCCP Particles")]
public class RCCP_Particles : RCCP_Component {

    /// <summary>
    /// Prefab used for creating spark particles upon collisions (one-time contact).
    /// </summary>
    [Tooltip("Prefab spawned for one-time spark effects on collision contact.")]
    public GameObject contactSparklePrefab;

    /// <summary>
    /// Prefab used for creating scratch particles when collisions persist (OnCollisionStay).
    /// </summary>
    [Tooltip("Prefab spawned for continuous scratch sparks during sustained collisions.")]
    public GameObject scratchSparklePrefab;

    /// <summary>
    /// Prefab used for creating wheel sparkles if a wheel is severely deflated or spinning at high RPM.
    /// </summary>
    [Tooltip("Prefab spawned for wheel sparks when a tire is deflated and spinning fast.")]
    public GameObject wheelSparklePrefab;

    /// <summary>
    /// Collision layer filter. Only collisions against these layers will trigger scratch/contact effects.
    /// </summary>
    [Tooltip("Only collisions with objects on these layers will trigger scratch and contact effects.")]
    public LayerMask collisionFilter = -1;

    private List<ParticleSystem> contactSparkeList = new List<ParticleSystem>();
    private List<ParticleSystem> scratchSparkeList = new List<ParticleSystem>();
    private List<ParticleSystem> wheelSparkleList = new List<ParticleSystem>();

    /// <summary>
    /// Per-slot "currently scratching" state for the scratch pool. Driven by our own bookkeeping
    /// instead of ParticleSystem.isPlaying so a scrape can (re)start immediately even while the
    /// previous burst is still aging out — this is what removed the ~1s start delay.
    /// </summary>
    private bool[] scratchEmitting;

    /// <summary>
    /// Reusable buffer for collision contact points, avoiding a per-frame allocation in OnCollisionStay.
    /// </summary>
    private readonly List<ContactPoint> scratchContacts = new List<ContactPoint>();

    /// <summary>
    /// Defines the slip particle systems used by each wheel, potentially referencing multiple ground friction variations.
    /// </summary>
    [System.Serializable]
    public class WheelParticles {

        [Tooltip("Wheel collider that drives the slip particle emission for this wheel.")]
        public RCCP_WheelCollider wheelCollider;
        [Tooltip("Slip particle systems for each ground friction material on this wheel.")]
        public List<RCCP_WheelSlipParticles> allWheelParticles = new List<RCCP_WheelSlipParticles>();

        /// <summary>
        /// Enables the slip particle at the specified ground index, disabling others.
        /// The rate is set based on total slip and temperature.
        /// </summary>
        /// <param name="index">Index of the ground material particle to enable.</param>
        /// <param name="totalSlip">Combined wheel slip value controlling particle emission rate.</param>
        /// <param name="totalTemp">Wheel temperature value affecting particle behavior.</param>
        public void EnableParticleByIndex(int index, float totalSlip, float totalTemp) {

            // Disable all particles except the one at 'index'.
            for (int i = 0; i < allWheelParticles.Count; i++) {

                if (i != index) {

                    if (allWheelParticles[i] != null)
                        allWheelParticles[i].Emit(false, 0f);

                }

            }

            if (allWheelParticles[index] != null)
                allWheelParticles[index].Emit(true, totalSlip * Mathf.InverseLerp(20f, 125f, totalTemp));

        }

        /// <summary>
        /// Disables all slip particles for this wheel.
        /// </summary>
        public void DisableParticles() {

            for (int i = 0; i < allWheelParticles.Count; i++) {

                if (allWheelParticles[i] != null)
                    allWheelParticles[i].Emit(false, 0f);

            }

        }

    }

    /// <summary>
    /// Holds slip-particle references for each wheel in the vehicle.
    /// </summary>
    [Tooltip("Per-wheel slip particle references, one entry for each wheel collider on the vehicle.")]
    [HideInInspector] public WheelParticles[] wheelParticles;

    /// <summary>
    /// The maximum number of one-time contact spark effects to pool.
    /// </summary>
    private readonly int maximumContactSparkle = 5;

    public override void Start() {

        base.Start();

        // Create pooled contact spark particle objects.
        if (contactSparklePrefab && contactSparkeList.Count < 1) {

            for (int i = 0; i < maximumContactSparkle; i++) {

                GameObject sparks = Instantiate(contactSparklePrefab, transform.position, transform.rotation);
                sparks.transform.SetParent(transform, true);
                sparks.TryGetComponent<ParticleSystem>(out var sparksPS);
                contactSparkeList.Add(sparksPS);
                ParticleSystem.EmissionModule em = sparksPS.emission;
                em.enabled = false;

            }

        }

        // Create pooled scratch spark particle objects.
        if (scratchSparklePrefab && scratchSparkeList.Count < 1) {

            for (int i = 0; i < maximumContactSparkle; i++) {

                GameObject sparks = Instantiate(scratchSparklePrefab, transform.position, transform.rotation);
                sparks.transform.SetParent(transform, true);
                sparks.TryGetComponent<ParticleSystem>(out var sparksPS);
                scratchSparkeList.Add(sparksPS);
                ParticleSystem.EmissionModule em = sparksPS.emission;
                em.enabled = false;

            }

        }

        // Track per-slot scratch emission state once the scratch pool exists.
        scratchEmitting = new bool[scratchSparkeList.Count];

        // Create wheel sparkle systems, one for each wheel, if deflated or spinning quickly.
        if (wheelSparklePrefab && wheelSparkleList.Count < 1) {

            for (int i = 0; i < CarController.AllWheelColliders.Length; i++) {

                GameObject sparks = Instantiate(wheelSparklePrefab, CarController.AllWheelColliders[i].transform.position, transform.rotation);
                sparks.transform.SetParent(CarController.AllWheelColliders[i].transform, true);
                sparks.TryGetComponent<ParticleSystem>(out var sparksPS);
                wheelSparkleList.Add(sparksPS);
                ParticleSystem.EmissionModule em = sparksPS.emission;
                em.enabled = false;

            }

        }

        // Create a list of slip particle systems for each wheel friction type in RCCPGroundMaterials.
        wheelParticles = new WheelParticles[CarController.AllWheelColliders.Length];

        for (int i = 0; i < wheelParticles.Length; i++) {

            wheelParticles[i] = new WheelParticles();

            for (int k = 0; k < RCCPGroundMaterials.frictions.Length; k++) {

                GameObject ps = Instantiate(RCCPGroundMaterials.frictions[k].groundParticles, transform.position, transform.rotation);
                ps.TryGetComponent<ParticleSystem>(out var psComp);
                ParticleSystem.EmissionModule em = psComp.emission;
                em.enabled = false;
                ps.transform.SetParent(CarController.AllWheelColliders[i].transform, false);
                ps.transform.localPosition = Vector3.zero;
                ps.transform.localRotation = Quaternion.identity;

                ps.TryGetComponent<RCCP_WheelSlipParticles>(out var slipParticles);
                wheelParticles[i].allWheelParticles.Add(slipParticles);
                wheelParticles[i].wheelCollider = CarController.AllWheelColliders[i];

            }

        }

    }

    private void Update() {

        // Enable wheel sparkle if the wheel is deflated and rotating quickly; otherwise disable.
        if (wheelSparkleList.Count >= 1) {

            for (int i = 0; i < CarController.AllWheelColliders.Length; i++) {

                if (CarController.AllWheelColliders[i].WheelCollider.enabled) {

                    if (CarController.AllWheelColliders[i].deflated && Mathf.Abs(CarController.AllWheelColliders[i].WheelCollider.rpm) >= 250f) {

                        ParticleSystem.EmissionModule em = wheelSparkleList[i].emission;

                        if (!em.enabled)
                            em.enabled = true;

                    } else {

                        ParticleSystem.EmissionModule em = wheelSparkleList[i].emission;

                        if (em.enabled)
                            em.enabled = false;

                    }

                } else {

                    ParticleSystem.EmissionModule em = wheelSparkleList[i].emission;

                    if (em.enabled)
                        em.enabled = false;

                }

            }

        }

        // Manage wheel slip particle systems based on isSkidding and ground friction index.
        for (int i = 0; i < wheelParticles.Length; i++) {

            if (wheelParticles[i].wheelCollider.WheelCollider.enabled && wheelParticles[i].wheelCollider.isSkidding) {

                wheelParticles[i].EnableParticleByIndex(
                    wheelParticles[i].wheelCollider.groundIndex,
                    wheelParticles[i].wheelCollider.TotalSlip,
                    wheelParticles[i].wheelCollider.totalWheelTemp);

            } else {

                wheelParticles[i].DisableParticles();

            }

        }

    }

    /// <summary>
    /// Called on collisions. Spawns a quick contact particle system (sparks) if collision velocity is sufficient.
    /// </summary>
    /// <param name="collision">The collision data from the physics engine.</param>
    public void OnCollision(Collision collision) {

        if (!enabled)
            return;

        // If no collision points or velocity is too low, ignore.
        if (collision.contactCount < 1)
            return;

        if (collision.relativeVelocity.magnitude < 5)
            return;

        // Find an unused contact spark in the pool and play it at the first contact point.
        for (int i = 0; i < contactSparkeList.Count; i++) {

            if (!contactSparkeList[i].isPlaying) {

                contactSparkeList[i].transform.position = collision.GetContact(0).point;
                ParticleSystem.EmissionModule em = contactSparkeList[i].emission;
                em.rateOverTimeMultiplier = collision.impulse.magnitude / 500f;
                em.enabled = true;
                contactSparkeList[i].Play();
                break;

            }

        }

    }

    /// <summary>
    /// Called each frame while colliding. Drives the scratch particle pool continuously so the sparks
    /// follow the sliding contact point and react to the current sliding speed. Emission is toggled via
    /// our own per-slot state (scratchEmitting) rather than ParticleSystem.isPlaying, so a scrape starts
    /// the instant contact resumes instead of waiting ~1s for the previous burst to age out.
    /// </summary>
    public void OnCollisionStay(Collision collision) {

        if (!enabled)
            return;

        // No contact, sliding too slowly, or off-filter layer: stop any active scratch and bail.
        if (collision.contactCount < 1
            || collision.relativeVelocity.magnitude < .75f
            || ((1 << collision.gameObject.layer) & collisionFilter) == 0) {

            StopAllScratch();
            return;

        }

        // GC-free contact fetch into the reusable buffer.
        collision.GetContacts(scratchContacts);

        float rate = collision.relativeVelocity.magnitude;
        int used = Mathf.Min(scratchContacts.Count, scratchSparkeList.Count);

        for (int i = 0; i < scratchSparkeList.Count; i++) {

            if (scratchSparkeList[i] == null)
                continue;

            ParticleSystem.EmissionModule em = scratchSparkeList[i].emission;

            if (i < used) {

                // Track the live contact point and current sliding speed every frame.
                scratchSparkeList[i].transform.position = scratchContacts[i].point;
                em.rateOverTimeMultiplier = rate;

                if (!em.enabled)
                    em.enabled = true;

                // Start (or resume) emission once per active spell. Gated on our own flag — NOT
                // isPlaying — so it fires even while leftover particles from a prior scrape linger.
                if (!scratchEmitting[i]) {

                    scratchSparkeList[i].Play();
                    scratchEmitting[i] = true;

                }

            } else if (scratchEmitting[i]) {

                // This slot has no contact this frame: stop emitting, let existing particles fade.
                em.enabled = false;
                scratchSparkeList[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
                scratchEmitting[i] = false;

            }

        }

    }

    /// <summary>
    /// Called when a collision ends (no longer in contact). Stops any active scratch particles.
    /// </summary>
    public void OnCollisionExit(Collision collision) {

        if (!enabled)
            return;

        StopAllScratch();

    }

    /// <summary>
    /// Stops emission on every active scratch slot and clears their state, letting live particles fade naturally.
    /// </summary>
    private void StopAllScratch() {

        if (scratchSparkeList == null || scratchEmitting == null)
            return;

        for (int i = 0; i < scratchSparkeList.Count; i++) {

            if (scratchSparkeList[i] == null || !scratchEmitting[i])
                continue;

            ParticleSystem.EmissionModule em = scratchSparkeList[i].emission;
            em.enabled = false;
            scratchSparkeList[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            scratchEmitting[i] = false;

        }

    }

    private void Reset() {

        // Automatically assign default prefabs from RCCP_Settings if none are set.
        contactSparklePrefab = RCCP_Settings.Instance.contactParticles;
        scratchSparklePrefab = RCCP_Settings.Instance.scratchParticles;
        wheelSparklePrefab = RCCP_Settings.Instance.wheelSparkleParticles;

    }

}
