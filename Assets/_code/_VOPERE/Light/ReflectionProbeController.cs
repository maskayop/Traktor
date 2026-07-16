using UnityEngine;
using UnityEngine.Rendering;

namespace Vopere.Common
{
    [RequireComponent(typeof(ReflectionProbe))]
    public class ReflectionProbeController : MonoBehaviour
    {
        [SerializeField] float delay;

        ReflectionProbe reflectionProbe;
        float currentTime = 0;

        void Start()
        {
            reflectionProbe = GetComponent<ReflectionProbe>();
            reflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
        }

        void Update()
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                reflectionProbe.refreshMode = ReflectionProbeRefreshMode.EveryFrame;
                currentTime = delay;
            }
            else
                reflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
        }
    }
}
