using UnityEngine;
using System.Collections;

public class SimpleCanvasGroupFade : MonoBehaviour {
	
	public CanvasGroup cG;
	
	public float fadeTime = 0.5f;
	
	public void FadeIn()
	{
		StartCoroutine (Fade(1.0f));
	}

	public void FadeOut()
	{
		StartCoroutine (Fade(0.0f, false));
	}

	private IEnumerator Fade(float dest, bool fadeIn = true)
	{
		cG.interactable = cG.blocksRaycasts = false;

		float time = 0.0f;
		
		while(time < fadeTime)
		{
			time += Time.deltaTime;
			
			if(time < fadeTime)
			{
				cG.alpha = Mathf.Lerp (cG.alpha, dest, time/fadeTime);
				
				yield return null;
			}
		}
		
		cG.alpha = dest;

		cG.interactable = cG.blocksRaycasts = fadeIn;
	}
}
