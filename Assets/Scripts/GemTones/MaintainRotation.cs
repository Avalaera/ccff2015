using UnityEngine;
using System.Collections;

public class MaintainRotation : MonoBehaviour {

	private void LateUpdate()
	{
		transform.rotation = Quaternion.identity;
	}
}
