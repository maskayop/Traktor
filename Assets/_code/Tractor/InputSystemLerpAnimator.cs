using UnityEngine;

namespace Tractor
{
    public class InputSystemLerpAnimator : MonoBehaviour
    {
        [SerializeField] string inputName;

        InputController inputController;
        CustomInput input;

        void Start()
        {
            inputController = InputController.Instance;
            input = inputController.GetInputByName(inputName);
        }
    }
}
