// Copyright (C) 2014 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using System;


namespace TMPro
{
    
  
    public struct TMPro_MeshInfo
    {           
        public Vector3[] vertices;
        public Vector2[] uv0s;
        public Vector2[] uv2s;
        public Color32[] vertexColors;
        public Vector3[] normals;
        public Vector4[] tangents;
    }
       

    // Structure containing information about each Character & releated Mesh info for the text object.   
    public struct TMPro_CharacterInfo 
    {     
        public char character;
        public short lineNumber;
        public short charNumber;
        public short index;
        public short vertexIndex;
        public Vector3 bottomLeft;
        public Vector3 topRight;
        public float topLine;      
        public float baseLine;
        public float bottomLine;     
        public float aspectRatio;
        public float scale;
        public Color32 color;
        public FontStyles style;       
        public bool isVisible;
    }


    public struct TMPro_TextMetrics
    {
        public int characterCount;
        public int wordCount;
        public int spaceCount;
        public int lineCount;
        public Rect textRect;
    }


    [Serializable]
    public class TextInfo
    {
        // These first 3 fields could be replaced by the TextMetrics      
        public int characterCount;
        public int spaceCount;
        public int wordCount;
        public int lineCount;

        public TMPro_CharacterInfo[] characterInfo;
        public LineInfo[] lineInfo;
        public TMPro_MeshInfo meshInfo;

        // Might was to add bounds in here.
    }


    public struct LineInfo
    {
        public int characterCount;
        public int spaceCount;
        public int wordCount;
        public int firstCharacterIndex;
        public int lastCharacterIndex;
        public float lineLength;
        public Extents lineExtents;

    }


    public struct Extents
    {
        public Vector2 min;
        public Vector2 max;

        public Extents(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }

        public override string ToString()
        {
            string s = "Min (" + min.x.ToString("f2") + ", " + min.y.ToString("f2") + ")   Max (" + max.x.ToString("f2") + ", " + max.y.ToString("f2") + ")";           
            return s;
        }
    }


    [Serializable]
    public struct Mesh_Extents
    {
        public Vector2 min;
        public Vector2 max;
      
     
        public Mesh_Extents(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;           
        }

        public override string ToString()
        {
            string s = "Min (" + min.x.ToString("f2") + ", " + min.y.ToString("f2") + ")   Max (" + max.x.ToString("f2") + ", " + max.y.ToString("f2") + ")";
            //string s = "Center: (" + ")" + "  Extents: (" + ((max.x - min.x) / 2).ToString("f2") + "," + ((max.y - min.y) / 2).ToString("f2") + ").";
            return s;
        }
    }


    // Structure used for Word Wrapping which tracks the state of execution when the last space or carriage return character was encountered. 
    public struct WordWrapState
    {
        public int previous_WordBreak; 
        public int total_CharacterCount;
        public int visible_CharacterCount;
        public float maxLineLength;
        public int wordCount;
        public FontStyles fontStyle;
        public float fontScale;
        public float xAdvance; 
        public float baselineOffset;

        //public TextInfo textInfo;
        public LineInfo lineInfo;
        
        public Color32 vertexColor;
        public Extents meshExtents;
        //public Mesh_Extents lineExtents;    
    }

}
