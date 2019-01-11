using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HexCoordinates))]
public class HexCoordinatesDrawer :PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        int x = property.FindPropertyRelative("x").intValue;
        int z = property.FindPropertyRelative("z").intValue;
        HexCoordinates coordinates = new HexCoordinates(x, z);

        position = EditorGUI.PrefixLabel(position, label);
        GUI.Label(position, coordinates.ToString());
    }
}
