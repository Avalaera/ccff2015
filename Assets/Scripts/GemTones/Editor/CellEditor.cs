using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

[CustomEditor(typeof(Cell))]
public class CellEditor : ButtonEditor {

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		SerializedProperty graphic = serializedObject.FindProperty("graphic");
		SerializedProperty bg = serializedObject.FindProperty("bg");
		SerializedProperty spriteChoices = serializedObject.FindProperty("spriteChoices");
		SerializedProperty spriteTints = serializedObject.FindProperty("spriteTints");
		SerializedProperty states = serializedObject.FindProperty("states");

		EditorGUILayout.PropertyField(graphic);
		EditorGUILayout.PropertyField(bg);
		EditorGUILayout.PropertyField(spriteChoices, true);
		EditorGUILayout.PropertyField(spriteTints, true);
		EditorGUILayout.PropertyField(states, true);
	
		serializedObject.ApplyModifiedProperties();
		
		base.OnInspectorGUI();
	}
}
