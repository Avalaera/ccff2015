using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SynchronizerData;
using UnityEngine.UI;
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

	private Sprite specificMatchSprite;

	public int scoreMult = 10, biggestMult = 0;

	public Text scoreText;

	private SimpleCanvasGroupFade endFade = null;

	private AudioSource endAudio = null;

	[HideInInspector]
	public int buttons = 0;

	public enum ClearStyles
	{
		MatchesOnly,
		AllCriticalSpaces,
		SpecificMatchOnly
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

			PerformColumnAction (MakeRow);

			movableRowList = new List<RowData>(rowList);

			keys = new Vector2[ccDict.Keys.Count];
			ccDict.Keys.CopyTo (keys, 0);

			DoWithKeys (MakeCriticalCellWithIndex);

			endFade = GameObject.FindGameObjectWithTag ("Score").GetComponent<SimpleCanvasGroupFade>();

			endAudio = GameObject.FindGameObjectWithTag ("Score").GetComponent<AudioSource>();
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
		if(mask.Equals (BeatType.OnBeat))
		{
			if(movableRowList.Count > 0 && buttons > 0)
			{
				if(beatsToSkip > 0)
				{
					beatsToSkip--;
				}
				else
				{
					int y = picker.PickRow<RowData> (rowList, movableRowList);
						
					movableRowList[y].MoveForward(cellPrefab);

					if(movableRowList[y].CellCount >= rows && movableRowList[y].Trans.localPosition.x >= rows * cellPrefab.Size.x)
					{
						movableRowList.Remove (movableRowList[y]);
					}
				}
			}
			else if(buttons > -1)
			{
				buttons--;

				endFade.FadeIn ();

				endAudio.Play ();
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
					PerformColumnAction (ScoreMatchesB);
					break;
				case ClearStyles.SpecificMatchOnly:
					PerformColumnAction (ScoreMatchesC);
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

	private void ScoreMatchesC(int x)
	{
		RowData r = rowList[x];
		
		for(int i = 0; i < r.Cells.Count; i++)
		{
			if(r.Cells[i].Match (specificMatchSprite))
			{
				HandleScoringCell(r.Cells[i], r);
			}
		}
	}

	private void HandleScoringCell(Cell c, RowData r = null, bool doSpecial = true)
	{
		List<Sprite> spriteInfo = new List<Sprite>();

		if(c != null)
		{
			if(r == null)
			{
				r = rowList[c.transform.parent.GetSiblingIndex()];
			}

			if(c.matched || c.remove)
			{
				if(c.matched && !spriteInfo.Contains (c.DefaultGraphicTD.sprite))
				{
					spriteInfo.Add (c.DefaultGraphicTD.sprite);
				}

				if(c.remove)
				{
					c.ccCell.PlayFail ();
				}
				else
				{
					c.ccCell.PlayScore (c.spriteTints[c.spriteChoices.IndexOf (c.DefaultGraphicTD.sprite)]);
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
					ClearMatchingColors(spriteInfo);
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

	public void SpriteMatchScore(Sprite s)
	{
		CriticalCellData data = null;

		specificMatchSprite = s;

		List<Cell> biggestList = new List<Cell>();

		int spriteMatches = 0, soloMatches = 0, keyIndex = -1;

		for(int i = 0; i < keys.Length; i++)
		{
			data = ccDict[keys[i]];
			
			if(data.cell != null && data.cell.Match (s))
			{
				if(data.CurrentMatchList.Count >= MatchRequirement)
				{
					if(data.CurrentMatchList.Count > biggestList.Count)
					{
						biggestList = data.CurrentMatchList;

						keyIndex = i;
					}

					spriteMatches++;
				}
				else
				{
					soloMatches++;
				}
			}
		}

		if(keyIndex > -1)
		{
			int score = int.Parse (scoreText.text) + (biggestList.Count * scoreMult * biggestMult) + ((spriteMatches - biggestList.Count) * scoreMult);

			scoreText.text = score.ToString ();

			List<Sprite> l = new List<Sprite>();
			l.Add (s);
			
			Match (ccDict[keys[keyIndex]].CurrentMatchList);
		}
	}

	private void ClearMatchingColors(List<Sprite> info)
	{
		for(int i = 0; i < columns; i++)
		{
			for(int j = 0; j < rowList[i].Cells.Count; j++)
			{
				if(info.Contains (rowList[i].Cells[j].DefaultGraphicTD.sprite))
				{
					if(!rowList[i].Cells[j].matched)
					{
						rowList[i].Cells[j].remove = true;
					}

					HandleScoringCell (rowList[i].Cells[j], rowList[i], false);

					j--;
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
