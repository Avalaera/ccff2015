using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

[CustomEditor(typeof(DialButton))][CanEditMultipleObjects()]
public class DialButtonEditor : ButtonEditor {
	
	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		SerializedProperty mult = serializedObject.FindProperty("mult");
		SerializedProperty rb = serializedObject.FindProperty("rb");

		EditorGUILayout.PropertyField (mult);
		EditorGUILayout.PropertyField(rb);

		serializedObject.ApplyModifiedProperties();

		base.OnInspectorGUI();

		serializedObject.Update();
		
		SerializedProperty oPD = serializedObject.FindProperty("onPointerDown");
		SerializedProperty oPU = serializedObject.FindProperty("onPointerUp");
		
		EditorGUILayout.PropertyField(oPD);
		EditorGUILayout.PropertyField(oPU);
		
		serializedObject.ApplyModifiedProperties();
	}
}
