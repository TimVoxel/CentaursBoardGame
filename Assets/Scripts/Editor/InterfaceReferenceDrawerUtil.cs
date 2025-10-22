using UnityEditor;
using UnityEngine;

#nullable enable

public static class InterfaceReferenceDrawerUtil
{
    private static GUIStyle? _labelStyle;

    private static GUIStyle LabelStyle
    {
        get
        {
            if (_labelStyle != null)
            {
                return _labelStyle;
            }

            var style = new GUIStyle(EditorStyles.label)
            {
                font = EditorStyles.objectField.font,
                fontSize = EditorStyles.objectField.fontSize,
                fontStyle = EditorStyles.objectField.fontStyle,
                alignment = TextAnchor.MiddleRight,
                padding = new RectOffset(0, 2, 0, 0)
            };
            _labelStyle = style;
            return _labelStyle;
        }
    }

    public static void Draw(Rect position, SerializedProperty property, GUIContent label, InterfaceArgs args)
    {
        var controlID = GUIUtility.GetControlID(FocusType.Passive) - 1;
        var isHovering = position.Contains(Event.current.mousePosition);
        var displayStrinrg = property.objectReferenceValue == null || isHovering
            ? $"({args.InterfaceType.Name})"
            : "*";

        DrawInterfaceNameLabel(position, displayStrinrg, controlID);
    }

    private static void DrawInterfaceNameLabel(Rect position, string displayString, int controlID)
    {
        if (Event.current.type == EventType.Repaint)
        {
            const int additionalLeftWidth = 3;
            const int verticalIndent = 1;

            var content = EditorGUIUtility.TrTextContent(displayString);
            var size = LabelStyle.CalcSize(content);
            var labelPosition = position;

            labelPosition.width = size.x + additionalLeftWidth;
            labelPosition.x += position.width - labelPosition.width - 18;
            labelPosition.height -= verticalIndent * 2;
            labelPosition.y += verticalIndent;
            LabelStyle.Draw(labelPosition, content, controlID, DragAndDrop.activeControlID == controlID, false);
        }
    }
}