using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class RowPickerData
{
	public enum Styles
	{
		Rand,
		Cycle
	}

	[SerializeField]
	public Styles style;

	private int lastCycle = -1;

	public int PickRow<T>(List<T> total, List<T> available)
	{
		int pick = -1;

		switch(style)
		{
			case Styles.Rand:
				pick = UnityEngine.Random.Range(0, available.Count);
				break;
			case Styles.Cycle:
				Cycle<T>(total, available, ref pick);
				break;
		}

		lastCycle = total.IndexOf (available[pick]);

		return pick;
	}

	private void Cycle<T>(List<T> total, List<T> available, ref int pick)
	{
		lastCycle++;

		List<T> c = new List<T>();
		c.AddRange (total.GetRange(lastCycle, total.Count - lastCycle));
		c.AddRange (total.GetRange(0, lastCycle));

		for(int i = 0; i < c.Count && pick == -1; i++)
		{
			pick = available.IndexOf (c[i]);
		}
	}
}
