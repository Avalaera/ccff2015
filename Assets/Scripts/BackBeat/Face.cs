using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Face : MonoBehaviour {

	private Rigidbody2D rb;

	private DialButton curr = null;

	[Range(0.0f, 100.0f)]
	public float resetSpeed = 2.0f;

	private void Awake()
	{
		rb = gameObject.GetComponent<Rigidbody2D>();
	}

	public void SetDB(DialButton newDB)
	{
		curr = newDB;
	}

	public void RemDB()
	{
		curr = null;
	}
	
	// Update is called once per frame
	private void FixedUpdate ()
	{
		if(curr == null && rb.rotation > 0.0f)
		{
			rb.AddTorque (-Time.deltaTime * resetSpeed);
			//rb.MoveRotation (Time.deltaTime * resetSpeed);
		}
	}
}
