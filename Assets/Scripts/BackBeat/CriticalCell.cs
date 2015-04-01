using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CriticalCell : MonoBehaviour {

	private Rigidbody rb;

	public float torqueMagnitude = 10.0f;

	[HideInInspector]
	public int matchReq;

	[HideInInspector]
	public bool instantClear = false;

	private CriticalCellData data = null;

	private ParticleSystem pSys;

	private void Awake()
	{
		rb = gameObject.GetComponent<Rigidbody>();

		rb.AddTorque(transform.forward * torqueMagnitude);

		pSys = GetComponentInChildren<ParticleSystem>();
	}

	public void SetCriticalCellData(CriticalCellData d)
	{
		data = d;
	}

	public void OnTriggerEnter(Collider other)
	{
		if(other.tag.Equals("Cell"))
		{
			Cell c = other.GetComponent<Cell>();

			if(data.cell != c)
			{
				data.cell = c;

				c.ccCell = this;

				CheckForMatches (data);
			}
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if(other.tag.Equals("Cell"))
		{
			Cell c = other.GetComponent<Cell>();

			if(data.cell == c)
			{
				data.cell.matched = false;

				data.cell = null;

				c.ccCell = null;

				data.DoOnNeighbors(CheckForMatches);
			}
		}
	}

	private static void CheckForMatches(CriticalCellData data)
	{
		List<Cell> matches = data.FindMatches();

		if(matches.Count >= Board.Instance.MatchRequirement)
		{
			for(int i = 0; i < matches.Count; i++)
			{
				matches[i].matched = true;
			}

			if(Board.Instance.autoClearOnMatch)
			{
				Board.Instance.Match (matches);
			}
		}
		else
		{
			data.cell.matched = false;
		}
	}

	public void Play(Color c)
	{
		pSys.startColor = c;

		pSys.Play ();
	}
}
