// Copyright (C) 2014 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using UnityEditor;
using System.Collections;


namespace TMPro.EditorUtilities
{

    [CustomEditor(typeof(TMPro_AdvancedLayout)), CanEditMultipleObjects]
    public class TMPro_AdvancedLayoutEditor : Editor
    {
        private GUISkin mySkin;
        //private GUIStyle textAreaBox;
        private GUIStyle fieldLabel;

        private SerializedProperty prop_isEnabled;
        private SerializedProperty prop_propertiesChanged;


        void OnEnable()
        {
            // Find to location of the TextMesh Pro Asset Folder (as users may have moved it)
            string tmproAssetFolderPath = TMPro_EditorUtility.GetAssetLocation();        
                 
            // Get all serialized properties
            prop_isEnabled = serializedObject.FindProperty("m_isEnabled");
            prop_propertiesChanged = serializedObject.FindProperty("propertiesChanged");

            // Get the appropriate skin based on Dark or Light Unity Skin.
            if (EditorGUIUtility.isProSkin)
                mySkin = AssetDatabase.LoadAssetAtPath(tmproAssetFolderPath + "/GUISkins/TMPro_DarkSkin.guiskin", typeof(GUISkin)) as GUISkin;
            else
                mySkin = AssetDatabase.LoadAssetAtPath(tmproAssetFolderPath + "/GUISkins/TMPro_LightSkin.guiskin", typeof(GUISkin)) as GUISkin;

            if (mySkin != null)
            {
                fieldLabel = mySkin.FindStyle("Section Label");
                //textAreaBox = mySkin.FindStyle("Text Area Box (Editor)");
            }
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Label("<b>TMPro - Advanced Text Layout</b>", fieldLabel);

            EditorGUILayout.PropertyField(prop_isEnabled);
            if (GUI.changed)
                prop_propertiesChanged.boolValue = true;

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
