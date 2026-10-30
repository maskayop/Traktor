using UnityEngine;
using UnityEngine.InputSystem;   // ← Required for new Input System

namespace TobyFredson
{
	public class MenuManager : MonoBehaviour
	{
		// Changed to GameObject because only the game object of the menu needs to be accessed
		public GameObject optionsMenu;
		public GameObject forest;
		public GameObject hills;

		void Update()
		{
			// Toggle Options Menu with H
			if (Keyboard.current.hKey.wasPressedThisFrame)
			{
				bool isActive = optionsMenu.activeSelf;
				optionsMenu.SetActive(!isActive);
			}

			// Toggle Forest with Alpha1 (number key 1)
			if (Keyboard.current.digit1Key.wasPressedThisFrame)
			{
				bool isActive = forest.activeSelf;
				forest.SetActive(!isActive);
			}

			// Toggle Hills with Alpha2 (number key 2)
			if (Keyboard.current.digit2Key.wasPressedThisFrame)
			{
				bool isActive = hills.activeSelf;
				hills.SetActive(!isActive);
			}
		}
	}
}