using UnityEngine;

namespace Tractor
{
    public class TractorTrailer : MonoBehaviour, ICheckable
    {
        [Header("Автоотцепка")]
        public float autoDetachAngle = 90f;

        RCCP_CarController carController;
        RCCP_TrailerController currentTrailer;

        public RCCP_TrailerController CurrentTrailer => currentTrailer;
        public bool HasTrailer() => currentTrailer != null;

        public bool HasVariable(string conditionName)
        {
            switch (conditionName)
            {
                case "hasTrailer":
                    return true;
                default:
                    return false;
            }
        }

        public float GetVariableValue(string conditionName)
        {
            switch (conditionName)
            {
                case "hasTrailer":
                    return HasTrailer() ? 1f : 0f;
                default:
                    Debug.LogWarning($"TractorTrailer: неизвестная переменная {conditionName}");
                    return 0f;
            }
        }

        public void Init(RCCP_CarController car)
        {
            carController = car;
        }

        void Update()
        {
            if (carController == null) return;

            CheckTrailer();
            CheckAutoDetach();
        }

        void CheckTrailer()
        {
            if (carController.OtherAddonsManager && carController.OtherAddonsManager.TrailAttacher)
                currentTrailer = carController.OtherAddonsManager.TrailAttacher.attachedTrailer;
            else
                currentTrailer = null;
        }

        void CheckAutoDetach()
        {
            if (currentTrailer == null) return;

            float angle = Vector3.Angle(carController.transform.forward, currentTrailer.transform.forward);

            if (angle > autoDetachAngle)
            {
                Debug.Log($"Угол {angle:F1}° превышен — отцепляем!");
                DetachTrailer();
            }
        }

        public void DetachTrailer()
        {
            currentTrailer?.DetachTrailer();
        }

        public void ForceDetachTrailer()
        {
            // Сначала пробуем через currentTrailer
            if (currentTrailer != null)
            {
                currentTrailer.DetachTrailer();
                currentTrailer = null;
                return;
            }

            // Если currentTrailer уже null, ищем через RCCP-менеджер
            if (carController != null &&
                carController.OtherAddonsManager != null &&
                carController.OtherAddonsManager.TrailAttacher != null)
            {
                var attached = carController.OtherAddonsManager.TrailAttacher.attachedTrailer;
                if (attached != null)
                {
                    attached.DetachTrailer();
                    carController.OtherAddonsManager.TrailAttacher.attachedTrailer = null;
                }
            }
        }
    }
}
