using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

public class RowData
{
	private List<Cell> cells = new List<Cell>();
	
	private Transform transform;
	
	private Vector2 dimensions;
	
	public RowBackEvent onMoveBackward = new RowBackEvent();

	private int offset = 0;

	public RowData(Transform parent, Transform r, Vector2 d)
	{
		transform = r;
		
		dimensions = d;
		
		transform.parent = parent;
		
		transform.localPosition = new Vector3(0.0f, -transform.GetSiblingIndex() * dimensions.y);
		
		transform.name = string.Format ("Row[{0}]", transform.GetSiblingIndex());
	}
	
	private void AddCell(Cell c)
	{
		c.onClick.AddListener (MoveBackward);

		c.clickOnBeat = Board.Instance.rowMoveOnBeat;

		c.transform.SetParent (transform, false);
		
		c.transform.SetAsFirstSibling ();
		
		if(cells.Count < 1)
		{
			c.transform.localPosition = new Vector3(transform.localPosition.x - (2.0f * dimensions.x), 0.0f);
		}
		else
		{
			c.transform.localPosition = new Vector3(cells[0].transform.localPosition.x - dimensions.x, 0.0f);
		}
		
		cells.Insert (0, c);
	}

	public Cell RemoveCell(int index)
	{
		Cell c = null;

		if(index > -1 && Cells.Count > index)
		{
			c = Cells[index];

			Cells.Remove (c);

			c.transform.SetParent (Board.Instance.transform);

			for(int i = 0; i < cells.Count; i++)
			{
				cells[i].transform.localPosition = new Vector3(-transform.localPosition.x + ((i + 1) * dimensions.x), 0.0f);
			}

			MoveBackward ();

			if(c.ccCell != null && cells.Count > index)
			{
				c.ccCell.OnTriggerEnter(cells[index].Coll);
			}
		}

		return c;
	}
	
	public void MoveForward(Cell c)
	{
		transform.Translate (new Vector3(dimensions.x, 0.0f));
		
		int filled = (int)(transform.localPosition.x / dimensions.x);
		
		if(filled > CellCount)
		{
			AddCell (GameObject.Instantiate<Cell>(c));
		}

		if(offset < 0)
		{
			offset++;
		}
	}
	
	public void MoveBackward()
	{
		if(transform.localPosition.x > 0.0f)
		{
			transform.Translate (new Vector3(-dimensions.x, 0.0f));

			offset--;

			onMoveBackward.Invoke(this);	
		}
	}

	public int Offset
	{
		get
		{
			return offset;
		}
	}

	public List<Cell> Cells
	{
		get
		{
			return cells;
		}
	}

	public int CellCount
	{
		get
		{
			return cells.Count;
		}
	}

	public Transform Trans
	{
		get
		{
			return transform;
		}
	}

	public override string ToString ()
	{
		return transform.name;
	}
}

[Serializable]
public class RowBackEvent : UnityEvent<RowData> {}
