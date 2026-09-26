using TMPro;
using UnityEngine;

namespace Tractor.UI
{
    public class UITestInfoWindow : MonoBehaviour
    {
        [Header("Основное")]
        [SerializeField] TextMeshProUGUI speedText;
        [SerializeField] TextMeshProUGUI RPMText;
        [SerializeField] TextMeshProUGUI handbrakeText;

        [Header("Педали")]
        [SerializeField] TextMeshProUGUI throttleText;
        [SerializeField] TextMeshProUGUI brakeText;
        [SerializeField] TextMeshProUGUI clutchText;

        [Header("Коробка передач")]
        [SerializeField] TextMeshProUGUI gearboxLevelText;
        [SerializeField] TextMeshProUGUI gearboxRangeText;
        [SerializeField] TextMeshProUGUI gearboxGearText;

        [Header("Лампочки и звуки")]
        [SerializeField] TextMeshProUGUI turnLightsText;
        [SerializeField] TextMeshProUGUI alarmLightsText;
        [SerializeField] TextMeshProUGUI hornSignalText;

        [Header("Зажигание")]
        [SerializeField] TextMeshProUGUI massText;
        [SerializeField] TextMeshProUGUI starterText;
        [SerializeField] TextMeshProUGUI ignitionText;

        [Header("Определитель дороги")]
        [SerializeField] TextMeshProUGUI roadStatusText;
        [SerializeField] TextMeshProUGUI directionText;

        TractorController tractorMain;
        TractorGearbox tractorGearbox;
        TractorEngine tractorEngine;
        TractorLights tractorLights;
        TractorAudio tractorAudio;
        RoadDetector roadDetector;

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
            tractorMain = FindAnyObjectByType<TractorController>();

            if (!tractorMain)
                return;

            tractorGearbox = tractorMain.tractorGearbox;
            tractorEngine = tractorMain.tractorEngine;
            tractorLights = tractorMain.tractorLights;
            tractorAudio = tractorMain.tractorAudio;
            roadDetector = tractorMain.roadDetector;
        }

        void UpdateTexts()
        {
            if (!tractorGearbox || !tractorMain || !tractorEngine || !roadDetector)
                return;

            //Основное
            speedText.text = tractorMain.speed.ToString("F2");
            RPMText.text = tractorMain.RPM.ToString("F0");

            handbrakeText.text = tractorMain.isHandbrake.ToString();
            ColorBoolText(handbrakeText, tractorMain.isHandbrake);

            //Педали
            throttleText.text = tractorMain.throttle.ToString("F2");
            brakeText.text = tractorMain.brake.ToString("F2");
            clutchText.text = tractorMain.clutch.ToString("F2");

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

            //Лампочки и звуки
            if (tractorLights.leftTurnIsOn)
            {
                turnLightsText.text = "Left";
                turnLightsText.color = Color.orange;
            }
            else if (tractorLights.rightTurnIsOn)
            {
                turnLightsText.text = "Right";
                turnLightsText.color = Color.violet;
            }
            else
            {
                turnLightsText.text = "0";
                turnLightsText.color = Color.white;
            }

            alarmLightsText.text = tractorLights.alarmIsOn.ToString();
            ColorBoolText(alarmLightsText, tractorLights.alarmIsOn);

            hornSignalText.text = tractorAudio.hornIsPlaying.ToString();
            ColorBoolText(hornSignalText, tractorAudio.hornIsPlaying);

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
