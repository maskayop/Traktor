using UnityEngine;

namespace Tractor
{
    public class InputSystemLerpAnimator : MonoBehaviour
    {
        [SerializeField] string inputName;

        [Header("Positions")]
        [SerializeField] Vector3 startPosition;
        [SerializeField] Vector3 endPosition;

        [Header("Rotations")]
        [SerializeField] Vector3 startRotation;
        [SerializeField] Vector3 endRotation;

        InputController inputController;
        public CustomInput input;

        void Start()
        {
            inputController = InputController.Instance;
            input = inputController?.GetInputByName(inputName);
        }

        void Update()
        {
            Animate();
        }

        void Animate()
        {
            if (input == null)
                return;

            if (input.InputValue >= 0)
            {
                transform.localPosition = Vector3.Lerp(startPosition, endPosition, input.InputValue);
                transform.localRotation = Quaternion.Euler(Vector3.Lerp(startRotation, endRotation, input.InputValue));
            }
            else
            {
                transform.localPosition = Vector3.Lerp(startPosition, -endPosition, -input.InputValue);
                transform.localRotation = Quaternion.Euler(Vector3.Lerp(startRotation, -endRotation, -input.InputValue));
            }
        }
    }
}
