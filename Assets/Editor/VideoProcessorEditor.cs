using UnityEngine;
using UnityEditor;

namespace DarkRoom {

[CanEditMultipleObjects, CustomEditor(typeof(VideoProcessor))]
sealed class VideoProcessorEditor : Editor
{
    (SerializedProperty prop, GUIContent label) _feedbackAmount;
    (SerializedProperty prop, GUIContent label) _feedbackBlend;
    (SerializedProperty prop, GUIContent label) _noiseFrequency;
    (SerializedProperty prop, GUIContent label) _noiseToFlicker;
    (SerializedProperty prop, GUIContent label) _noiseToShake;
    (SerializedProperty prop, GUIContent label) _noiseToStretch;

    void OnEnable()
    {
        _feedbackAmount = Property("_feedbackAmount", "Amount");
        _feedbackBlend  = Property("_feedbackBlend", "Ratio");
        _noiseFrequency = Property("_noiseFrequency", "Frequency");
        _noiseToFlicker = Property("_noiseToFlicker", "Flicker");
        _noiseToShake   = Property("_noiseToShake", "Shake");
        _noiseToStretch = Property("_noiseToStretch", "Stretch");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Feedback");
        EditorGUI.indentLevel++;
        PropertyField(_feedbackAmount);
        PropertyField(_feedbackBlend);
        EditorGUI.indentLevel--;

        EditorGUILayout.LabelField("Noise Based Effects");
        EditorGUI.indentLevel++;
        PropertyField(_noiseFrequency);
        PropertyField(_noiseToFlicker);
        PropertyField(_noiseToShake);
        PropertyField(_noiseToStretch);
        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }

    (SerializedProperty, GUIContent) Property(string name, string label)
      => (serializedObject.FindProperty(name), new GUIContent(label));

    void PropertyField((SerializedProperty prop, GUIContent label) pair)
      => EditorGUILayout.PropertyField(pair.prop, pair.label);
}

} // namespace DarkRoom
