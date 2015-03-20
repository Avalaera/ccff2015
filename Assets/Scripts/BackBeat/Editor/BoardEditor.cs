using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Board))]
public class BoardEditor : Editor {

	private Board board = null;

	private Vector3 mouseWorldPos, lastHovered, vKey;

	private Vector3? dropTarget = null;

	private float ccGUISize = 1.0f;

	private void OnEnable()
	{
		board = target as Board;
	}

	private void OnDisable()
	{
		GUIUtility.hotControl = 0;
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		GUILayout.Space (20.0f);

		if(GUILayout.Button ("Clear Critical Cells"))
		{
			board.CCDict.FullClear ();

			board.ccDict = new CriticalCellDictionary();
		}

		GUILayout.Space (10.0f);

		if(GUI.changed)
		{
			EditorUtility.SetDirty (target);
		}
	}

	private void OnSceneGUI()
	{
		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

		mouseWorldPos = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition).GetPoint (-SceneView.lastActiveSceneView.camera.transform.position.z + 0.7f);

		if(DragAndDrop.objectReferences.Length == 0)
		{
			if(Event.current.type == EventType.MouseDrag)
			{
				if(board.CCDict.ContainsKey (lastHovered))
				{
					DragAndDrop.PrepareStartDrag ();

					DragAndDrop.objectReferences = new Object[] { null };

					DragAndDrop.SetGenericData ("CriticalCellData", (object) board.CCDict[lastHovered]);
		
					DragAndDrop.StartDrag (string.Format ("({0},{1})", lastHovered.x, lastHovered.y));
				}
				
				Event.current.Use ();
			}
		}
		else
		{
			if(dropTarget != null)
			{
				if(Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Link;

					Event.current.Use ();
				}
				else if(Event.current.type == EventType.DragExited)
				{
					CriticalCellData c = (CriticalCellData) DragAndDrop.GetGenericData ("CriticalCellData");
				
					if(!board.CCDict.ContainsKey (dropTarget.Value))
					{
						board.CCDict.Add (dropTarget.Value, new CriticalCellData());
					}

					c.NeighborPositions.Add (dropTarget.Value);
					board.CCDict[dropTarget.Value].NeighborPositions.Add (lastHovered);

					DragAndDrop.AcceptDrag();

					DragAndDrop.SetGenericData ("CriticalCellData", null);

					Event.current.Use ();
				}
			}
			else
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;

				if(Event.current.type == EventType.DragExited)
				{
					DragAndDrop.AcceptDrag();
					
					DragAndDrop.SetGenericData ("CriticalCellData", null);
					
					Event.current.Use ();
				}
			}
		}

		if(Event.current.type == EventType.Repaint)
		{
			dropTarget = null;
		}

		ccGUISize = Mathf.Min (board.cellPrefab.Size.x, board.cellPrefab.Size.y) / 4.0f;

		board.Perform2DArrayAction (DrawButton);

		if(GUI.changed)
		{
			EditorUtility.SetDirty (target);
		}
	}

	private void DrawButton(int x, int y)
	{
		Vector3 v = new Vector3(board.transform.position.x + (x * board.cellPrefab.Size.x), board.transform.position.y - (y * board.cellPrefab.Size.y), board.transform.position.z);
		vKey = new Vector3(board.transform.position.x + x, board.transform.position.y - y, board.transform.position.z);

		if(Handles.Button(v, Quaternion.identity, ccGUISize, ccGUISize, DrawRectangle))
		{	
			if(board.CCDict.ContainsKey (vKey))
			{
				CriticalCellData c = board.CCDict[vKey];

				for(int i = 0; i < c.NeighborPositions.Count; i++)
				{
					board.CCDict[c.NeighborPositions[i]].NeighborPositions.Remove (vKey);
				}

				board.CCDict.Remove (vKey);
			}
			else
			{
				board.CCDict.Add (vKey, new CriticalCellData());
			}
		}

		if(board.CCDict.ContainsKey (vKey))
		{
			CriticalCellData c = board.CCDict[vKey];

			Color prevC = Handles.color;

			Handles.color = board.autoClearOnMatch ? Color.cyan : Color.magenta;

			for(int i = 0; i < c.NeighborPositions.Count; i++)
			{
				Vector3 nP = new Vector3(board.transform.position.x + ((c.NeighborPositions[i].x - board.transform.position.x) * board.cellPrefab.Size.x), board.transform.position.y - ((board.transform.position.y - c.NeighborPositions[i].y) * board.cellPrefab.Size.y), board.transform.position.z);

				Vector3 dir = nP - v;
				dir.Normalize ();

				Vector3 start = v + (dir * ccGUISize);
				Vector3 end = nP - (dir * ccGUISize);

				Handles.DrawLine(start, end);
			}

			Handles.color = prevC;
		}
	}

	private void DrawRectangle(int controlId, Vector3 pos, Quaternion rotation, float size)
	{
		Vector3[] verts = new Vector3[4]
		{
			new Vector3(pos.x - size, pos.y - size, pos.z), 
		    new Vector3(pos.x - size, pos.y + size, pos.z), 
		    new Vector3(pos.x + size, pos.y + size, pos.z), 
			new Vector3(pos.x + size, pos.y - size, pos.z)
		};

		if(mouseWorldPos.x >= verts[0].x && mouseWorldPos.x <= verts[2].x && mouseWorldPos.y >= verts[0].y && mouseWorldPos.y <= verts[2].y)
		{
			CriticalCellData c = (CriticalCellData) DragAndDrop.GetGenericData ("CriticalCellData");

			if(c != null)
			{
				if(lastHovered != vKey && !c.NeighborPositions.Contains (vKey))
				{
					dropTarget = vKey;
				}
			}
			else
			{
				lastHovered = vKey;
			}
		}

		Handles.DrawSolidRectangleWithOutline(verts, board.CCDict.ContainsKey (new Vector2(vKey.x, vKey.y)) ? Color.grey : Color.clear, board.autoClearOnMatch ? Color.cyan : Color.magenta);
	}
}
