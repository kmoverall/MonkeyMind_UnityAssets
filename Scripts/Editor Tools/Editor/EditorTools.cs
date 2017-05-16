using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

//http://answers.unity3d.com/questions/1073094/custom-inspector-layer-mask-variable.html
public class EditorTools {

    static List<string> layers;
    static string[] layerNames;

    public static LayerMask LayerMaskField(string label, LayerMask selected) {

        if (layers == null) {
            layers = new List<string>();
            layerNames = new string[4];
        }
        else {
            layers.Clear();
        }

        int emptyLayers = 0;
        for (int i = 0; i < 32; i++) {
            string layerName = LayerMask.LayerToName(i);

            if (layerName != "") {

                for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i - emptyLayers));
                layers.Add(layerName);
            }
            else {
                emptyLayers++;
            }
        }

        if (layerNames.Length != layers.Count) {
            layerNames = new string[layers.Count];
        }
        for (int i = 0; i < layerNames.Length; i++) layerNames[i] = layers[i];

        selected.value = EditorGUILayout.MaskField(label, selected.value, layerNames);

        return selected;
    }

	public delegate void SubEditor(SerializedProperty property, int[] indices);
    public delegate bool PropertyComparison(SerializedProperty a, SerializedProperty b);
    public delegate T DefaultValueSetter<T>();

    public static void DrawUnmanagedProperties(this Editor editor, List<string> managedProperties)
    {
        SerializedProperty others = editor.serializedObject.GetIterator();
        others.Next(true);
        do
        {
            if (!managedProperties.Contains(others.name))
            {
                EditorGUILayout.PropertyField(others, true);
            }
        } while (others.NextVisible(false));
    }

    public static void DrawUnmanagedProperties(this Editor editor, string[] managedProperties)
    {
        List<string> props = new List<string>(managedProperties);
        DrawUnmanagedProperties(editor, managedProperties);
    }

    public static void DrawUnmanagedProperties(this Editor editor)
    {
        List<string> managedProperties = new List<string> { "m_Script", "m_ObjectHideFlags" };
        DrawUnmanagedProperties(editor, managedProperties);
    }

    //Draws two fields in one line, labeled Min and Max
    public static void MinMaxField(GUIContent label, SerializedProperty minProperty, SerializedProperty maxProperty)
    {
        EditorGUILayout.BeginHorizontal();

        float labelWidth = EditorGUIUtility.labelWidth;
        int indent = EditorGUI.indentLevel;
        EditorGUILayout.PrefixLabel(label);

        EditorGUIUtility.labelWidth = 30;
        EditorGUI.indentLevel = 0;
        EditorGUILayout.PropertyField(minProperty, new GUIContent("Min"));
        EditorGUILayout.PropertyField(maxProperty, new GUIContent("Max"));
        EditorGUIUtility.labelWidth = labelWidth;
        EditorGUI.indentLevel = indent;

        EditorGUILayout.EndHorizontal();
    }

    public static void MinMaxField(string label, SerializedProperty minProperty, SerializedProperty maxProperty)
    {
        MinMaxField(new GUIContent(label), minProperty, maxProperty);
    }

    //Draws an array with buttons for deleting individual elements, or adding elements to the end
    public static void DynamicArrayField(GUIContent label, SerializedProperty property, SubEditor DrawSubEditor,
                           int[] indices = null, SerializedProperty foldoutField = null,  PropertyComparison SortCompareFunction = null)
    {
        if (!property.isArray)
        {
            EditorGUILayout.HelpBox("Property is not an array!", MessageType.Error);
            return;
        }

        if (foldoutField != null)
            foldoutField.boolValue = EditorGUILayout.Foldout(foldoutField.boolValue, label);
        else
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        if (indices == null)
            indices = new int[0];

        if (foldoutField == null || foldoutField.boolValue)
        {
            EditorGUI.indentLevel++;
            int elementCount = property.arraySize;
            for (int i = 0; i < elementCount; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();
                int[] newIndices = indices;
                System.Array.Resize(ref newIndices, newIndices.Length + 1);
                newIndices[newIndices.Length - 1] = i;
                DrawSubEditor(property.GetArrayElementAtIndex(i), newIndices);
                EditorGUILayout.EndVertical();

                Color defaultBGColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                {
                    elementCount--;
                    //DeleteArrayElementAtIndex only deletes if the element is set to null first. It just sets the element to null if it isn't
                    if (property.GetArrayElementAtIndex(i).propertyType == SerializedPropertyType.ObjectReference)
                        property.GetArrayElementAtIndex(i).objectReferenceValue = null;
                    property.DeleteArrayElementAtIndex(i);
                }
                GUI.backgroundColor = defaultBGColor;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.GetControlRect();
            Rect indentRect = EditorGUI.IndentedRect(GUILayoutUtility.GetLastRect());
            if (GUI.Button(indentRect, "+"))
            {
                property.arraySize++;
            }

            if (SortCompareFunction != null)
            {
                EditorGUILayout.GetControlRect();
                indentRect = EditorGUI.IndentedRect(GUILayoutUtility.GetLastRect());
                indentRect.width /= 2;
                if (GUI.Button(indentRect, "\u25B2"))
                {
                    Sort(property, SortCompareFunction, true);
                }
                indentRect.x += indentRect.width;
                if (GUI.Button(indentRect, "\u25BC"))
                {
                    Sort(property, SortCompareFunction, false);
                }
            }


            EditorGUI.indentLevel--;
        }
    }

    public static void DynamicArrayField(string label, SerializedProperty property, SubEditor DrawSubEditor,
                           int[] indices = null, SerializedProperty foldoutField = null, PropertyComparison SortCompareFunction = null)
    {
        DynamicArrayField(new GUIContent(label), property, DrawSubEditor, indices, foldoutField, SortCompareFunction);
    }

    //Insertion sort implementation. reverseOrder negates the comparison function if true
    static void Sort(SerializedProperty array, PropertyComparison Compare, bool reverseOrder = false)
    {
        if (!array.isArray)
        {
            Debug.LogWarning("Attempted to sort a non-array property");
            return;
        }

        for (int i = 1; i < array.arraySize; i++)
        {
            for (int j = i; j > 0 && (Compare(array.GetArrayElementAtIndex(j), array.GetArrayElementAtIndex(j - 1)) ^ reverseOrder); j--)
            {
                array.MoveArrayElement(j, j - 1);
            }
        }
    }

    //Draws a field with a button that sets the property to a default value based on a function
    public static void FieldWithDefault<T>(SerializedProperty property, ref T variable, DefaultValueSetter<T> DefaultSetter)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        EditorGUILayout.PropertyField(property);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Default"))
        {
            variable = DefaultSetter();
        }
        EditorGUILayout.EndHorizontal();
    }

    //Draws a field with a button that sets the property to a default value based on a variable
    public static void FieldWithDefault<T>(SerializedProperty property, ref T variable, T defaultValue)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
        EditorGUILayout.PropertyField(property);

        if (GUILayout.Button("Default"))
        {
            variable = defaultValue;
        }
        EditorGUILayout.EndHorizontal();
    }
}