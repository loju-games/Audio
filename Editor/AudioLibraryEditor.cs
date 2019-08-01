using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Loju.Audio.Editor
{
    [CustomEditor(typeof(AudioLibrary))]
    public sealed class AudioLibraryEditor : UnityEditor.Editor
    {

        private AudioLibrary _target;
        private AudioLibraryList _list;
        private SerializedProperty _propertyIsMaster;
        private SerializedProperty _propertyMaster;

        private bool _showInherited;

        public void OnEnable()
        {
            _target = target as AudioLibrary;
            _list = new AudioLibraryList(_target, serializedObject, serializedObject.FindProperty("_items"));
            _propertyIsMaster = serializedObject.FindProperty("_isMaster");
            _propertyMaster = serializedObject.FindProperty("_parent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_propertyIsMaster);
            if (!_propertyIsMaster.boolValue) EditorGUILayout.PropertyField(_propertyMaster);

            EditorGUILayout.Space();

            _list.DoLayoutList();

            EditorGUILayout.Space();

            if (!_propertyIsMaster.boolValue)
            {
                string[] inheritKeys = _target.GetInheritedKeys();
                if (inheritKeys != null && inheritKeys.Length > 0)
                {
                    int i = 0, l = inheritKeys.Length;
                    _showInherited = EditorGUILayout.Foldout(_showInherited, string.Format("Inherited ({0})", l));
                    if (_showInherited)
                    {
                        for (; i < l; ++i)
                        {
                            EditorGUILayout.LabelField(inheritKeys[i]);
                        }
                    }

                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

    public sealed class AudioLibraryList : ReorderableList
    {

        private AudioLibrary _library;

        public AudioLibraryList(AudioLibrary library, SerializedObject obj, SerializedProperty property) : base(obj, property, true, true, true, true)
        {
            _library = library;

            base.drawHeaderCallback += OnDrawHeader;
            base.drawElementCallback += OnDrawElement;
            base.elementHeightCallback += OnElementHeight;
        }

        private void OnDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Library Items", EditorStyles.boldLabel);
        }

        private float OnElementHeight(int index)
        {
            SerializedProperty element = base.serializedProperty.GetArrayElementAtIndex(index);

            return EditorGUI.GetPropertyHeight(element) + 6;
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = base.serializedProperty.GetArrayElementAtIndex(index);

            rect.x += 10;
            rect.y += 2;
            rect.width -= 10;

            string key = element.FindPropertyRelative("key").stringValue;
            bool isOverride = _library.IsOverride(key);
            string label = isOverride ? string.Concat(key, " [Override]") : key;

            EditorGUI.PropertyField(rect, element, new GUIContent(label), true);
        }

    }
}