﻿using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace EasyText.Editor
{
    [CustomEditor(typeof(SpriteText), true)]
    [CanEditMultipleObjects]
    public class SpriteTextEditor : GraphicEditor
    {
        #region 属性
        private SpriteText _inlineText;
        private string _lastText;
        SerializedProperty _text;
        SerializedProperty m_Text;
        SerializedProperty m_FontData;
        SerializedProperty isVerticle;
        SerializedProperty charSpacing;
        GUIContent _inputGUIContent;
        GUIContent _outputGUIContent;
        #endregion
        protected override void OnEnable()
        {
            base.OnEnable();
            _lastText = "";
            _inputGUIContent = new GUIContent("Input Text");
            _outputGUIContent = new GUIContent("Output Text");

            _text = serializedObject.FindProperty("_text");
            m_Text = serializedObject.FindProperty("m_Text");
            m_FontData = serializedObject.FindProperty("m_FontData");
            isVerticle = serializedObject.FindProperty("isVerticle");
            charSpacing = serializedObject.FindProperty("charSpacing");
        
            _inlineText = target as SpriteText;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_text, _inputGUIContent);
            EditorGUILayout.PropertyField(m_Text, _outputGUIContent);
            EditorGUILayout.PropertyField(m_FontData);
            EditorGUILayout.PropertyField(isVerticle);
            if(isVerticle.boolValue)
                EditorGUILayout.PropertyField(charSpacing);
            AppearanceControlsGUI();
            RaycastControlsGUI();
           
            //更新字符
            if (_inlineText != null && _lastText != _text.stringValue)
            {
                _inlineText.text = _text.stringValue;
                _lastText = _text.stringValue;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
