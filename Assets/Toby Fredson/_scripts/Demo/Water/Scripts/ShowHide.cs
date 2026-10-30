using UnityEngine;

namespace TobyFredson
{
	public class ShowHide : MonoBehaviour
	{
		public void ShowIt(GameObject obj)
		{
			if (obj != null)
				obj.SetActive(true);
		}

		public void HideIt(GameObject obj)
		{
			if (obj != null)
				obj.SetActive(false);
		}
	}
}