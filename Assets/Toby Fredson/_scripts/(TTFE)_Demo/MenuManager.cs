using UnityEngine;
using UnityEngine.InputSystem;   // ← Important: Add this

namespace TobyFredson.SceneScripts
{
	[AddComponentMenu("Toby Fredson/The Toby Foliage Engine/Scene/Menu Manager")]
	public class MenuManager : MonoBehaviour
	{
		[SerializeField] private GameObject optionsMenu;
		[SerializeField] private GameObject forest;
		[SerializeField] private GameObject hills;

		void Update()
		{
			// Toggle options menu with H key
			if (Keyboard.current.hKey.wasPressedThisFrame)
			{
				if (optionsMenu != null)
					optionsMenu.SetActive(!optionsMenu.activeSelf);
			}

			// Toggle forest with 1 key (Alpha1 / Digit1)
			if (Keyboard.current.digit1Key.wasPressedThisFrame)
			{
				if (forest != null)
					forest.SetActive(!forest.activeSelf);
			}

			// Toggle hills with 2 key (Alpha2 / Digit2)
			if (Keyboard.current.digit2Key.wasPressedThisFrame)
			{
				if (hills != null)
					hills.SetActive(!hills.activeSelf);
			}
		}
	}
}