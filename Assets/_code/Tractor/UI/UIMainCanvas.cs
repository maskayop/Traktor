using TMPro;
using UnityEngine;

namespace Tractor
{
    public class UIMainCanvas : MonoBehaviour
    {
        public static UIMainCanvas Instance;

        [Header("Основное")]
        [SerializeField] TextMeshProUGUI speedText;

        [Header("Коробка передач")]
        [SerializeField] TextMeshProUGUI gearboxLevelText;
        [SerializeField] TextMeshProUGUI gearboxRangeText;
        [SerializeField] TextMeshProUGUI gearboxGearText;

        [Header("Зажигание")]
        [SerializeField] TextMeshProUGUI massText;
        [SerializeField] TextMeshProUGUI starterText;
        [SerializeField] TextMeshProUGUI ignitionText;

        [Header("Определитель дороги")]
        [SerializeField] TextMeshProUGUI roadStatusText;
        [SerializeField] TextMeshProUGUI directionText;

        TractorMain tractorMain;
        TractorGearbox tractorGearbox;
        TractorEngine tractorEngine;
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
            tractorMain = FindAnyObjectByType<TractorMain>();
            tractorGearbox = tractorMain.tractorGearbox;
            tractorEngine = tractorMain.tractorEngine;

            roadDetector = FindAnyObjectByType<RoadDetector>();
        }

        void UpdateTexts()
        {
            if (!tractorGearbox || !tractorMain || !tractorEngine)
                return;

            //Основное
            speedText.text = tractorMain.speed.ToString("F2");

            //Коробка передач
            if (tractorGearbox.isGearLevel1)
                gearboxLevelText.text = "1";
            else
                gearboxLevelText.text = "2";

            gearboxRangeText.text = tractorGearbox.currentRange.ToString();

            if (tractorGearbox.currentGear == 0)
                gearboxGearText.text = "N";
            else
                gearboxGearText.text = tractorGearbox.currentGear.ToString();

            //Зажигание
            massText.text = tractorEngine.Mass.ToString();
            ColorBoolText(massText, tractorEngine.Mass);

            starterText.text = tractorEngine.Starter.ToString();
            ColorBoolText(starterText, tractorEngine.Starter);

            ignitionText.text = tractorEngine.Ignition.ToString();
            ColorBoolText(ignitionText, tractorEngine.Ignition);

            //Определитель дороги
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

        void ColorBoolText(TextMeshProUGUI INtext, bool value)
        {
            if (value)
                INtext.color = Color.green;
            else
                INtext.color = Color.red;
        }
    }
}
