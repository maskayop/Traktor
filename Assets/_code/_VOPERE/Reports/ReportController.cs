using UnityEngine;

namespace Vopere
{
    public class ReportController : MonoBehaviour
    {
        public static ReportController Instance;

        [SerializeField] ReportSaver reportSaver;
        [SerializeField] ReportLoader reportLoader;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create ReportController");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            Init();
        }

        public void Init() { }

        public void LoadAllReports()
        {
            reportLoader.LoadAll();
        }

        public void SaveReport()
        {
            reportSaver.Save();
        }

        public void SetReport(Report r)
        {
            reportSaver.report = r;
        }
    }
}
