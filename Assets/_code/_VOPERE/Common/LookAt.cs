using UnityEngine;

namespace Vopere.Common
{
    public class LookAt : MonoBehaviour
    {
        [SerializeField] Transform target;          // На что смотрим
        [SerializeField] Vector3 forwardAxis = Vector3.forward; // Какая ось смотрит на цель

        void LateUpdate()
        {
            if (target == null) return;

            // Вычисляем направление от объекта к цели
            Vector3 direction = target.position - transform.position;

            // Если цель прямо в центре объекта — ничего не делаем (избегаем ошибок)
            if (direction.sqrMagnitude < 0.0001f) return;

            // Создаём поворот, который смотрит вдоль выбранной оси
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);

            // Если нужная ось отличается от forward — корректируем
            if (forwardAxis != Vector3.forward)
            {
                Quaternion axisCorrection = Quaternion.FromToRotation(Vector3.forward, forwardAxis);
                lookRotation = lookRotation * axisCorrection;
            }

            // Жёстко применяем поворот
            transform.rotation = lookRotation;
        }
    }
}
