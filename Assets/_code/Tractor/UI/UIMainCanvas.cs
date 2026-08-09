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

        TractorMain tractorMain;
        TractorGearbox tractorGearbox;

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
        }
    }
}
