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
/// Customization applier for vehicles.
/// 6 Upgrade managers for paints, wheels, upgrades, spoilers, customization, and sirens.
/// </summary>
[DefaultExecutionOrder(10)]
[AddComponentMenu("BoneCracker Games/Realistic Car Controller Pro/Addons/RCCP Customizer")]
public class RCCP_Customizer : RCCP_Component {

    /// <summary>
    /// Save file name of the vehicle.
    /// </summary>
    [Tooltip("PlayerPrefs key used to persist this vehicle's customization loadout.")]
    public string saveFileName = "";

    /// <summary>
    /// Auto initializes all managers. Disable it for networked vehicles.
    /// </summary>
    [Tooltip("Initializes all upgrade managers automatically; disable for networked vehicles.")]
    public bool autoInitialize = true;

    /// <summary>
    /// Loads the latest loadout.
    /// </summary>
    [Tooltip("Loads the saved customization loadout from PlayerPrefs on startup.")]
    public bool autoLoadLoadout = true;

    /// <summary>
    /// Auto save.
    /// </summary>
    [Tooltip("Automatically saves the loadout to PlayerPrefs whenever a customization changes.")]
    public bool autoSave = true;

    /// <summary>
    /// Defines the method/timing used for initializing all upgrade managers.
    /// </summary>
    public enum InitializeMethod { Awake, OnEnable, Start, DelayedWithFixedUpdate }

    /// <summary>
    /// Selected method (timing) for initializing all upgrade managers. 
    /// [SerializeField] removed as requested; no extra attributes used.
    /// </summary>
    [Tooltip("Unity callback used to initialize upgrade managers (Awake, OnEnable, Start, or delayed).")]
    public InitializeMethod initializeMethod = InitializeMethod.Start;

    /// <summary>
    /// Loadout class.
    /// </summary>
    [Tooltip("Current customization state including paint, wheels, upgrades, and accessories.")]
    public RCCP_CustomizationLoadout loadout = new RCCP_CustomizationLoadout();

    #region All upgrade managers

    /// <summary>
    /// Paint manager.
    /// </summary>
    private RCCP_VehicleUpgrade_PaintManager _paintManager;
    public RCCP_VehicleUpgrade_PaintManager PaintManager {

        get {

            if (_paintManager == null)
                _paintManager = GetComponentInChildren<RCCP_VehicleUpgrade_PaintManager>(true);

            return _paintManager;

        }

    }

    /// <summary>
    /// Wheel Manager.
    /// </summary>
    private RCCP_VehicleUpgrade_WheelManager _wheelManager;
    public RCCP_VehicleUpgrade_WheelManager WheelManager {

        get {

            if (_wheelManager == null)
                _wheelManager = GetComponentInChildren<RCCP_VehicleUpgrade_WheelManager>(true);

            return _wheelManager;

        }

    }

    /// <summary>
    /// Upgrade Manager.
    /// </summary>
    private RCCP_VehicleUpgrade_UpgradeManager _upgradeManager;
    public RCCP_VehicleUpgrade_UpgradeManager UpgradeManager {

        get {

            if (_upgradeManager == null)
                _upgradeManager = GetComponentInChildren<RCCP_VehicleUpgrade_UpgradeManager>(true);

            return _upgradeManager;

        }

    }

    /// <summary>
    /// Spoiler Manager.
    /// </summary>
    private RCCP_VehicleUpgrade_SpoilerManager _spoilerManager;
    public RCCP_VehicleUpgrade_SpoilerManager SpoilerManager {

        get {

            if (_spoilerManager == null)
                _spoilerManager = GetComponentInChildren<RCCP_VehicleUpgrade_SpoilerManager>(true);

            return _spoilerManager;

        }

    }

    /// <summary>
    /// Siren Manager.
    /// </summary>
    private RCCP_VehicleUpgrade_SirenManager _sirenManager;
    public RCCP_VehicleUpgrade_SirenManager SirenManager {

        get {

            if (_sirenManager == null)
                _sirenManager = GetComponentInChildren<RCCP_VehicleUpgrade_SirenManager>(true);

            return _sirenManager;

        }

    }

    /// <summary>
    /// Customization Manager.
    /// </summary>
    private RCCP_VehicleUpgrade_CustomizationManager _customizationManager;
    public RCCP_VehicleUpgrade_CustomizationManager CustomizationManager {

        get {

            if (_customizationManager == null)
                _customizationManager = GetComponentInChildren<RCCP_VehicleUpgrade_CustomizationManager>(true);

            return _customizationManager;

        }

    }

    /// <summary>
    /// Decal Manager.
    /// </summary>
    private RCCP_VehicleUpgrade_DecalManager _decalManager;
    public RCCP_VehicleUpgrade_DecalManager DecalManager {

        get {

            if (_decalManager == null)
                _decalManager = GetComponentInChildren<RCCP_VehicleUpgrade_DecalManager>(true);

            return _decalManager;

        }

    }

    /// <summary>
    /// Neon Manager.
    /// </summary>
    private RCCP_VehicleUpgrade_NeonManager _neonManager;
    public RCCP_VehicleUpgrade_NeonManager NeonManager {

        get {

            if (_neonManager == null)
                _neonManager = GetComponentInChildren<RCCP_VehicleUpgrade_NeonManager>(true);

            return _neonManager;

        }

    }

    #endregion

    //  V2.51 (T2-3): tracks save keys claimed by active customizers this play session, to detect collisions.
    //  Maps key -> claiming customizer so a stale entry (vehicle already destroyed, e.g. the demo respawn
    //  flow where Destroy() is deferred to end-of-frame while the replacement spawns the same frame) can be
    //  reclaimed silently instead of being false-flagged as a collision.
    private static readonly Dictionary<string, RCCP_Customizer> _activeSaveKeys = new Dictionary<string, RCCP_Customizer>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetActiveSaveKeys() {
        _activeSaveKeys.Clear();
    }

    public override void Awake() {

        base.Awake();

        //  V2.51 (T2-3): ensure a unique, non-empty save key before Load(). An empty saveFileName makes every
        //  runtime-spawned clone persist under the same ("") PlayerPrefs slot and clobber each other.
        EnsureUniqueSaveFileName();

        //  Loads the latest loadout.
        if (autoLoadLoadout)
            Load();

        //  Initializes all managers if selected method is Awake.
        if (initializeMethod == InitializeMethod.Awake)
            Initialize();

    }

    /// <summary>
    /// V2.51 (T2-3): assigns a unique runtime save key when empty, and re-keys on collision with another active
    /// customizer. Correctly-set inspector keys are preserved (no migration). Empty/colliding keys only.
    /// </summary>
    private void EnsureUniqueSaveFileName() {

        if (string.IsNullOrEmpty(saveFileName)) {

            saveFileName = System.Guid.NewGuid().ToString("N");
            _activeSaveKeys[saveFileName] = this;
            Debug.LogWarning("RCCP_Customizer on '" + name + "' had an empty saveFileName - assigned a unique runtime key '" + saveFileName + "'. Set a stable saveFileName in the inspector for persistent per-vehicle loadouts.", this);

        } else if (Application.isPlaying) {

            //  Re-key only when the slot is held by ANOTHER vehicle that is still alive. A free slot, this same
            //  instance, or a destroyed holder (Unity fake-null) are all reclaimed silently - the latter covers
            //  respawning the same demo vehicle, where the outgoing vehicle's Destroy() hasn't run yet.
            if (_activeSaveKeys.TryGetValue(saveFileName, out RCCP_Customizer holder) && holder != null && holder != this) {

                string old = saveFileName;
                saveFileName = old + "_" + System.Guid.NewGuid().ToString("N").Substring(0, 6);
                Debug.LogWarning("RCCP_Customizer on '" + name + "': saveFileName '" + old + "' is in use by another active vehicle - re-keyed to '" + saveFileName + "'. Give each vehicle a unique saveFileName to avoid shared loadouts.", this);

            }

            _activeSaveKeys[saveFileName] = this;

        }

    }

    //  V2.51 (T2-3): release this customizer's save key on destroy so a later spawn re-using the same stable
    //  key (e.g. respawning the same demo vehicle) reclaims it cleanly instead of being flagged as a collision.
    private void OnDestroy() {

        ReleaseSaveKey();

    }

    /// <summary>
    /// V2.51 (T2-3): frees this customizer's claimed save key. OnDestroy covers normal teardown, but Unity's
    /// Destroy() is deferred to end-of-frame - so a caller that destroys the current vehicle and spawns a
    /// replacement in the SAME frame (e.g. RCCP_Demo.Spawn) must call this BEFORE Destroy() so the replacement
    /// reclaims the stable key instead of being needlessly re-keyed. Idempotent and safe to call repeatedly.
    /// </summary>
    public void ReleaseSaveKey() {

        if (!string.IsNullOrEmpty(saveFileName)
            && _activeSaveKeys.TryGetValue(saveFileName, out RCCP_Customizer holder) && holder == this)
            _activeSaveKeys.Remove(saveFileName);

    }

    public override void OnEnable() {

        base.OnEnable();

        //  Initializes all managers if selected method is OnEnable.
        if (initializeMethod == InitializeMethod.OnEnable)
            Initialize();

    }

    public override void Start() {

        base.Start();

        //  Initializes all managers if selected method is Start.
        if (initializeMethod == InitializeMethod.Start)
            Initialize();

        //  Initializes all managers if selected method is delayed with fixed update.
        if (initializeMethod == InitializeMethod.DelayedWithFixedUpdate)
            StartCoroutine(Delayed());

    }

    /// <summary>
    /// Delayed initialization via coroutine if needed.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Delayed() {

        yield return new WaitForFixedUpdate();
        Initialize();

    }

    /// <summary>
    /// Initialize all upgrade managers.
    /// </summary>
    public void Initialize() {

        if (loadout == null)
            loadout = new RCCP_CustomizationLoadout();

        //  Paint manager
        if (PaintManager)
            PaintManager.Initialize();

        //  Wheel manager
        if (WheelManager)
            WheelManager.Initialize();

        //  Upgrade manager
        if (UpgradeManager)
            UpgradeManager.Initialize();

        //  Spoiler manager
        if (SpoilerManager)
            SpoilerManager.Initialize();

        //  Siren manager
        if (SirenManager)
            SirenManager.Initialize();

        //  Customization manager
        if (CustomizationManager)
            CustomizationManager.Initialize();

        //  Decal manager
        if (DecalManager)
            DecalManager.Initialize();

        //  Neon manager
        if (NeonManager)
            NeonManager.Initialize();

    }

    /// <summary>
    /// Retrieves the current customization loadout.
    /// </summary>
    /// <returns></returns>
    public RCCP_CustomizationLoadout GetLoadout() {

        if (loadout != null) {

            return loadout;

        } else {

            loadout = new RCCP_CustomizationLoadout();
            return loadout;

        }

    }

    /// <summary>
    /// Saves the current loadout to PlayerPrefs (JSON).
    /// </summary>
    public void Save() {

        if (loadout == null)
            loadout = new RCCP_CustomizationLoadout();

        PlayerPrefs.SetString(saveFileName, JsonUtility.ToJson(loadout));

    }

    /// <summary>
    /// Loads the previously saved loadout from PlayerPrefs (JSON).
    /// </summary>
    public void Load() {

        if (PlayerPrefs.HasKey(saveFileName))
            loadout = (RCCP_CustomizationLoadout)JsonUtility.FromJson(PlayerPrefs.GetString(saveFileName), typeof(RCCP_CustomizationLoadout));

    }

    /// <summary>
    /// Deletes the last saved loadout and restores vehicle upgrades to default.
    /// </summary>
    public void Delete() {

        if (PlayerPrefs.HasKey(saveFileName))
            PlayerPrefs.DeleteKey(saveFileName);

        loadout = new RCCP_CustomizationLoadout();

        //  Restore paint manager
        if (PaintManager)
            PaintManager.Restore();

        //  Restore wheel manager
        if (WheelManager)
            WheelManager.Restore();

        //  Restore upgrade manager
        if (UpgradeManager)
            UpgradeManager.Restore();

        //  Restore spoiler manager
        if (SpoilerManager)
            SpoilerManager.Restore();

        //  Restore siren manager
        if (SirenManager)
            SirenManager.Restore();

        //  Restore customization manager
        if (CustomizationManager)
            CustomizationManager.Restore();

        //  Restore decal manager
        if (DecalManager)
            DecalManager.Restore();

        //  Restore neon manager
        if (NeonManager)
            NeonManager.Restore();

    }

    /// <summary>
    /// Hides all visual upgrades such as spoilers, sirens, decals, and neon.
    /// </summary>
    public void HideAll() {

        if (SpoilerManager)
            SpoilerManager.DisableAll();

        if (SirenManager)
            SirenManager.DisableAll();

        if (DecalManager)
            DecalManager.DisableAll();

        if (NeonManager)
            NeonManager.DisableAll();

    }

    /// <summary>
    /// Shows all visual upgrades such as spoilers, sirens, decals, and neon.
    /// </summary>
    public void ShowAll() {

        if (SpoilerManager)
            SpoilerManager.EnableAll();

        if (SirenManager)
            SirenManager.EnableAll();

        if (DecalManager)
            DecalManager.EnableAll();

        if (NeonManager)
            NeonManager.EnableAll();

    }

    /// <summary>
    /// Reload method reserved for future usage (currently empty).
    /// </summary>
    public void Reload() {

        //

    }

    private void Reset() {

        RCCP_CarController carController = GetComponentInParent<RCCP_CarController>(true);

        if (carController != null)
            saveFileName = carController.transform.name;

    }

}
