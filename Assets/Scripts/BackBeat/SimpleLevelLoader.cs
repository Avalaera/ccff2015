using UnityEngine;
using System.Collections;

public class SimpleLevelLoader : MonoBehaviour {

	public int levelIndex = -1;

	public CanvasGroup cG;

	public float fadeTime = 0.5f;

	public void Load()
	{
		cG.interactable = false;

		StartCoroutine (Fade());
	}

	private IEnumerator Fade()
	{
		float time = 0.0f;

		while(time < fadeTime)
		{
			time += Time.deltaTime;

			if(time < fadeTime)
			{
				cG.alpha = Mathf.Lerp (cG.alpha, 0.0f, time/fadeTime);

				yield return null;
			}
		}

		cG.alpha = 0.0f;

		Application.LoadLevel (levelIndex);
	}
}
