using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using System;

public class DialButton : Button, IDragHandler {

	public Rigidbody2D rb;

	private Quaternion originalRotation;

	private float startAngle = 0.0f;

	public DialButtonEvent onPointerDown, onPointerUp;

	private Sprite sprite = null;

	private bool dragging = false;

	private Collider2D coll = null;

	private Board board = null;

	protected override void Start()
	{
		base.Start ();

		originalRotation = rb.transform.rotation;

		coll = GetComponent<Collider2D>();

		board = Board.Instance;
	
		Sprite[] sprites = board.cellPrefab.spriteChoices.ToArray();

		int index = UnityEngine.Random.Range(0, sprites.Length);

		sprite = sprites[index];

		image.color = board.cellPrefab.spriteTints[index];
	}

	public override void OnPointerDown(PointerEventData data)
	{
		originalRotation = rb.transform.rotation;

		Vector3 screenPos = Camera.main.WorldToScreenPoint(rb.transform.position);
		Vector3 vector = (Vector3)(data.position) - screenPos;

		startAngle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

		base.OnPointerDown (data);

		onPointerDown.Invoke (this);

		dragging = true;
	}

	public void OnDrag(PointerEventData data)
	{
		if(interactable)
		{
			Vector3 screenPos = Camera.main.WorldToScreenPoint(rb.transform.position);
			Vector3 vector = (Vector3)(data.position) - screenPos;

			float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

			Quaternion newRotation = Quaternion.AngleAxis(angle - startAngle , rb.transform.forward);
			newRotation.y = 0; //see comment from above 
			newRotation.eulerAngles = new Vector3(0,0,newRotation.eulerAngles.z);

			rb.MoveRotation ((originalRotation * newRotation).eulerAngles.z);
		}
	}

	public override void OnPointerUp(PointerEventData data)
	{
		base.OnPointerUp (data);

		onPointerUp.Invoke(this);

		dragging = false;
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		if(other.tag.Equals ("Hook") && dragging)
		{
			interactable = false;

			coll.enabled = false;

			image.color = Color.white;

			onPointerUp.Invoke(this);

			board.SpriteMatchScore(sprite);
		}
	}
}

[Serializable]
public class DialButtonEvent : UnityEvent<DialButton> {}
