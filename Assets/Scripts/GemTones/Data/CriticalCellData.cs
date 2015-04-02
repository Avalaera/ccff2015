using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class CriticalCellDictionary : SerializableDictionary<Vector2, CriticalCellData> { }

[Serializable]
public class CriticalCellData
{
	[SerializeField]
	private List<Vector2> neighborPositions = new List<Vector2>();

	[NonSerialized]
	private List<CriticalCellData> neighbors = new List<CriticalCellData>();

	[NonSerialized]
	public Cell cell = null;

	[NonSerialized]
	private List<Cell> currentMatchList = new List<Cell>();

	public void AddNeighbor(CriticalCellData newNeighbor)
	{
		if(!neighbors.Contains (newNeighbor))
		{
			neighbors.Add (newNeighbor);
		}
	}

	public List<Cell> FindMatches()
	{
		currentMatchList = new List<Cell>();

		Match (ref currentMatchList);

		return currentMatchList;
	}

	private void Match(ref List<Cell> prev)
	{
		if(cell != null)
		{
			prev.Add (cell);
			
			for(int i = 0; i < neighbors.Count; i++)
			{
				if(neighbors[i].cell != null && !prev.Contains (neighbors[i].cell) && neighbors[i].cell.Match (cell))
				{
					neighbors[i].Match (ref prev);
				}
			}
		}
	}

	public void DoOnNeighbors(Action<CriticalCellData> a)
	{
		for(int i = 0; i < neighbors.Count; i++)
		{
			if(neighbors[i].cell != null)
			{
				a.Invoke (neighbors[i]);
			}
		}
	}
	
	public List<Vector2> NeighborPositions
	{
		get
		{
			return neighborPositions;
		}
	}

	public List<Cell> CurrentMatchList
	{
		get
		{
			return currentMatchList;
		}
	}
}
