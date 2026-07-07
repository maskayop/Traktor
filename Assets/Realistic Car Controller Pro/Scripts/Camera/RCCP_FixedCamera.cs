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
/// Fixed camera system for RCCP Camera. It simply parents the RCCP Camera, and calculates target position, rotation, FOV, etc...
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller Pro/Camera/RCCP Fixed Camera")]
public class RCCP_FixedCamera : RCCP_Singleton<RCCP_FixedCamera> {

    /// <summary>
    /// Target position.
    /// </summary>
    private Vector3 targetPosition;

    /// <summary>
    /// Max distance.
    /// </summary>
    [Min(0f), Tooltip("Maximum distance before the camera repositions to a new fixed viewpoint.")]
    public float maxDistance = 50f;

    /// <summary>
    /// Current distance.
    /// </summary>
    private float distance = 0f;

    /// <summary>
    /// FOV limits.
    /// </summary>
    [Min(0f), Tooltip("Narrowest field of view when the vehicle is far away.")]
    public float minimumFOV = 20f;

    /// <summary>
    /// FOV limits.
    /// </summary>
    [Min(0f), Tooltip("Widest field of view when the vehicle is close.")]
    public float maximumFOV = 60f;

    /// <summary>
    /// Can track now?
    /// </summary>
    [Tooltip("Whether the fixed camera is actively tracking the vehicle.")]
    public bool canTrackNow = false;

    // Reused RaycastHit buffer for ChangePosition. Physics.RaycastAll allocates a fresh array on
    // every call; the non-alloc overload fills this buffer instead. 16 is plenty for the single
    // ray fired here — grows if a future change ever saturates it.
    private RaycastHit[] raycastHitsBuffer = new RaycastHit[16];

    private void LateUpdate() {

        //  If can't track now, return.
        if (!canTrackNow)
            return;

        RCCP_Camera mCamera = RCCP_SceneManager.Instance.activePlayerCamera;

        // If current camera is null, return.
        if (!mCamera)
            return;

        // If current camera is null, return.
        if (mCamera.cameraTarget.playerVehicle == null)
            return;

        //  Getting camera target.
        Transform target = mCamera.cameraTarget.playerVehicle.transform;

        //  Getting speed of the vehicle and calculating the distance.
        float speed = mCamera.cameraTarget.playerVehicle.absoluteSpeed;
        distance = Vector3.Distance(transform.position, target.position);

        //  Calculating and setting field of view of the camera.
        mCamera.targetFieldOfView = Mathf.Lerp(distance > maxDistance / 10f ? maximumFOV : 70f, minimumFOV, (distance * 1.5f) / maxDistance);

        //  Setting target position.
        targetPosition = target.transform.position;
        targetPosition += target.transform.rotation * Vector3.forward * (speed * .025f);

        //  Moving camera to the correct position.
        transform.Translate((-target.forward * speed) / 50f * Time.deltaTime);

        //  Always look at the target.
        transform.LookAt(targetPosition);

        //  If distance exceeds max distance, change position.
        if (distance > maxDistance)
            ChangePosition();

    }

    /// <summary>
    /// Changes position of the camera.
    /// </summary>
    public void ChangePosition() {

        //  If can't track now, return.
        if (!canTrackNow)
            return;

        // If current camera is null, return.
        if (!RCCP_SceneManager.Instance.activePlayerCamera)
            return;

        // If current camera is null, return.
        if (RCCP_SceneManager.Instance.activePlayerCamera.cameraTarget.playerVehicle == null)
            return;

        //  Getting camera target.
        Transform target = RCCP_SceneManager.Instance.activePlayerCamera.cameraTarget.playerVehicle.transform;

        if (target == null)
            return;

        //  Creating random angle.
        float randomizedAngle = UnityEngine.Random.Range(-15f, 15f);

        Vector3 rayDir = Quaternion.AngleAxis(randomizedAngle, target.up) * target.forward;
        int hitCount = Physics.RaycastNonAlloc(target.position, rayDir, raycastHitsBuffer, maxDistance);
        bool positioned = false;

        for (int i = 0; i < hitCount; i++) {

            if (!positioned && !raycastHitsBuffer[i].transform.IsChildOf(target) && !raycastHitsBuffer[i].collider.isTrigger) {

                positioned = true;
                transform.position = raycastHitsBuffer[i].point;
                transform.LookAt(target.position);
                transform.position += transform.forward * 3f;

            }

        }

        if (!positioned) {

            transform.position = target.position + new Vector3(0f, Random.Range(2.5f, 7.5f), 0f);
            transform.position += Quaternion.AngleAxis(randomizedAngle, target.up) * target.forward * (maxDistance * .9f);

        }

    }

}
