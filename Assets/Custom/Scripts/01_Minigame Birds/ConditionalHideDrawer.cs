#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
public class ConditionalHideDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
        bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

        if (enabled)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
        bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

        if (enabled)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
        return -EditorGUIUtility.standardVerticalSpacing;
    }

    private bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHAtt, SerializedProperty property)
    {
        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.conditionalSourceField);

        if (sourcePropertyValue == null)
        {
            Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourceProperty found: " + condHAtt.conditionalSourceField);
            return true;
        }

        return sourcePropertyValue.boolValue;
    }
}
#endif