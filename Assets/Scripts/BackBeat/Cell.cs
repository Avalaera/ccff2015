using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using SynchronizerData;
using UnityEngine.UI;
using System;

public class Cell : Button {

	public class TransData
	{
		public Mesh mesh;

		public Color color;

		public Vector3 scale;

		public Quaternion rotation;

		public TransData(Vector3 s, Quaternion rot, Mesh m = null, Color c = new Color())
		{
			mesh = m;

			color = c;

			scale = s;
		
			rotation = rot;
		}
	}

	[Serializable]
	public struct RendererStates
	{
		[SerializeField]
		public Material highlight, normal;
	}

	public Transform graphic;

	public MeshRenderer bg;

	public List<Mesh> shapes;

	public List<Color> colorChoices;

	public RendererStates states;

	[HideInInspector]
	public CriticalCell ccCell;

	private TransData defaultGraphicTD;

	private Animator anim;

	private BeatObserver beatObs;

	private BeatType bMask;

	[HideInInspector]
	public bool matched = false;

	[HideInInspector]
	public bool clickOnBeat = false;

	[HideInInspector]
	public bool remove = false;

	private Collider coll;

	protected override void Awake()
	{
		base.Awake ();

		defaultGraphicTD = new TransData(graphic.transform.localScale, graphic.transform.rotation);

		beatObs = GameObject.FindGameObjectWithTag ("OnBeatObs").GetComponent<BeatObserver>();
		beatObs.beatEvent.AddListener (DoOnBeat);

		anim = GetComponentInChildren<Animator>();

		coll = GetComponent<Collider>();

		Randomize();
	}

	private void Randomize()
	{
		defaultGraphicTD.mesh = shapes[UnityEngine.Random.Range(0, shapes.Count)];
		defaultGraphicTD.color = colorChoices[UnityEngine.Random.Range(0, colorChoices.Count)];

		graphic.GetComponent<MeshFilter>().mesh = defaultGraphicTD.mesh;
		graphic.GetComponent<MeshRenderer>().material.color = defaultGraphicTD.color;
	}

	public override void OnPointerClick (UnityEngine.EventSystems.PointerEventData eventData)
	{
		if(!clickOnBeat || bMask.Equals(BeatType.OnBeat))
		{
			onClick.Invoke ();
		}
	}

	public bool Match(Cell otherCell)
	{
		return defaultGraphicTD.color.Equals (otherCell.defaultGraphicTD.color) && defaultGraphicTD.mesh.Equals (otherCell.defaultGraphicTD.mesh);
	}

	public void DoOnBeat(BeatType mask)
	{
		bMask = mask;

		if(matched && mask.Equals (BeatType.OnBeat))
		{
			anim.SetTrigger ("OnBeatTrigger");
		}
	}

	public void Destroy()
	{
		onClick.RemoveAllListeners();

		beatObs.beatEvent.RemoveListener(DoOnBeat);

		anim.StopPlayback();

		ClearCriticalCell ();

		gameObject.SetActive (false);
	}

	public void ClearCriticalCell()
	{
		if(ccCell != null)
		{
			ccCell.OnTriggerExit (coll);
		}
	}

	private void FixedUpdate()
	{
		if(ccCell != null)
		{
			graphic.rotation = ccCell.transform.rotation;

			bg.material = states.highlight;
		}
		else
		{
			if(graphic.rotation != defaultGraphicTD.rotation)
			{
				graphic.rotation = Quaternion.Lerp (graphic.rotation, defaultGraphicTD.rotation, 10.0f * Time.deltaTime);

				bg.material = states.normal;
			}
		}
	}

	public Collider Coll
	{
		get
		{
			return coll;
		}
	}

	public Vector2 Size
	{
		get
		{
			return new Vector2(transform.localScale.x, transform.localScale.y);
		}
	}

	public TransData DefaultGraphicTD
	{
		get
		{
			return defaultGraphicTD;
		}
	}
}
