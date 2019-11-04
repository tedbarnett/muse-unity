// Copyright (C) 2014 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using UnityEditor;


namespace TMPro.EditorUtilities
{
    public static class TMPro_CreateObjectMenu
    {
        [MenuItem("GameObject/Create Other/TextMeshPro Text", false, -30)]
        static void CreateTextMeshProObjectPerform()
        {
            GameObject go = new GameObject("TextMeshPro");
            TextMeshPro textMeshPro = go.AddComponent<TextMeshPro>();
            textMeshPro.text = "Hello World!";
            textMeshPro.anchor = AnchorPositions.BaseLine;
        }

    }
}
