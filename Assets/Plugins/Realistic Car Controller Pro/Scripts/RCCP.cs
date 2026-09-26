//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
/// API for instantiating, registering new RCCP vehicles, and changes at runtime.
///</summary>
public static class RCCP {

    ///<summary>
    /// Loud no-op guard (V2.51 trust fix): warns instead of silently bailing when the target vehicle is null.
    ///</summary>
    private static bool Require(RCCP_CarController vehicle, string caller) {

        if (vehicle == null) {
            Debug.LogWarning("RCCP." + caller + " skipped: target vehicle is null.");
            return false;
        }

        return true;

    }

    ///<summary>
    /// Loud no-op guard (V2.51 trust fix): warns instead of silently bailing when a required sub-component is missing.
    /// Pass the component itself (Unity's implicit bool operator handles destroyed/null objects).
    ///</summary>
    private static bool Require(bool componentExists, string componentName, RCCP_CarController vehicle, string caller) {

        if (!componentExists) {
            Debug.LogWarning("RCCP." + caller + " skipped: no " + componentName + " on '" + (vehicle != null ? vehicle.name : "null") + "'.");
            return false;
        }

        return true;

    }

    ///<summary>
    /// Spawn a RCCP vehicle prefab with given position, rotation, sets its controllable, and engine state.
    ///</summary>
    /// <param name="vehiclePrefab">Vehicle prefab to instantiate.</param>
    /// <param name="position">World position to spawn the vehicle at.</param>
    /// <param name="rotation">World rotation to spawn the vehicle with.</param>
    /// <param name="registerAsPlayerVehicle">If true, registers the spawned vehicle as the active player vehicle.</param>
    /// <param name="isControllable">If true, the player can control the spawned vehicle.</param>
    /// <param name="isEngineRunning">If true, the engine starts running immediately after spawn.</param>
    /// <returns>The spawned RCCP_CarController instance.</returns>
    public static RCCP_CarController SpawnRCC(RCCP_CarController vehiclePrefab, Vector3 position, Quaternion rotation, bool registerAsPlayerVehicle, bool isControllable, bool isEngineRunning) {

        if (vehiclePrefab == null) {
            Debug.LogError("RCCP: vehiclePrefab is null! Cannot spawn.");
            return null;
        }

        // V2.51 (T2-1 fix): stash the opt-out BEFORE Instantiate. RCCP vehicle prefabs ship ACTIVE, so Unity
        // runs OnEnable synchronously inside Instantiate — assigning neverAutoRegister afterwards is too late
        // (OnRCCPSpawned has already fired and the scene manager auto-registered this vehicle as the player,
        // stealing the camera). OnEnable consumes this flag before firing OnRCCPSpawned, so
        // registerAsPlayerVehicle:false now wins for both active and inactive prefabs.
        RCCP_CarController.SetPendingNeverAutoRegister(!registerAsPlayerVehicle);

        RCCP_CarController spawnedRCC = GameObject.Instantiate(vehiclePrefab, position, rotation);

        spawnedRCC.gameObject.SetActive(true);
        spawnedRCC.SetCanControl(isControllable);

        if (registerAsPlayerVehicle)
            RCCP_SceneManager.Instance.RegisterPlayer(spawnedRCC);

        if (isEngineRunning)
            spawnedRCC.StartEngine();
        else
            spawnedRCC.KillEngine();

        return spawnedRCC;

    }

    ///<summary>
    /// Registers the vehicle as player vehicle.
    ///</summary>
    /// <param name="vehicle">Vehicle to register as the active player vehicle.</param>
    public static void RegisterPlayerVehicle(RCCP_CarController vehicle) {

        if (!Require(vehicle, "RegisterPlayerVehicle"))
            return;

        RCCP_SceneManager.Instance.RegisterPlayer(vehicle);

    }

    ///<summary>
    /// Registers the vehicle as player vehicle with controllable state.
    ///</summary>
    /// <param name="vehicle">Vehicle to register as the active player vehicle.</param>
    /// <param name="isControllable">If true, the player can control this vehicle.</param>
    public static void RegisterPlayerVehicle(RCCP_CarController vehicle, bool isControllable) {

        if (!Require(vehicle, "RegisterPlayerVehicle"))
            return;

        RCCP_SceneManager.Instance.RegisterPlayer(vehicle, isControllable);

    }

    ///<summary>
    /// Registers the vehicle as player vehicle with controllable and engine state.
    ///</summary>
    /// <param name="vehicle">Vehicle to register as the active player vehicle.</param>
    /// <param name="isControllable">If true, the player can control this vehicle.</param>
    /// <param name="engineState">If true, the engine will be running after registration.</param>
    public static void RegisterPlayerVehicle(RCCP_CarController vehicle, bool isControllable, bool engineState) {

        if (!Require(vehicle, "RegisterPlayerVehicle"))
            return;

        RCCP_SceneManager.Instance.RegisterPlayer(vehicle, isControllable, engineState);

    }

    ///<summary>
    /// De-Registers the player vehicle. 
    ///</summary>
    public static void DeRegisterPlayerVehicle() {

        RCCP_SceneManager.Instance.DeRegisterPlayer();

    }

    ///<summary>
    /// Sets controllable state of the vehicle.
    ///</summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="isControllable">If true, the player can control this vehicle.</param>
    public static void SetControl(RCCP_CarController vehicle, bool isControllable) {

        if (!Require(vehicle, "SetControl"))
            return;

        vehicle.SetCanControl(isControllable);

    }

    ///<summary>
    /// Sets whether the vehicle is currently driven by an external controller.
    ///</summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="isExternal">If true, marks the vehicle as externally controlled.</param>
    public static void SetExternalControl(RCCP_CarController vehicle, bool isExternal) {

        if (!Require(vehicle, "SetExternalControl"))
            return;

        vehicle.externalControl = isExternal;

    }

    ///<summary>
    /// Sets engine state of the vehicle.
    ///</summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="engineState">If true, starts the engine; if false, kills it.</param>
    public static void SetEngine(RCCP_CarController vehicle, bool engineState) {

        if (!Require(vehicle, "SetEngine"))
            return;

        if (engineState)
            vehicle.StartEngine();
        else
            vehicle.KillEngine();

    }

    ///<summary>
    /// Sets the mobile controller type.
    ///</summary>
    /// <param name="mobileController">Mobile controller type to activate.</param>
    public static void SetMobileController(RCCP_Settings.MobileController mobileController) {

        RCCP_SceneManager.Instance.SetMobileController(mobileController);

        if (RCCP_Settings.Instance != null && RCCP_Settings.Instance.verboseLog)
            Debug.Log("Mobile Controller has been changed to " + mobileController.ToString());

    }

    ///<summary>
    /// Sets the vehicle's gearbox to automatic or manual based on the specified state.
    ///</summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="state">If true, sets transmission to Automatic; if false, sets to Manual.</param>
    public static void SetAutomaticGear(RCCP_CarController vehicle, bool state) {

        if (!Require(vehicle, "SetAutomaticGear"))
            return;

        if (!Require(vehicle.Gearbox, "Gearbox", vehicle, "SetAutomaticGear"))
            return;

        vehicle.Gearbox.transmissionType = state ? RCCP_Gearbox.TransmissionType.Automatic : RCCP_Gearbox.TransmissionType.Manual;

    }

    ///<summary>
    /// Sets the vehicle's gearbox to the specified transmission type.
    ///</summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="transmissionType">Transmission type to apply (Automatic, Manual, etc.).</param>
    public static void SetAutomaticGear(RCCP_CarController vehicle, RCCP_Gearbox.TransmissionType transmissionType) {

        if (!Require(vehicle, "SetAutomaticGear"))
            return;

        if (!Require(vehicle.Gearbox, "Gearbox", vehicle, "SetAutomaticGear"))
            return;

        vehicle.Gearbox.transmissionType = transmissionType;

    }

    ///<summary>
    /// Enables or disables hill-start assist (automatic brake hold on slopes) on the target vehicle.
    ///</summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="state">If true, enables hill-start assist.</param>
    public static void SetHillStartAssist(RCCP_CarController vehicle, bool state) {

        if (!Require(vehicle, "SetHillStartAssist"))
            return;

        if (!Require(vehicle.Inputs, "Inputs", vehicle, "SetHillStartAssist"))
            return;

        vehicle.Inputs.hillStartAssist = state;

    }

    ///<summary>
    /// Engages or disengages cruise control on the target vehicle at its current target speed.
    ///</summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="state">If true, engages cruise control.</param>
    public static void SetCruiseControl(RCCP_CarController vehicle, bool state) {

        if (!Require(vehicle, "SetCruiseControl"))
            return;

        if (!Require(vehicle.Inputs, "Inputs", vehicle, "SetCruiseControl"))
            return;

        vehicle.Inputs.SetCruiseControl(state);

    }

    ///<summary>
    /// Engages or disengages cruise control on the target vehicle with a new target speed (km/h).
    ///</summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="state">If true, engages cruise control.</param>
    /// <param name="targetSpeed">Target speed in km/h.</param>
    public static void SetCruiseControl(RCCP_CarController vehicle, bool state, float targetSpeed) {

        if (!Require(vehicle, "SetCruiseControl"))
            return;

        if (!Require(vehicle.Inputs, "Inputs", vehicle, "SetCruiseControl"))
            return;

        vehicle.Inputs.SetCruiseControl(state, targetSpeed);

    }

    ///<summary>
    /// Starts / stops recording the player vehicle.
    ///</summary>
    /// <param name="vehicle">Target vehicle to record.</param>
    public static void StartStopRecord(RCCP_CarController vehicle) {

        if (!Require(vehicle, "StartStopRecord"))
            return;

        if (!Require(vehicle.OtherAddonsManager, "OtherAddonsManager", vehicle, "StartStopRecord"))
            return;

        if (!Require(vehicle.OtherAddonsManager.Recorder, "Recorder", vehicle, "StartStopRecord"))
            return;

        vehicle.OtherAddonsManager.Recorder.Record();

    }

    ///<summary>
    /// Starts / stops replay of the last record.
    ///</summary>
    /// <param name="vehicle">Target vehicle to replay.</param>
    public static void StartStopReplay(RCCP_CarController vehicle) {

        if (!Require(vehicle, "StartStopReplay"))
            return;

        if (!Require(vehicle.OtherAddonsManager, "OtherAddonsManager", vehicle, "StartStopReplay"))
            return;

        if (!Require(vehicle.OtherAddonsManager.Recorder, "Recorder", vehicle, "StartStopReplay"))
            return;

        vehicle.OtherAddonsManager.Recorder.Play();

    }

    ///<summary>
    /// Starts or stops replay of the specified record.
    ///</summary>
    /// <param name="vehicle">Target vehicle to replay.</param>
    /// <param name="recordedClip">Recorded clip to play back.</param>
    public static void StartStopReplay(RCCP_CarController vehicle, RCCP_Recorder.RecordedClip recordedClip) {

        if (!Require(vehicle, "StartStopReplay"))
            return;

        if (!Require(vehicle.OtherAddonsManager, "OtherAddonsManager", vehicle, "StartStopReplay"))
            return;

        if (!Require(vehicle.OtherAddonsManager.Recorder, "Recorder", vehicle, "StartStopReplay"))
            return;

        vehicle.OtherAddonsManager.Recorder.Play(recordedClip);

    }

    ///<summary>
    /// Stops record / replay.
    ///</summary>
    /// <param name="vehicle">Target vehicle to stop recording or replaying.</param>
    public static void StopRecordReplay(RCCP_CarController vehicle) {

        if (!Require(vehicle, "StopRecordReplay"))
            return;

        if (!Require(vehicle.OtherAddonsManager, "OtherAddonsManager", vehicle, "StopRecordReplay"))
            return;

        if (!Require(vehicle.OtherAddonsManager.Recorder, "Recorder", vehicle, "StopRecordReplay"))
            return;

        vehicle.OtherAddonsManager.Recorder.Stop();

    }

    ///<summary>
    /// Sets new behavior.
    ///</summary>
    /// <param name="behaviorIndex">Index of the behavior preset in RCCP_Settings.behaviorTypes array.</param>
    public static void SetBehavior(int behaviorIndex) {

        RCCP_SceneManager.Instance.SetBehavior(behaviorIndex);

        if (RCCP_Settings.Instance != null && RCCP_Settings.Instance.verboseLog)
            Debug.Log("Behavior has been changed to " + behaviorIndex.ToString());

    }

    /// <summary>
    /// Clears the global behavior override (V2.51 trust fix). Counterpart to SetBehavior — turns
    /// RCCP_Settings.overrideBehavior back off so global preset changes no longer affect vehicles. NOTE: vehicles
    /// RETAIN whatever values are currently applied; this does NOT revert a preset already baked into their
    /// components. See RCCP_SceneManager.ClearBehavior.
    /// </summary>
    public static void ClearBehavior() {

        RCCP_SceneManager.Instance.ClearBehavior();

    }

    /// <summary>
    /// Gets the index of a behavior preset by its name.
    /// Returns -1 if not found.
    /// </summary>
    /// <param name="behaviorName">Name of the behavior preset (case-insensitive).</param>
    /// <returns>Index of the matching behavior preset, or -1 if not found.</returns>
    public static int GetBehaviorIndexByName(string behaviorName) {

        if (string.IsNullOrEmpty(behaviorName))
            return -1;

        RCCP_Settings settings = RCCP_Settings.Instance;

        if (settings == null || settings.behaviorTypes == null)
            return -1;

        for (int i = 0; i < settings.behaviorTypes.Length; i++) {

            if (settings.behaviorTypes[i] != null &&
                string.Equals(settings.behaviorTypes[i].behaviorName, behaviorName, System.StringComparison.OrdinalIgnoreCase)) {
                return i;
            }

        }

        return -1;

    }

    /// <summary>
    /// Gets a behavior preset by its name.
    /// Returns null if not found.
    /// </summary>
    /// <param name="behaviorName">Name of the behavior preset (case-insensitive).</param>
    /// <returns>The matching BehaviorType, or null if not found.</returns>
    public static RCCP_Settings.BehaviorType GetBehaviorByName(string behaviorName) {

        int index = GetBehaviorIndexByName(behaviorName);

        if (index >= 0 && RCCP_Settings.Instance != null && index < RCCP_Settings.Instance.behaviorTypes.Length)
            return RCCP_Settings.Instance.behaviorTypes[index];

        return null;

    }

    /// <summary>
    /// Sets new behavior by name.
    /// </summary>
    /// <param name="behaviorName">Name of the behavior preset (case-insensitive)</param>
    public static void SetBehavior(string behaviorName) {

        int index = GetBehaviorIndexByName(behaviorName);

        if (index >= 0) {
            SetBehavior(index);
        } else {
            Debug.LogWarning("Behavior preset '" + behaviorName + "' not found.");
        }

    }

    /// <summary>
    /// Changes the camera mode.
    /// </summary>
    public static void ChangeCamera() {

        RCCP_SceneManager.Instance.ChangeCamera();

    }

    /// <summary>
    /// Transport player vehicle the specified position and rotation.
    /// </summary>
    /// <param name="position">Position.</param>
    /// <param name="rotation">Rotation.</param>
    public static void Transport(Vector3 position, Quaternion rotation) {

        RCCP_SceneManager.Instance.Transport(position, rotation);

    }

    /// <summary>
    /// Transport the target vehicle to specified position and rotation.
    /// </summary>
    /// <param name="vehicle">Target vehicle to transport.</param>
    /// <param name="position">World position to move the vehicle to.</param>
    /// <param name="rotation">World rotation to apply to the vehicle.</param>
    public static void Transport(RCCP_CarController vehicle, Vector3 position, Quaternion rotation) {

        if (!Require(vehicle, "Transport"))
            return;

        RCCP_SceneManager.Instance.Transport(vehicle, position, rotation);

    }

    /// <summary>
    /// Transport the target vehicle to specified position and rotation.
    /// </summary>
    /// <param name="vehicle">Target vehicle to transport.</param>
    /// <param name="position">World position to move the vehicle to.</param>
    /// <param name="rotation">World rotation to apply to the vehicle.</param>
    /// <param name="resetVelocity">If true, resets the vehicle's velocity after transport.</param>
    public static void Transport(RCCP_CarController vehicle, Vector3 position, Quaternion rotation, bool resetVelocity) {

        if (!Require(vehicle, "Transport"))
            return;

        RCCP_SceneManager.Instance.Transport(vehicle, position, rotation, resetVelocity);

    }

    /// <summary>
    /// Cleans all skidmarks on the current scene.
    /// </summary>
    public static void CleanSkidmarks() {

        RCCP_SkidmarksManager.Instance.CleanSkidmarks();

    }

    /// <summary>
    /// Cleans target skidmarks on the current scene.
    /// </summary>
    /// <param name="index">Index of the skidmark set to clean.</param>
    public static void CleanSkidmarks(int index) {

        RCCP_SkidmarksManager.Instance.CleanSkidmarks(index);

    }

    /// <summary>
    /// Repairs the target vehicle.
    /// </summary>
    /// <param name="carController">Target vehicle to repair.</param>
    public static void Repair(RCCP_CarController carController) {

        if (!Require(carController, "Repair"))
            return;

        if (!Require(carController.Damage, "Damage", carController, "Repair"))
            return;

        carController.Damage.repairNow = true;

    }

    /// <summary>
    /// Repairs the player vehicle. Does nothing if no player vehicle is found.
    /// </summary>
    public static void Repair() {

        RCCP_CarController carController = RCCP_SceneManager.Instance.activePlayerVehicle;

        if (!Require(carController, "Repair"))
            return;

        if (!Require(carController.Damage, "Damage", carController, "Repair"))
            return;

        carController.Damage.repairNow = true;

    }

    /// <summary>
    /// Enters photo mode: freezes the simulation and orbits a temporary camera around the player vehicle.
    /// </summary>
    public static void EnterPhotoMode() {

        RCCP_PhotoMode.Instance.EnterPhotoMode();

    }

    /// <summary>
    /// Exits photo mode and restores the simulation.
    /// </summary>
    public static void ExitPhotoMode() {

        RCCP_PhotoMode.Instance.ExitPhotoMode();

    }

    /// <summary>
    /// Captures a super-size screenshot to persistentDataPath/Photos and returns the file path.
    /// </summary>
    public static string CapturePhoto() {

        return RCCP_PhotoMode.Instance.CapturePhoto();

    }

    /// <summary>
    /// Sets the photo mode camera mode (Orbit / FreeCam / AutoOrbit).
    /// </summary>
    public static void SetPhotoCameraMode(RCCP_PhotoMode.PhotoCameraMode mode) {

        RCCP_PhotoMode.Instance.SetCameraMode(mode);

    }

    /// <summary>
    /// Cycles the photo mode camera mode (Orbit -> FreeCam -> AutoOrbit).
    /// </summary>
    public static void CyclePhotoCameraMode() {

        RCCP_PhotoMode.Instance.CyclePhotoCameraMode();

    }

    /// <summary>
    /// Sets the photo mode camera field of view (degrees).
    /// </summary>
    public static void SetPhotoFieldOfView(float fieldOfView) {

        RCCP_PhotoMode.Instance.SetFieldOfView(fieldOfView);

    }

}
