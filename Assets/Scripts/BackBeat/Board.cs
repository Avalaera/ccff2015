using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SynchronizerData;
using System;

public class Board : MonoBehaviour {

	[Range(1, 10)]
	public int rows, columns;

	public Cell cellPrefab;

	public CriticalCell ccPrefab;

	private List<RowData> rowList, movableRowList;

	[HideInInspector][SerializeField]
	public CriticalCellDictionary ccDict;

	private Vector2[] keys;

	private static Board instance = null;

	public int MatchRequirement = 3;

	public bool autoClearOnMatch = false;

	public int numOfBeatsToSkipAfterMatch = 0;

	private int beatsToSkip = 0;

	public bool rowMoveOnBeat = false;

	public RowPickerData picker;

	public enum ClearStyles
	{
		MatchesOnly,
		AllCriticalSpaces
	};

	public enum SpecialClearStyles
	{
		None,
		ClearAllMatchingColors
	};

	public ClearStyles clearStyle;

	public SpecialClearStyles specialClearStyle;

	private void Awake()
	{
		if(instance == null)
		{
			instance = this;

			rowList = new List<RowData>();

			PerformRowAction (MakeRow);

			movableRowList = new List<RowData>(rowList);

			keys = new Vector2[ccDict.Keys.Count];
			ccDict.Keys.CopyTo (keys, 0);

			DoWithKeys (MakeCriticalCellWithIndex);
		}
		else
		{
			this.enabled = false;

			Debug.LogWarningFormat ("Duplicate Board instance, {0] disabled.", name);
		}
	}

	private void MakeRow(int x)
	{
		rowList.Add (new RowData(transform, new GameObject().transform, cellPrefab.Size));

		rowList[x].MoveForward (cellPrefab);

		rowList[x].onMoveBackward.AddListener (AddToMovableRowList);
	}

	private void MakeCriticalCellWithIndex(int i)
	{
		MakeCriticalCell (keys[i], ccDict[keys[i]]);
	}

	private void MakeCriticalCell(Vector2 pos, CriticalCellData d)
	{
		CriticalCell c = Instantiate<CriticalCell>(ccPrefab);

		c.transform.parent = transform.parent;

		Vector2 scale = cellPrefab.Size * 0.5f;

		c.transform.localScale = new Vector3(scale.x, scale.y, c.transform.localScale.z);

		float adjustedX = pos.x - transform.position.x;
		float adjustedY = transform.position.y - pos.y;

		c.name = string.Format ("_CriticalCell[{0},{1}]", adjustedX, adjustedY);

		c.transform.position = new Vector3(transform.position.x + (adjustedX * cellPrefab.Size.x), transform.position.y - (cellPrefab.Size.y * adjustedY), transform.position.z);

		c.matchReq = MatchRequirement;

		c.instantClear = autoClearOnMatch;

		for(int i = 0; i < d.NeighborPositions.Count ; i++)
		{
			d.AddNeighbor(ccDict[d.NeighborPositions[i]]);
		}

		c.SetCriticalCellData(d);
	}

	private void PerformColumnAction(Action<int> a)
	{
		for(int y = 0; y < columns; y++)
		{
			a.Invoke (y);
		}
	}

	private void PerformRowAction(Action<int> a)
	{
		for(int x = 0; x < rows; x++)
		{
			a.Invoke (x);
		}
	}

	public void Perform2DArrayAction(Action<int, int> a)
	{
		for(int y = 0; y < columns; y++)
		{
			for(int x = 0; x < rows; x++)
			{
				a.Invoke (x, y);
			}
		}
	}

	private void DoWithKeys(Action<int> a)
	{
		for(int i = 0; i < keys.Length; i++)
		{
			a.Invoke (i);
		}
	}

	private void OnDrawGizmos()
	{
		if(cellPrefab != null)
		{
			Perform2DArrayAction (DrawWireCube);
		}	
	}

	private void DrawWireCube(int x, int y)
	{
		if(x == 0)
		{
			Gizmos.color = Color.green;
		}
		else if (x == rows - 1)
		{
			Gizmos.color = Color.red;
		}
		else
		{
			Gizmos.color = Color.white;
		}

		Gizmos.DrawWireCube(new Vector3(transform.position.x + (x * cellPrefab.Size.x), transform.position.y - (y * cellPrefab.Size.y), 0), cellPrefab.Size);
	}

	public void DoOnBeat(BeatType mask)
	{
		if(movableRowList.Count > 0 && mask.Equals (BeatType.OnBeat))
		{
			if(beatsToSkip > 0)
			{
				beatsToSkip--;
			}
			else
			{
				int y = picker.PickRow<RowData> (rowList, movableRowList);
					
				movableRowList[y].MoveForward(cellPrefab);

				if(movableRowList[y].CellCount >= columns && movableRowList[y].Trans.localPosition.x >= columns * cellPrefab.Size.x)
				{
					movableRowList.Remove (movableRowList[y]);
				}
			}
		}
	}

	private void AddToMovableRowList(RowData r)
	{
		if(!movableRowList.Contains (r))
		{
			movableRowList.Add (r);
		}
	}

	public void Match(List<Cell> matches)
	{
		if(matches.Count >= MatchRequirement || autoClearOnMatch)
		{
			switch(clearStyle)
			{
				case ClearStyles.AllCriticalSpaces:
					DoWithKeys (ScoreMatchesA);
					break;
				case ClearStyles.MatchesOnly:
					PerformRowAction (ScoreMatchesB);
					break;
			}

			beatsToSkip = numOfBeatsToSkipAfterMatch;
		}
	}

	private void ScoreMatchesA(int i)
	{
		Cell c = ccDict[keys[i]].cell;

		if(c != null)
		{
			c.remove = true;

			HandleScoringCell(c);

			c = null;
		}
	}

	private void ScoreMatchesB(int x)
	{
		RowData r = rowList[x];

		for(int i = 0; i < r.Cells.Count; i++)
		{
			HandleScoringCell(r.Cells[i], r);
		}
	}

	private void HandleScoringCell(Cell c, RowData r = null, bool doSpecial = true)
	{
		List<Color> colorInfo = new List<Color>();

		if(c != null)
		{
			if(r == null)
			{
				r = rowList[c.transform.parent.GetSiblingIndex()];
			}

			if(c.matched || c.remove)
			{
				if(c.matched && !colorInfo.Contains (c.DefaultGraphicTD.color))
				{
					colorInfo.Add (c.DefaultGraphicTD.color);
				}

				r.RemoveCell (r.Cells.IndexOf (c));
				
				c.Destroy();
			}
		}

		if(doSpecial)
		{
			switch(specialClearStyle)
			{
				case SpecialClearStyles.ClearAllMatchingColors:
					ClearMatchingColors(colorInfo);
					break;
			};
		}
	}
	
	public void ClickScore()
	{
		CriticalCellData data = null;

		for(int i = 0; i < keys.Length; i++)
		{
			data = ccDict[keys[i]];

			if(data.CurrentMatchList.Count >= MatchRequirement)
			{
				i = keys.Length;

				Match (data.CurrentMatchList);
			}
		}
	}

	private void ClearMatchingColors(List<Color> info)
	{
		for(int i = 0; i < rows; i++)
		{
			for(int j = 0; j < rowList[i].Cells.Count; j++)
			{
				if(info.Contains (rowList[i].Cells[j].DefaultGraphicTD.color))
				{
					rowList[i].Cells[j].matched = true;

					HandleScoringCell (rowList[i].Cells[j], rowList[i], false);
				}
			}
		}
	}

	public static Board Instance
	{
		get
		{
			return instance;
		}
	}

	public List<RowData> RowList
	{
		get
		{
			return rowList;
		}
	}

	public CriticalCellDictionary CCDict
	{
		get
		{
			return ccDict;
		}
	}
}
