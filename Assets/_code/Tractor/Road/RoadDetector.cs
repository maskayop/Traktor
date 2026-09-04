using UnityEngine;

namespace Tractor
{
    public class RoadDetector : MonoBehaviour
    {
        [SerializeField] WheelRaycast[] wheelRaycasts = new WheelRaycast[4];

        [Header("Частота обновления")]
        [SerializeField] float updateDelay = 1.0f;

        [Header("Info")]
        public int onRoad = 1;
        public bool wrongDirection = false;
        public bool onLane = false;
        public bool onCross = false;

        public string laneStatus = "";

        // x = FL, y = FR, z = BL, w = BR
        public Vector4 halfLanes = Vector4.zero;
        public Vector4 laneNumbers = Vector4.zero;

        Vector4 notOnRoadVector = new Vector4(-9, -9, -9, -9);

        float currentTime = 0;

        void Update()
        {
            currentTime -= Time.deltaTime;

            if (currentTime < 0)
            {
                Detect();
                currentTime = updateDelay;
            }
        }

        void Detect()
        {
            GetInfoFromWheels();
            CheckIfOnRoad();
            CheckDirection();
            UpdateLaneStatus();
        }

        void GetInfoFromWheels()
        {
            for (int i = 0; i < wheelRaycasts.Length; i++)
            {
                if (wheelRaycasts[i].wheelIndex == WheelRaycast.WheelIndex.FrontLeft)
                {
                    halfLanes.x = wheelRaycasts[i].GetHalfLane();
                    laneNumbers.x = wheelRaycasts[i].GetLaneNumber();
                }
                else if (wheelRaycasts[i].wheelIndex == WheelRaycast.WheelIndex.FrontRight)
                {
                    halfLanes.y = wheelRaycasts[i].GetHalfLane();
                    laneNumbers.y = wheelRaycasts[i].GetLaneNumber();
                }
                else if (wheelRaycasts[i].wheelIndex == WheelRaycast.WheelIndex.BackLeft)
                {
                    halfLanes.z = wheelRaycasts[i].GetHalfLane();
                    laneNumbers.z = wheelRaycasts[i].GetLaneNumber();
                }
                else if (wheelRaycasts[i].wheelIndex == WheelRaycast.WheelIndex.BackRight)
                {
                    halfLanes.w = wheelRaycasts[i].GetHalfLane();
                    laneNumbers.w = wheelRaycasts[i].GetLaneNumber();
                }
            }
        }

        void CheckIfOnRoad()
        {
            if (laneNumbers == notOnRoadVector)
                onRoad = -1;
            else if (laneNumbers.x == -9 || laneNumbers.y == -9 || laneNumbers.z == -9 || laneNumbers.w == -9)
                onRoad = 0;
            else
                onRoad = 1;
        }

        void CheckDirection()
        {
            if (laneNumbers.x == -1 || laneNumbers.y == -1 || laneNumbers.z == -1 || laneNumbers.w == -1)
            {
                onLane = true;
                wrongDirection = false;
                onCross = true;
            }
            else
                onCross = false;

            if (onCross)
                return;

            if (laneNumbers.x == laneNumbers.y && laneNumbers.x == laneNumbers.z && laneNumbers.x == laneNumbers.w)
            {
                if (halfLanes.x == 1 && halfLanes.y == 3 || halfLanes.x == 3 && halfLanes.y == 1 ||
                    halfLanes.z == 1 && halfLanes.w == 3 || halfLanes.z == 3 && halfLanes.w == 1)
                {
                    onLane = false;
                    wrongDirection = true;
                }
                else if (halfLanes.x == halfLanes.y && halfLanes.z == halfLanes.w)
                    onLane = false;
                else
                    onLane = true;
            }
            else
                onLane = false;

            if (onLane)
            {
                if (halfLanes.x == 1 && halfLanes.y == 0 && halfLanes.z == 1 && halfLanes.w == 0)
                    wrongDirection = false;
                else if (halfLanes.x == 3 && halfLanes.y == 2 && halfLanes.z == 3 && halfLanes.w == 2)
                    wrongDirection = false;
                else if (halfLanes.x == halfLanes.y && halfLanes.z == 1 && halfLanes.w == 0 ||
                    halfLanes.x == 1 && halfLanes.y == 0 && halfLanes.z == halfLanes.w)
                    wrongDirection = false;
                else if (halfLanes.x == 1 && halfLanes.z == 3 || halfLanes.x == 3 && halfLanes.z == 1)
                    wrongDirection = true;
                else
                    wrongDirection = true;
            }
        }

        void UpdateLaneStatus()
        {
            if (onRoad == 1)
            {
                if (!onCross)
                    laneStatus = "На дороге, на полосе ";
                else
                    laneStatus = "На дороге, на перекрёстке";
            }
            else if (onRoad == 0)
            {
                if (!onCross)
                {
                    laneStatus = "Частично выезд с дороги, на полосе ";

                    for (int i = 0; i < 4; i++)
                    {
                        if (laneNumbers.x != -9)
                        {
                            laneStatus += laneNumbers.x;
                            break;
                        }
                        else if (laneNumbers.y != -9)
                        {
                            laneStatus += laneNumbers.y;
                            break;
                        }
                        else if (laneNumbers.z != -9)
                        {
                            laneStatus += laneNumbers.z;
                            break;
                        }
                        else if (laneNumbers.w != -9)
                        {
                            laneStatus += laneNumbers.w;
                            break;
                        }
                    }
                }
                else
                    laneStatus = "Частично выезд с дороги, на перекрёстке ";
            }
            else if (onRoad == -1)
                laneStatus = "Не на дороге";

            if (onLane && !onCross)
                laneStatus += laneNumbers.x;

            if (onRoad == 1 && !onLane && !onCross)
            {
                if (laneNumbers.x != laneNumbers.y)
                    laneStatus += laneNumbers.x + " и полосе " + laneNumbers.y;
                else if (laneNumbers.z != laneNumbers.w)
                    laneStatus += laneNumbers.z + " и полосе " + laneNumbers.w;
            }
        }
    }
}
