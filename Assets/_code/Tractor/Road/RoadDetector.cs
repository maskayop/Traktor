using UnityEngine;

namespace Tractor
{
    public class RoadDetector : MonoBehaviour
    {
        [SerializeField] WheelRaycast[] wheelRaycasts = new WheelRaycast[4];

        public bool wrongDirection = false;
        public int onRoad = 1;
        public bool onLane = false;

        // x = FL, y = FR, z = BL, w = BR
        public Vector4 halfLanes = Vector4.zero;
        public Vector4 laneNumbers = Vector4.zero;
        public string[] materialNames = new string[4];

        Vector4 notOnRoadVector = new Vector4(-1, -1, -1, -1);
        Vector4 correctLaneVector = new Vector4(1, 0, 1, 0);
        Vector4 wrongLaneVector = new Vector4(0, 1, 0, 1);

        void Update()
        {
            Detect();
        }

        void Detect()
        {
            for (int i = 0; i < wheelRaycasts.Length; i++)
            {
                if (wheelRaycasts[i].wheelIndex == WheelRaycast.WheelIndex.FrontLeft)
                {
                    halfLanes.x = wheelRaycasts[i].GetHalfLane();
                    laneNumbers.x = wheelRaycasts[i].GetLaneNumber();
                    materialNames[0] = wheelRaycasts[i].GetMaterialName();
                }
                else if (wheelRaycasts[i].wheelIndex == WheelRaycast.WheelIndex.FrontRight)
                {
                    halfLanes.y = wheelRaycasts[i].GetHalfLane();
                    laneNumbers.y = wheelRaycasts[i].GetLaneNumber();
                    materialNames[1] = wheelRaycasts[i].GetMaterialName();
                }
                else if (wheelRaycasts[i].wheelIndex == WheelRaycast.WheelIndex.BackLeft)
                {
                    halfLanes.z = wheelRaycasts[i].GetHalfLane();
                    laneNumbers.z = wheelRaycasts[i].GetLaneNumber();
                    materialNames[2] = wheelRaycasts[i].GetMaterialName();
                }
                else if (wheelRaycasts[i].wheelIndex == WheelRaycast.WheelIndex.BackRight)
                {
                    halfLanes.w = wheelRaycasts[i].GetHalfLane();
                    laneNumbers.w = wheelRaycasts[i].GetLaneNumber();
                    materialNames[3] = wheelRaycasts[i].GetMaterialName();
                }
            }

            if (laneNumbers == notOnRoadVector)
                onRoad = -1;
            else if (laneNumbers.x == -1 || laneNumbers.y == -1 || laneNumbers.z == -1 || laneNumbers.w == -1)
                onRoad = 0;
            else
                onRoad = 1;

            if (laneNumbers.x == laneNumbers.y && laneNumbers.x == laneNumbers.z && laneNumbers.x == laneNumbers.w)
            {
                onLane = true;
            }
            else
                onLane = false;
        }
    }
}
