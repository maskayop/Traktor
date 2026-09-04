using UnityEngine;

namespace Tractor
{
    public class WheelRaycast : MonoBehaviour
    {
        public enum WheelIndex { FrontLeft, FrontRight, BackLeft, BackRight }

        [Header("Колесо")]
        public WheelIndex wheelIndex;

        [Header("Рейкаст")]
        [SerializeField] float raycastDistance = 5f;
        [SerializeField] LayerMask roadLayer;
        [SerializeField] string roadHalflaneMaterialProperty = "_IsOddHalfLane";
        [SerializeField] string roadLaneNumberMaterialProperty = "_LaneNumber";

        [Header("Визуализация")]
        [SerializeField] bool showDebugRay = true;

        [Header("Частота обновления")]
        [SerializeField] float updateDelay = 1.0f;

        int halfLane = 0;
        int laneNumber = 0;
        string materialName;

        float currentTime = 0;

        void Update()
        {
            currentTime -= Time.deltaTime;

            if (currentTime < 0)
            {
                DoRaycast();
                currentTime = updateDelay;
            }
        }

        void DoRaycast()
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, raycastDistance, roadLayer))
            {
                if (showDebugRay)
                    Debug.DrawLine(transform.position, hit.point, Color.green);

                Material hitMaterial = GetMaterialFromHit(hit);

                if (hitMaterial != null)
                {
                    if (hitMaterial.HasProperty(roadHalflaneMaterialProperty))
                        halfLane = hitMaterial.GetInt(roadHalflaneMaterialProperty);

                    if (hitMaterial.HasProperty(roadLaneNumberMaterialProperty))
                        laneNumber = hitMaterial.GetInt(roadLaneNumberMaterialProperty);

                    materialName = hitMaterial.name;
                }
                else
                {
                    laneNumber = -9;
                    materialName = "";
                }
            }
            else
            {
                if (showDebugRay)
                    Debug.DrawRay(transform.position, Vector3.down * raycastDistance, Color.red);

                laneNumber = -9;
                materialName = "";
            }
        }

        Material GetMaterialFromHit(RaycastHit hit)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();

            if (renderer == null)
                return null;

            if (renderer.sharedMaterials.Length == 1)
                return renderer.sharedMaterials[0];

            return GetMaterialByTriangle(hit, renderer);
        }

        Material GetMaterialByTriangle(RaycastHit hit, Renderer renderer)
        {
            MeshFilter meshFilter = hit.collider.GetComponent<MeshFilter>();

            if (meshFilter == null || meshFilter.mesh == null)
                return renderer.sharedMaterials[0];

            Mesh mesh = meshFilter.mesh;

            int triangleIndex = hit.triangleIndex;
            int subMeshIndex = GetSubMeshIndex(mesh, triangleIndex);

            Material[] materials = renderer.sharedMaterials;

            if (subMeshIndex < materials.Length)
                return materials[subMeshIndex];

            return materials[0];
        }

        int GetSubMeshIndex(Mesh mesh, int triangleIndex)
        {
            int triangleCount = 0;

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] subMeshTriangles = mesh.GetTriangles(i);
                triangleCount += subMeshTriangles.Length / 3;

                if (triangleIndex < triangleCount)
                    return i;
            }

            return 0;
        }

        public int GetHalfLane()
        {
            return halfLane;
        }

        public int GetLaneNumber()
        {
            return laneNumber;
        }

        public string GetMaterialName()
        {
            return materialName;
        }
    }
}
