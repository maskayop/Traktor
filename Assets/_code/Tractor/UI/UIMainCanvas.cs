using TMPro;
using UnityEngine;

namespace Tractor
{
    public class UIMainCanvas : MonoBehaviour
    {
        public static UIMainCanvas Instance;

        [Header("Трактор")]
        [SerializeField] TextMeshProUGUI speedText;

        [Header("Коробка передач")]
        [SerializeField] TextMeshProUGUI gearboxLevelText;
        [SerializeField] TextMeshProUGUI gearboxRangeText;
        [SerializeField] TextMeshProUGUI gearboxGearText;

        [Header("Определитель дороги")]
        [SerializeField] TextMeshProUGUI roadStatusText;
        [SerializeField] TextMeshProUGUI directionText;

        TractorMain tractorMain;
        TractorGearbox tractorGearbox;
        RoadDetector roadDetector;

        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Cannot create UIMainCanvas");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            Init();
        }

        void Update()
        {
            UpdateTexts();
        }

        public void Init()
        {
            tractorGearbox = FindAnyObjectByType<TractorGearbox>();
            tractorMain = FindAnyObjectByType<TractorMain>();
            roadDetector = FindAnyObjectByType<RoadDetector>();
        }

        void UpdateTexts()
        {
            if (!tractorGearbox || !tractorMain)
                return;

            speedText.text = tractorMain.speed.ToString("F2");

            if (tractorGearbox.isGearLevel1)
                gearboxLevelText.text = "1";
            else
                gearboxLevelText.text = "2";

            gearboxRangeText.text = tractorGearbox.currentRange.ToString();
            gearboxGearText.text = tractorGearbox.currentGear.ToString();

            roadStatusText.text = roadDetector.laneStatus;

            if (roadDetector.wrongDirection)
            {
                directionText.color = Color.red;
                directionText.text = "-1";
            }
            else
            {
                directionText.color = Color.green;
                directionText.text = "1";
            }
        }
    }
}
