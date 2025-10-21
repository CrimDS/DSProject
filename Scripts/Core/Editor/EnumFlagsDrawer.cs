using UnityEditor;
using UnityEngine;

/// <summary>
/// This is an Editor script that tells Unity how to draw the UI for the
/// [EnumFlags] attribute. It creates the multi-select dropdown.
/// This script MUST be placed in a folder named "Editor".
/// </summary>
[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
    }
}

