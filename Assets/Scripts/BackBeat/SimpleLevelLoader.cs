using UnityEngine;
using System.Collections;

public class SimpleLevelLoader : MonoBehaviour {

	public int levelIndex = -1;

	public void Load()
	{
		Application.LoadLevel(levelIndex);
	}
}
