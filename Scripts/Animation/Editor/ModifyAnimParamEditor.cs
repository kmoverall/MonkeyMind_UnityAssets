using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(ModifyAnimParam))]
public class ModifyAnimParamEditor : Editor {

    void OnEnable () {

	}
	
	public override void OnInspectorGUI () {
		serializedObject.Update();

        serializedObject.FindProperty("isInChild").boolValue = EditorGUILayout.ToggleLeft("Is In Child", serializedObject.FindProperty("isInChild").boolValue);
        if (serializedObject.FindProperty("isInChild").boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("inChild"), new GUIContent("Child Name"));
        }
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerEvent"));

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("paramName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("paramType"));

        int[] modIndices = new int[0];
        string[] modNames = new string[0];
        switch (serializedObject.FindProperty("paramType").enumValueIndex)
        {
            case (int)ModifyAnimParam.Type.Float:
            case (int)ModifyAnimParam.Type.Int:
                modIndices = new int[]
                {
                    (int)ModifyAnimParam.Modification.Set,
                    (int)ModifyAnimParam.Modification.Add,
                    (int)ModifyAnimParam.Modification.Subtract,
                    (int)ModifyAnimParam.Modification.Multiply,
                    (int)ModifyAnimParam.Modification.Divide,
                    (int)ModifyAnimParam.Modification.Modulus,
                };
                modNames = new string[] { "Set", "Add", "Subtract", "Multiply", "Divide", "Modulus" };
                break;
            case (int)ModifyAnimParam.Type.Bool:
                modIndices = new int[]
                {
                    (int)ModifyAnimParam.Modification.Set,
                    (int)ModifyAnimParam.Modification.Toggle,
                };
                modNames = new string[] { "Set", "Toggle" };
                break;
            case (int)ModifyAnimParam.Type.Trigger:
                modIndices = new int[]
                {
                    (int)ModifyAnimParam.Modification.Set,
                    (int)ModifyAnimParam.Modification.Reset,
                };
                modNames = new string[] { "Set", "Reset" };
                break;

        }
        serializedObject.FindProperty("modifier").enumValueIndex = 
            EditorGUILayout.IntPopup("Function", serializedObject.FindProperty("modifier").enumValueIndex, modNames, modIndices);

        switch (serializedObject.FindProperty("paramType").enumValueIndex)
        {
            case (int)ModifyAnimParam.Type.Float:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("floatValue"), new GUIContent("Value"));
                break;
            case (int)ModifyAnimParam.Type.Int:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("intValue"), new GUIContent("Value"));
                break;
            case (int)ModifyAnimParam.Type.Bool:
                if (serializedObject.FindProperty("modifier").enumValueIndex == (int)ModifyAnimParam.Modification.Set)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("boolValue"), new GUIContent("Value"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
	}
}
