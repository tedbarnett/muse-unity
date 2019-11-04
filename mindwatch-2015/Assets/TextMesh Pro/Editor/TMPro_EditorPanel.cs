// Copyright (C) 2014 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using UnityEditor;
using System.Collections;


namespace TMPro.EditorUtilities
{

    [CustomEditor(typeof(TextMeshPro)), CanEditMultipleObjects]
    public class TMPro_EditorPanel : Editor
    {
      

        private struct m_foldout
        { // Track Inspector foldout panel states, globally.
            public static bool textInput = true;
            public static bool fontSettings = true;
            public static bool extraSettings = false;
            public static bool shadowSetting = false;
            public static bool materialEditor = true;
        }

        private static int m_eventID;

        private static string[] uiStateLabel = new string[] { "<i>(Click to expand)</i>", "<i>(Click to collapse)</i>" };

        private const string k_UndoRedo = "UndoRedoPerformed";

        private GUISkin mySkin;
        //private GUIStyle Group_Label;
        private GUIStyle textAreaBox;
        private GUIStyle Section_Label;

        private SerializedProperty text_prop;   
        private SerializedProperty fontAsset_prop;
        private SerializedProperty fontColor_prop;
        private SerializedProperty fontSize_prop;
        private SerializedProperty characterSpacing_prop;
        private SerializedProperty lineLength_prop;
        private SerializedProperty lineSpacing_prop;
        private SerializedProperty lineJustification_prop;
        private SerializedProperty anchorPosition_prop;
        private SerializedProperty horizontalMapping_prop;
        private SerializedProperty verticalMapping_prop;

        private SerializedProperty enableWordWrapping_prop;
        private SerializedProperty wordWrappingRatios_prop;
        private SerializedProperty enableKerning_prop;

        private SerializedProperty overrideHtmlColor_prop;

        private SerializedProperty inputSource_prop;
        private SerializedProperty havePropertiesChanged_prop;
        private SerializedProperty isInputPasingRequired_prop;
        private SerializedProperty isAffectingWordWrapping_prop;
        private SerializedProperty isRichText_prop;

        private SerializedProperty hasFontAssetChanged_prop;

        private SerializedProperty enableExtraPadding_prop;
        private SerializedProperty checkPaddingRequired_prop;

        private SerializedProperty isOrthographic_prop;

        //private SerializedProperty textRectangle_prop;

        //private SerializedProperty isMaskUpdateRequired_prop;
        //private SerializedProperty mask_prop;
        //private SerializedProperty maskOffset_prop;
        //private SerializedProperty maskOffsetMode_prop;
        //private SerializedProperty maskSoftness_prop;

        private SerializedProperty vertexOffset_prop;


        private SerializedProperty sortingLayerID_prop;
        private SerializedProperty sortingOrder_prop;

     
        private bool havePropertiesChanged = false;


        private TextMeshPro m_textMeshProScript;
        private Transform m_transform;
        private Renderer m_renderer;
        //private TMPro_UpdateManager m_updateManager;

        private Vector3[] handlePoints = new Vector3[4]; // { new Vector3(-10, -10, 0), new Vector3(-10, 10, 0), new Vector3(10, 10, 0), new Vector3(10, -10, 0) };
        private float prev_lineLenght;



        public void OnEnable()
        {
            // Initialize the Event Listener for Undo Events.
            Undo.undoRedoPerformed += OnUndoRedo;
            //Undo.postprocessModifications += OnUndoRedoEvent;   
      
            text_prop = serializedObject.FindProperty("m_text");
            fontAsset_prop = serializedObject.FindProperty("m_fontAsset");

            fontSize_prop = serializedObject.FindProperty("m_fontSize");
            fontColor_prop = serializedObject.FindProperty("m_fontColor");
            characterSpacing_prop = serializedObject.FindProperty("m_characterSpacing");
            lineLength_prop = serializedObject.FindProperty("m_lineLength");
            //textRectangle_prop = serializedObject.FindProperty("m_textRectangle");
            lineSpacing_prop = serializedObject.FindProperty("m_lineSpacing");
            lineJustification_prop = serializedObject.FindProperty("m_lineJustification");
            anchorPosition_prop = serializedObject.FindProperty("m_anchor");
            horizontalMapping_prop = serializedObject.FindProperty("m_horizontalMapping");
            verticalMapping_prop = serializedObject.FindProperty("m_verticalMapping");
            enableKerning_prop = serializedObject.FindProperty("m_enableKerning");
            overrideHtmlColor_prop = serializedObject.FindProperty("m_overrideHtmlColors");
            enableWordWrapping_prop = serializedObject.FindProperty("m_enableWordWrapping");
            wordWrappingRatios_prop = serializedObject.FindProperty("m_wordWrappingRatios");

            isOrthographic_prop = serializedObject.FindProperty("m_isOrthographic");

            havePropertiesChanged_prop = serializedObject.FindProperty("havePropertiesChanged");
            inputSource_prop = serializedObject.FindProperty("m_inputSource");
            isInputPasingRequired_prop = serializedObject.FindProperty("isInputParsingRequired");
            isAffectingWordWrapping_prop = serializedObject.FindProperty("isAffectingWordWrapping");
            enableExtraPadding_prop = serializedObject.FindProperty("m_enableExtraPadding");
            isRichText_prop = serializedObject.FindProperty("m_isRichText");
            checkPaddingRequired_prop = serializedObject.FindProperty("checkPaddingRequired");

            //isMaskUpdateRequired_prop = serializedObject.FindProperty("isMaskUpdateRequired");
            //mask_prop = serializedObject.FindProperty("m_mask");
            //maskOffset_prop= serializedObject.FindProperty("m_maskOffset");
            //maskOffsetMode_prop = serializedObject.FindProperty("m_maskOffsetMode");
            //maskSoftness_prop = serializedObject.FindProperty("m_maskSoftness");
            //vertexOffset_prop = serializedObject.FindProperty("m_vertexOffset");

            sortingLayerID_prop = serializedObject.FindProperty("m_sortingLayerID");
            sortingOrder_prop = serializedObject.FindProperty("m_sortingOrder");

            hasFontAssetChanged_prop = serializedObject.FindProperty("hasFontAssetChanged");

            // Find to location of the TextMesh Pro Asset Folder (as users may have moved it)
            string tmproAssetFolderPath = TMPro_EditorUtility.GetAssetLocation();        

            if (EditorGUIUtility.isProSkin)
                mySkin = AssetDatabase.LoadAssetAtPath(tmproAssetFolderPath + "/GUISkins/TMPro_DarkSkin.guiskin", typeof(GUISkin)) as GUISkin;
            else
                mySkin = AssetDatabase.LoadAssetAtPath(tmproAssetFolderPath + "/GUISkins/TMPro_LightSkin.guiskin", typeof(GUISkin)) as GUISkin;

            if (mySkin != null)
            {
                Section_Label = mySkin.FindStyle("Section Label");
                //Group_Label = mySkin.FindStyle("Group Label");
                textAreaBox = mySkin.FindStyle("Text Area Box (Editor)");
            }

            m_textMeshProScript = (TextMeshPro)target;
            m_transform = Selection.activeGameObject.transform;
            m_renderer = Selection.activeGameObject.GetComponent<Renderer>();

            //m_updateManager = Camera.main.gameObject.GetComponent<TMPro_UpdateManager>();
        }


        public void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            //Undo.postprocessModifications -= OnUndoRedoEvent;  
        }


        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            GUILayout.Label("<b>TEXT INPUT BOX</b>  <i>(Type your text below.)</i>", Section_Label, GUILayout.Height(23));

            GUI.changed = false;

            text_prop.stringValue = EditorGUILayout.TextArea(text_prop.stringValue, textAreaBox, GUILayout.Height(75), GUILayout.ExpandWidth(true));
            if (GUI.changed)
            {
                GUI.changed = false;
                inputSource_prop.enumValueIndex = 0;
                isInputPasingRequired_prop.boolValue = true;
                isAffectingWordWrapping_prop.boolValue = true;
                havePropertiesChanged = true;         
            }


            GUILayout.Label("<b>FONT SETTINGS</b>", Section_Label);

            EditorGUIUtility.fieldWidth = 30;

            // FONT ASSET
            EditorGUILayout.PropertyField(fontAsset_prop);
            if (GUI.changed)
            {
                GUI.changed = false;
                Undo.RecordObject(m_renderer, "Asset & Material Change");              
                havePropertiesChanged = true;
                hasFontAssetChanged_prop.boolValue = true;
                isAffectingWordWrapping_prop.boolValue = true;               
            }

            // FACE VERTEX COLOR
            EditorGUILayout.PropertyField(fontColor_prop, new GUIContent("Face Color"));
            if (GUI.changed)
            {
                GUI.changed = false;
                havePropertiesChanged = true;
            }

            // FONT SIZE & CHARACTER SPACING GROUP
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(fontSize_prop);
            EditorGUILayout.PropertyField(characterSpacing_prop);
            EditorGUILayout.EndHorizontal();


            // LINE LENGHT & LINE SPACING GROUP
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(lineLength_prop);
            lineLength_prop.floatValue = Mathf.Round(lineLength_prop.floatValue * 100) / 100f; // Rounding Line Length to 2 decimal. 
            if (GUI.changed)
            {
                GUI.changed = false;
                havePropertiesChanged = true;
                isAffectingWordWrapping_prop.boolValue = true;
            }

            EditorGUILayout.PropertyField(lineSpacing_prop);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.PropertyField(lineJustification_prop);
            if (lineJustification_prop.enumValueIndex == 3)
                EditorGUILayout.Slider(wordWrappingRatios_prop, 0.0f, 1.0f, new GUIContent("Wrap Ratios (W <-> C)"));


            EditorGUILayout.PropertyField(anchorPosition_prop, new GUIContent("Anchor Position:"));


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("UV Mapping Options");
            EditorGUILayout.PropertyField(horizontalMapping_prop, GUIContent.none, GUILayout.MinWidth(70f));
            EditorGUILayout.PropertyField(verticalMapping_prop, GUIContent.none, GUILayout.MinWidth(70f));
            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
            {
                GUI.changed = false;
                havePropertiesChanged = true;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(enableWordWrapping_prop, new GUIContent("Enable Word Wrap?"));
            if (GUI.changed)
            {
                GUI.changed = false;
                havePropertiesChanged = true;
                isAffectingWordWrapping_prop.boolValue = true;
                isInputPasingRequired_prop.boolValue = true;
            }
            EditorGUILayout.PropertyField(overrideHtmlColor_prop, new GUIContent("Override Color Tags?"));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(enableKerning_prop, new GUIContent("Enable Kerning?"));
            if (GUI.changed)
            {
                GUI.changed = false;
                isAffectingWordWrapping_prop.boolValue = true;
                havePropertiesChanged = true;
            }
            EditorGUILayout.PropertyField(enableExtraPadding_prop, new GUIContent("Extra Padding?"));
            if (GUI.changed)
            {
                GUI.changed = false;
                havePropertiesChanged = true;
                checkPaddingRequired_prop.boolValue = true;
            }
            EditorGUILayout.EndHorizontal();

            
            if (GUILayout.Button("<b>EXTRA SETTINGS</b>\t\t\t" + (m_foldout.extraSettings ? uiStateLabel[1] : uiStateLabel[0]), Section_Label))
                m_foldout.extraSettings = !m_foldout.extraSettings;

            if (m_foldout.extraSettings)
            {
                EditorGUI.indentLevel = 0;

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(sortingLayerID_prop);
                EditorGUILayout.PropertyField(sortingOrder_prop);

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(isOrthographic_prop, new GUIContent("Orthographic Mode?"));
                EditorGUILayout.PropertyField(isRichText_prop, new GUIContent("Enable Rich Text?"));
                //EditorGUILayout.PropertyField(textRectangle_prop, true);

                if (EditorGUI.EndChangeCheck())                                 
                    havePropertiesChanged = true;


                // EditorGUI.BeginChangeCheck();
                //EditorGUILayout.PropertyField(mask_prop);
                //EditorGUILayout.PropertyField(maskOffset_prop, true);
                //EditorGUILayout.PropertyField(maskSoftness_prop);
                //if (EditorGUI.EndChangeCheck())
                //{
                //    isMaskUpdateRequired_prop.boolValue = true;
                //    havePropertiesChanged = true;
                //}

                //EditorGUILayout.PropertyField(sortingLayerID_prop);
                //EditorGUILayout.PropertyField(sortingOrder_prop);

                // Mask Selection
            }
            


           

            if (havePropertiesChanged)
            {             
                havePropertiesChanged_prop.boolValue = true;
                havePropertiesChanged = false;              
                //m_updateManager.ScheduleObjectForUpdate(m_textMeshProScript);
            }

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
          
            /*
            Editor materialEditor = Editor.CreateEditor(m_renderer.sharedMaterial);
            if (materialEditor != null)
            {
                if (GUILayout.Button("<b>MATERIAL SETTINGS</b>     - <i>Click to expand</i> -", Section_Label))
                    m_foldout.materialEditor= !m_foldout.materialEditor;

                if (m_foldout.materialEditor)
                {
                    materialEditor.OnInspectorGUI();
                }
            }
            */
        }


        public void OnSceneGUI()
        {
            
            if (enableWordWrapping_prop.boolValue)
            {                             
                // Show Handles to represent Line Lenght settings      
                Bounds meshExtents = m_textMeshProScript.bounds;
                Vector3 lossyScale = m_transform.lossyScale;
             
                handlePoints[0] = m_transform.TransformPoint(new Vector3(meshExtents.min.x * lossyScale.x, meshExtents.min.y, 0));
                handlePoints[1] = m_transform.TransformPoint(new Vector3(meshExtents.min.x * lossyScale.x, meshExtents.max.y, 0));
                handlePoints[2] = handlePoints[1] + m_transform.TransformDirection(new Vector3(m_textMeshProScript.lineLength * lossyScale.x, 0, 0));
                handlePoints[3] = handlePoints[0] + m_transform.TransformDirection(new Vector3(m_textMeshProScript.lineLength * lossyScale.x, 0, 0));

                Handles.DrawSolidRectangleWithOutline(handlePoints, new Color32(0, 0, 0, 0), new Color32(255, 255, 0, 255));

                Vector3 old_right = (handlePoints[2] + handlePoints[3]) * 0.5f;
                Vector3 new_right = Handles.FreeMoveHandle(old_right, Quaternion.identity, HandleUtility.GetHandleSize(m_transform.position) * 0.05f, Vector3.zero, Handles.DotCap);
                
                if (old_right != new_right)
                {
                    float delta = new_right.x - old_right.x;                   
                    m_textMeshProScript.lineLength += delta / lossyScale.x;
                }
            }
            

            /* New Experimental Code
            // Margin Frame & Handles      
            Vector3 rectPos = m_transform.position;
            Vector4 textRect = m_textMeshProScript.textRectangle;

            handlePoints[0] = rectPos + m_transform.TransformDirection(new Vector3(- textRect.x, - textRect.w, 0)); // BL
            handlePoints[1] = rectPos + m_transform.TransformDirection(new Vector3(- textRect.x, + textRect.y, 0)); // TL
            handlePoints[2] = rectPos + m_transform.TransformDirection(new Vector3(+ textRect.z, + textRect.y, 0)); // TR
            handlePoints[3] = rectPos + m_transform.TransformDirection(new Vector3(+ textRect.z, - textRect.w, 0));   // BR

            Handles.DrawSolidRectangleWithOutline(handlePoints, new Color32(255, 255, 255, 0), new Color32(255, 255, 0, 255));

            // Draw & process FreeMoveHandles

            // LEFT HANDLE
            Vector3 old_left = (handlePoints[0] + handlePoints[1]) * 0.5f;
            Vector3 new_left = Handles.FreeMoveHandle(old_left, Quaternion.identity, HandleUtility.GetHandleSize(rectPos) * 0.05f, Vector3.zero, Handles.DotCap);
            bool hasChanged = false;
            if (old_left != new_left)
            {
                float delta = old_left.x - new_left.x;
                textRect.x += delta;              
                //Debug.Log("Left Margin H0:" + handlePoints[0] + "  H1:" + handlePoints[1]);
                hasChanged = true;
            }

            // TOP HANDLE
            Vector3 old_top = (handlePoints[1] + handlePoints[2]) * 0.5f;
            Vector3 new_top = Handles.FreeMoveHandle(old_top, Quaternion.identity, HandleUtility.GetHandleSize(rectPos) * 0.05f, Vector3.zero, Handles.DotCap);
            if (old_top != new_top)
            {
                float delta = old_top.y - new_top.y;             
                textRect.y -= delta;
                //Debug.Log("Top Margin H1:" + handlePoints[1] + "  H2:" + handlePoints[2]);   
                hasChanged = true;
            }

            // RIGHT HANDLE
            Vector3 old_right = (handlePoints[2] + handlePoints[3]) * 0.5f;
            Vector3 new_right = Handles.FreeMoveHandle(old_right, Quaternion.identity, HandleUtility.GetHandleSize(rectPos) * 0.05f, Vector3.zero, Handles.DotCap);
            if (old_right != new_right)
            {
                float delta = old_right.x - new_right.x;
                textRect.z -= delta;               
                hasChanged = true;
                //Debug.Log("Right Margin H2:" + handlePoints[2] + "  H3:" + handlePoints[3]);
            }

            // BOTTOM HANDLE
            Vector3 old_bottom = (handlePoints[3] + handlePoints[0]) * 0.5f;
            Vector3 new_bottom = Handles.FreeMoveHandle(old_bottom, Quaternion.identity, HandleUtility.GetHandleSize(rectPos) * 0.05f, Vector3.zero, Handles.DotCap);
            if (old_bottom != new_bottom)
            {
                float delta = old_bottom.y - new_bottom.y;
                textRect.w += delta;              
                hasChanged = true;
                //Debug.Log("Bottom Margin H0:" + handlePoints[0] + "  H3:" + handlePoints[3]);
            }

            if (hasChanged)
            {
                m_textMeshProScript.textRectangle = textRect;
                //m_textMeshProScript.ForceMeshUpdate();
            }
            */


        }

        // Special Handling of Undo / Redo Events.
        private void OnUndoRedo()
        {
            int undoEventID = Undo.GetCurrentGroup();
            int LastUndoEventID = m_eventID;

            if (undoEventID != LastUndoEventID)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    //Debug.Log("Undo & Redo Performed detected in Editor Panel. Event ID:" + Undo.GetCurrentGroup());
                    TMPro_EventManager.ON_TEXTMESHPRO_PROPERTY_CHANGED(true, targets[i] as TextMeshPro);
                    m_eventID = undoEventID;
                }
            }
        }

        /*
        private UndoPropertyModification[] OnUndoRedoEvent(UndoPropertyModification[] modifications)
        {
            int eventID = Undo.GetCurrentGroup();
            PropertyModification modifiedProp = modifications[0].propertyModification;      
            System.Type targetType = modifiedProp.target.GetType();
              
            if (targetType == typeof(Material))
            {
                //Debug.Log("Undo / Redo Event Registered in Editor Panel on Target: " + targetObject);
           
                //TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(true, targetObject as Material);
                //EditorUtility.SetDirty(targetObject);        
            }
  
            //string propertyPath = modifications[0].propertyModification.propertyPath;  
            //if (propertyPath == "m_fontAsset")
            //{
                //int currentEvent = Undo.GetCurrentGroup();
                //Undo.RecordObject(Selection.activeGameObject.renderer.sharedMaterial, "Font Asset Changed");
                //Undo.CollapseUndoOperations(currentEvent);
                //Debug.Log("Undo / Redo Event: Font Asset changed. Event ID:" + Undo.GetCurrentGroup());
            
            //}

            //Debug.Log("Undo / Redo Event Registered in Editor Panel on Target: " + modifiedProp.propertyPath + "  Undo Event ID:" + eventID + "  Stored ID:" + TMPro_EditorUtility.UndoEventID);
            //TextMeshPro_EventManager.ON_TEXTMESHPRO_PROPERTY_CHANGED(true, target as TextMeshPro);
            return modifications;
        }
        */
    }
}