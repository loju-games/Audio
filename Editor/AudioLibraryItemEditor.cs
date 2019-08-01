using UnityEngine;
using UnityEditor;

namespace Loju.Audio.Editor
{

    [CustomPropertyDrawer(typeof(AudioLibraryItem))]
    public sealed class AudioLibraryItemEditor : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;

            SerializedProperty key = property.FindPropertyRelative("key");
            SerializedProperty clips = property.FindPropertyRelative("clips");
            SerializedProperty mode = property.FindPropertyRelative("selectAtRandom");
            SerializedProperty volume = property.FindPropertyRelative("volumeScale");

            float height = EditorGUI.GetPropertyHeight(key) + EditorGUIUtility.singleLineHeight;
            height += EditorGUI.GetPropertyHeight(volume);
            height += EditorGUI.GetPropertyHeight(clips);
            if (clips.arraySize > 1) height += EditorGUI.GetPropertyHeight(mode);

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty key = property.FindPropertyRelative("key");
            Rect foldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, label);

            if (property.isExpanded)
            {
                SerializedProperty clips = property.FindPropertyRelative("clips");
                SerializedProperty mode = property.FindPropertyRelative("selectAtRandom");
                SerializedProperty volume = property.FindPropertyRelative("volumeScale");

                int indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                EditorGUI.BeginProperty(position, label, property);

                Rect keyRect = new Rect(position.x, foldRect.y + foldRect.height, position.width, EditorGUI.GetPropertyHeight(key));
                Rect volumeRect = new Rect(position.x, keyRect.y + keyRect.height, position.width, EditorGUI.GetPropertyHeight(volume));
                Rect clipsRect = new Rect(position.x, volumeRect.y + volumeRect.height, position.width, EditorGUI.GetPropertyHeight(clips));
                Rect modeRect = new Rect(position.x, clipsRect.y + clipsRect.height, position.width, EditorGUI.GetPropertyHeight(mode));

                EditorGUI.PropertyField(keyRect, key);
                EditorGUI.PropertyField(volumeRect, volume);
                EditorGUI.PropertyField(clipsRect, clips, true);
                if (clips.arraySize > 1) EditorGUI.PropertyField(modeRect, mode);

                EditorGUI.EndProperty();

                EditorGUI.indentLevel = indent;
            }
        }
    }

}