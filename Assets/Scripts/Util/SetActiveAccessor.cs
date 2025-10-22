using UnityEngine;

#nullable enable

namespace CentaursBoardGame
{
	public class SetActiveAccessor : MonoBehaviour
	{
		//This script is used for animation events
	
		public void SetGameObjectActive()
		{
			gameObject.SetActive(true);
		}

		public void SetGameObjectNotActive()
		{
			gameObject.SetActive(false);
		}
	}
}