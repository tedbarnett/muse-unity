// Copyright (C) 2014 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms
// Beta Release 0.1.44.


using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



namespace TMPro
{
   
    public enum AnchorPositions { TopLeft, Top, TopRight, Left, Center, Right, BottomLeft, Bottom, BottomRight, BaseLine };
    public enum AlignmentTypes { Left, Center, Right, Justified };
    public enum MaskingTypes { MaskOff = 0, MaskHard = 1, MaskSoft = 2 };
    public enum MaskingOffsetMode {  Percentage = 0, Pixel = 1 };  
    public enum TextureMappingOptions { Character = 0, Line = 1, Paragraph = 2, MatchAspect = 3 };
    public enum FontStyles { Normal = 0, Bold = 1, Italic = 2, Underline = 4, BoldItalic = Bold + Italic, BoldUnderline = Bold + Underline, BoldItalicUnderline = BoldUnderline + Italic, ItalicUnderline = Italic + Underline };


    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [AddComponentMenu("Mesh/TextMesh Pro")]
    public partial class TextMeshPro : MonoBehaviour 
    {
        // Public Properties & Serializable Properties  
        
        /// <summary>
        /// A string containing the text to be displayed.
        /// </summary>
        public string text
        {
            get { return m_text; }
            set { if (m_text != value) { m_inputSource = TextInputSources.Text; havePropertiesChanged = true; isAffectingWordWrapping = true; isInputParsingRequired = true; m_text = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// The TextMeshPro font asset to be assigned to this text object.
        /// </summary>
        public TextMeshProFont font
        {
            get { return m_fontAsset; }
            set { m_fontAsset = value; LoadFontAsset(); havePropertiesChanged = true; /* hasFontAssetChanged = true;*/ isAffectingWordWrapping = true; /* ScheduleUpdate(); */ }
        }


        /// <summary>
        /// The material to be assigned to this text object. An instance of the material will be assigned to the object's renderer.
        /// </summary>
        public Material fontMaterial
        {
            get { return GetComponent<Renderer>().material; }
            set { if (m_fontMaterial != value) { SetFontMaterial(value); havePropertiesChanged = true; m_fontMaterial = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// The material to be assigned to this text object.
        /// </summary>
        public Material fontSharedMaterial
        {
            get { return GetComponent<Renderer>().sharedMaterial; }
            set { if (m_sharedMaterial != value) { SetSharedFontMaterial(value); havePropertiesChanged = true; m_sharedMaterial = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// Sets the RenderQueue along with Ztest to force the text to be drawn last and on top of scene elements.
        /// </summary>
        public bool isOverlay
        {
            get { return m_isOverlay; }
            set { if (m_isOverlay != value) { m_isOverlay = value; SetShaderType(); havePropertiesChanged = true; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// This is the default vertex color assigned to each vertices. Color tags will override vertex colors unless the overrideColorTags is set.
        /// </summary>
        public Color32 color
        {
            get { return m_fontColor; }
            set { if (m_fontColor.Compare(value) == false) { havePropertiesChanged = true; m_fontColor = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// Sets the color of the _FaceColor property of the assigned material. Changing face color will result in an instance of the material.
        /// </summary>
        public Color32 faceColor
        {
            get { return m_faceColor; }
            set { if (m_faceColor.Compare(value) == false) { SetFaceColor(value); havePropertiesChanged = true; m_faceColor = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// Sets the color of the _OutlineColor property of the assigned material. Changing outline color will result in an instance of the material.
        /// </summary>
        public Color32 outlineColor
        {
            get { return m_outlineColor; }
            set { if (m_outlineColor.Compare(value) == false) { SetOutlineColor(value); havePropertiesChanged = true; m_outlineColor = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// Sets the thickness of the outline of the font. Setting this value will result in an instance of the material.
        /// </summary>
        public float outlineWidth
        {
            get { return m_outlineWidth; }
            set { SetOutlineThickness(value); havePropertiesChanged = true; checkPaddingRequired = true; m_outlineWidth = value; /* ScheduleUpdate(); */ }
        }


        /// <summary>
        /// The size of the font.
        /// </summary>
        public float fontSize
        {
            get { return m_fontSize; }
            set { if (m_fontSize != value) { havePropertiesChanged = true; isAffectingWordWrapping = true; /* hasFontScaleChanged = true; */ m_fontSize = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// The amount of additional spacing between characters.
        /// </summary>
        public float characterSpacing
        {
            get { return m_characterSpacing; }
            set { if (m_characterSpacing != value) { havePropertiesChanged = true; isAffectingWordWrapping = true; m_characterSpacing = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// Rectangle which defines the region where the text lives.
        /// </summary>
        public Vector4 textRectangle
        {
            get { return m_textRectangle; }
            set { m_textRectangle = value; havePropertiesChanged = true; isAffectingWordWrapping = true; }
        }


        /// <summary>
        /// Enables or Disables Rich Text Tags
        /// </summary>
        public bool richText
        {
            get { return m_isRichText; }
            set { m_isRichText = value; havePropertiesChanged = true; isAffectingWordWrapping = true; isInputParsingRequired = true; }
        }


        /// <summary>
        /// Determines where word wrap will occur.
        /// </summary>
        public float lineLength
        {
            get { return m_lineLength; }
            set { if (m_lineLength != value) { havePropertiesChanged = true; isAffectingWordWrapping = true; m_lineLength = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// Contains the bounds of the text object.
        /// </summary>
        public Bounds bounds
        {
            get { if (m_mesh != null) return m_mesh.bounds; return new Bounds(); }
            //set { if (_meshExtents != value) havePropertiesChanged = true; _meshExtents = value; }
        }

        /// <summary>
        /// The amount of additional spacing to add between each lines of text.
        /// </summary>
        public float lineSpacing
        {
            get { return m_lineSpacing; }
            set { if (m_lineSpacing != value) { havePropertiesChanged = true; m_lineSpacing = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// Determines the anchor position of the text object.  
        /// </summary>
        public AnchorPositions anchor
        {
            get { return m_anchor; }
            set { if (m_anchor != value) { havePropertiesChanged = true; m_anchor = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// Line justification options.
        /// </summary>
        public AlignmentTypes alignment
        {
            get { return m_lineJustification; }
            set { if (m_lineJustification != value) { havePropertiesChanged = true; m_lineJustification = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// Determines if kerning is enabled or disabled.
        /// </summary>
        public bool enableKerning
        {
            get { return m_enableKerning; }
            set { if (m_enableKerning != value) { havePropertiesChanged = true; isAffectingWordWrapping = true; m_enableKerning = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// Anchor dampening prevents the anchor position from being adjusted unless the positional change exceeds about 40% of the width of the underline character. This essentially stabilizes the anchor position.
        /// </summary>
        public bool anchorDampening
        {
            get { return m_anchorDampening; }
            set { if (m_anchorDampening != value) { havePropertiesChanged = true; m_anchorDampening = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// This overrides the color tags forcing the vertex colors to be the default font color.
        /// </summary>
        public bool overrideColorTags
        {
            get { return m_overrideHtmlColors; }
            set { if (m_overrideHtmlColors != value) { havePropertiesChanged = true; m_overrideHtmlColors = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// Adds extra padding around each character. This may be necessary when the displayed text is very small to prevent clipping.
        /// </summary>
        public bool extraPadding
        {
            get { return m_enableExtraPadding; }
            set { if (m_enableExtraPadding != value) { havePropertiesChanged = true; checkPaddingRequired = true; isAffectingWordWrapping = true; m_enableExtraPadding = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// Controls whether or not word wrapping is applied. When disabled, the text will be displayed on a single line.
        /// </summary>
        public bool enableWordWrapping
        {
            get { return m_enableWordWrapping; }
            set { if (m_enableWordWrapping != value) { havePropertiesChanged = true; isAffectingWordWrapping = true; isInputParsingRequired = true; m_enableWordWrapping = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// Controls how the face and outline textures will be applied to the text object.
        /// </summary>
        public TextureMappingOptions horizontalMapping
        {
            get { return m_horizontalMapping; }
            set { if (m_horizontalMapping != value) { havePropertiesChanged = true; m_horizontalMapping = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// Controls how the face and outline textures will be applied to the text object.
        /// </summary>
        public TextureMappingOptions verticalMapping
        {
            get { return m_verticalMapping; }
            set { if (m_verticalMapping != value) { havePropertiesChanged = true; m_verticalMapping = value; /* ScheduleUpdate(); */ } }
        }

        /// <summary>
        /// Forces objects that are not visible to get refreshed.
        /// </summary>
        public bool ignoreVisibility
        {
            get { return m_ignoreCulling; }
            set { if (m_ignoreCulling != value) { havePropertiesChanged = true; m_ignoreCulling = value; /* ScheduleUpdate(); */ } }
        }


        /// <summary>
        /// Sets Perspective Correction to Zero for Orthographic Camera mode & 0.875f for Perspective Camera mode.
        /// </summary>
        public bool isOrthographic
        {
            get { return m_isOrthographic; }
            set { havePropertiesChanged = true; m_isOrthographic = value; /* ScheduleUpdate(); */ }
        }



        /// <summary>
        /// Sets the Renderer's sorting Layer ID
        /// </summary>
        public int sortingLayerID
        {
            get { return m_sortingLayerID; }
            set { m_sortingLayerID = value; m_renderer.sortingLayerID = value; }
        }


        /// <summary>
        /// Sets the Renderer's sorting order within the assigned layer.
        /// </summary>
        public int sortingOrder
        {
            get { return m_sortingOrder; }
            set { m_sortingOrder = value; m_renderer.sortingOrder = value; }
        }


        public bool isAdvancedLayoutComponentPresent
        {
            //get { return m_isAdvanceLayoutComponentPresent; }
            set
            {
                if (m_isAdvanceLayoutComponentPresent != value)
                {
                    m_advancedLayoutComponent = value == true ? GetComponent<TMPro_AdvancedLayout>() : null;
                    havePropertiesChanged = true;
                    m_isAdvanceLayoutComponentPresent = value;
                }
            }
        }


        /// <summary>
        /// Allows to control how many characters are visible from the input. Non-visible character are set to fully transparent.
        /// </summary>
        public int maxVisibleCharacters
        {
            get { return m_maxVisibleCharacters; }
            set { if (m_maxVisibleCharacters != value) { havePropertiesChanged = true; m_maxVisibleCharacters = value; } }
        }

        /// <summary>
        /// Allows control over how many lines of text are displayed.
        /// </summary>
        public int maxVisibleLines
        {
            get { return m_maxVisibleLines; }
            set { if (m_maxVisibleLines != value) { havePropertiesChanged = true; isInputParsingRequired = true; m_maxVisibleLines = value; } }
        }


        //public TMPro_TextMetrics metrics
        //{
        //    get { return m_textMetrics; }
        //}


        //public int characterCount
        //{
        //    get { return m_textInfo.characterCount; }
        //}

        //public int lineCount
        //{
        //    get { return m_textInfo.lineCount; }
        //}


        public Vector2[] spacePositions
        {
            get { return m_spacePositions; }
        }


        /*
        // MASKING RELATED PROPERTIES
        /// <summary>
        /// Sets the mask type 
        /// </summary>
        public MaskingTypes mask
        {
            get { return m_mask; }
            set { m_mask = value; havePropertiesChanged = true; isMaskUpdateRequired = true; }
        }

        /// <summary>
        /// Set the masking offset mode (as percentage or pixels)
        /// </summary>
        public MaskingOffsetMode maskOffsetMode
        {
            get { return m_maskOffsetMode; }
            set { m_maskOffsetMode = value; havePropertiesChanged = true; isMaskUpdateRequired = true; }
        }

        /// <summary>
        /// Sets the masking offset from the bounds of the object
        /// </summary>
        public Vector4 maskOffset
        {
            get { return m_maskOffset; }
            set { m_maskOffset = value;  havePropertiesChanged = true; isMaskUpdateRequired = true; }
        }

        /// <summary>
        /// Sets the softness of the mask
        /// </summary>
        public Vector2 maskSoftness
        {
            get { return m_maskSoftness; }
            set { m_maskSoftness = value; havePropertiesChanged = true; isMaskUpdateRequired = true; }
        }

        /// <summary>
        /// Allows to move / offset the mesh vertices by a set amount
        /// </summary>
        public Vector2 vertexOffset
        {
            get { return m_vertexOffset; }
            set { m_vertexOffset = value; havePropertiesChanged = true; isMaskUpdateRequired = true; }
        }
        */

        public TextInfo textInfo
        {
            get { return m_textInfo; }
        }


        public TMPro_MeshInfo meshInfo
        {
            get { return m_meshInfo; }
        }

        public Mesh mesh
        {
            get { return m_mesh; }
        }


        public TMPro_CharacterInfo[] characterInfo
        {
            get { return m_textInfo.characterInfo; }

        }

  

        /// <summary>
        /// Function to be used to force recomputing of character padding when Shader / Material properties have been changed via script.
        /// </summary>
        public void UpdateMeshPadding()
        {
            m_padding = ShaderUtilities.GetPadding(m_renderer.sharedMaterials, m_enableExtraPadding, m_isUsingBold);
            havePropertiesChanged = true;
            /* ScheduleUpdate(); */
        }


        /// <summary>
        /// Function to force regeneration of the mesh before its normal process time. This is useful when changes to the text object properties need to be applied immediately.
        /// </summary>
        public void ForceMeshUpdate()
        {
            havePropertiesChanged = true;
            OnWillRenderObject();            
        }


        public void UpdateFontAsset()
        {           
            LoadFontAsset();
        }


        /// <summary>
        /// Function used to evaluate the length of a text string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public TextInfo GetTextInfo(string text)
        {
            TextInfo temp_textInfo = new TextInfo();           

            // Early exit if no font asset was assigned. This should not be needed since Arial SDF will be assigned by default.
            if (m_fontAsset.characterDictionary == null)
            {
                Debug.Log("Can't Generate Mesh! No Font Asset has been assigned to Object ID: " + this.GetInstanceID());
                return null;
            }

            // Early exit if string is empty.          
            if (text == null || text.Length == 0)
            {
                return null;
            }
                 
            // Convert String to Char[]
            StringToCharArray(text, ref m_text_buffer);

            int size = GetArraySizes(m_text_buffer);
            temp_textInfo.characterInfo = new TMPro_CharacterInfo[size];          

              
            m_fontIndex = 0;
            m_fontAssetArray[m_fontIndex] = m_fontAsset;
            // Scale the font to approximately match the point size           
            m_fontScale = (m_fontSize / m_fontAssetArray[m_fontIndex].fontInfo.PointSize * (m_isOrthographic ? 1 : 0.1f));
            float baseScale = m_fontScale; // BaseScale keeps the character aligned vertically since <size=+000> results in font of different scale.

            int charCode = 0; // Holds the character code of the currently being processed character.  
            int prev_charCode = 0;
            //bool isMissingCharacter; // Used to handle missing characters in the Font Atlas / Definition.

            m_style = FontStyles.Normal; // Set defaul style as normal.

            // GetPadding to adjust the size of the mesh due to border thickness, softness, glow, etc...
            if (checkPaddingRequired)
            {
                checkPaddingRequired = false;
                m_padding = ShaderUtilities.GetPadding(m_renderer.sharedMaterials, m_enableExtraPadding, m_isUsingBold);
                m_alignmentPadding = ShaderUtilities.GetFontExtent(m_sharedMaterial);
            }

            float style_padding = 0; // Extra padding required to accomodate Bold style.
            float xadvance_multiplier = 1; // Used to increase spacing between character when style is bold.

            m_baselineOffset = 0; // Used by subscript characters.
        
            float lineOffset = 0; // Amount of space between lines (font line spacing + m_linespacing).
            m_xAdvance = 0; // Used to track the position of each character.         

            int lineNumber = 0;
            int wordCount = 0;
            int character_Count = 0; // Total characters in the char[]
            int visibleCharacter_Count = 0; // # of visible characters.

            // Limit Line Length to whatever size fits all characters on a single line.
            m_lineLength = m_lineLength > max_LineWrapLength ? max_LineWrapLength : m_lineLength;

            // Initialize struct to track states of word wrapping
            m_SaveWordWrapState = new WordWrapState();
            int wrappingIndex = 0;


            if (temp_textInfo.lineInfo == null) temp_textInfo.lineInfo = new LineInfo[8];
            for (int i = 0; i < temp_textInfo.lineInfo.Length; i++)
            {              
                temp_textInfo.lineInfo[i] = new LineInfo();
                //m_textInfo.lineInfo[i].lineExtents = new Extents(k_InfinityVector, -k_InfinityVector);
            }

           
            // Tracking of the highest Ascender
            float maxAscender = 0;
            float maxDescender = 0;
           
            int lastLineNumber = 0;

            int endTagIndex = 0;
            // Parse through Character buffer to read html tags and begin creating mesh.
            for (int i = 0; m_text_buffer[i] != 0; i++)
            {
                m_tabSpacing = -999;
                m_spacing = -999;
                charCode = m_text_buffer[i];
            
                if (m_isRichText && charCode == 60)  // '<'
                {
                    // Check if Tag is valid. If valid, skip to the end of the validated tag.
                    if (ValidateHtmlTag(m_text_buffer, i + 1, out endTagIndex))
                    {
                        i = endTagIndex;
                        if (m_tabSpacing != -999)
                        {
                            // Move character to a fix position. Position expresses in characters (approximation).
                            m_xAdvance = m_tabSpacing * m_cached_Underline_GlyphInfo.width * m_fontScale;
                        }

                        if (m_spacing != -999)
                        {
                            m_xAdvance += m_spacing * m_fontScale * m_cached_Underline_GlyphInfo.width;
                        }

                        continue;
                    }
                }

                //isMissingCharacter = false;

                // Look up Character Data from Dictionary and cache it.
                m_fontAssetArray[m_fontIndex].characterDictionary.TryGetValue(charCode, out m_cached_GlyphInfo);
                if (m_cached_GlyphInfo == null)
                {
                    // Character wasn't found in the Dictionary.
                    m_fontAssetArray[m_fontIndex].characterDictionary.TryGetValue(88, out m_cached_GlyphInfo);
                    if (m_cached_GlyphInfo != null)
                    {
                        Debug.LogWarning("Character with ASCII value of " + charCode + " was not found in the Font Asset Glyph Table.");
                        // Replace the missing character by X (if it is found)
                        charCode = 88;
                        //isMissingCharacter = true;
                    }
                    else
                    {  // At this point the character isn't in the Dictionary, the replacement X isn't either so ... 
                        //continue;
                    }
                }


                // Store some of the text object's information
                temp_textInfo.characterInfo[character_Count].character = (char)charCode;
                //temp_textInfo.characterInfo[character_Count].color = m_htmlColor;
                //temp_textInfo.characterInfo[character_Count].style = m_style;
                temp_textInfo.characterInfo[character_Count].index = (short)i;


                // Handle Kerning if Enabled.                 
                if (m_enableKerning && character_Count >= 1)
                {
                    KerningPairKey keyValue = new KerningPairKey(prev_charCode, charCode);

                    KerningPair pair;

                    m_fontAsset.kerningDictionary.TryGetValue(keyValue.key, out pair);
                    if (pair != null)
                    {
                        m_xAdvance += pair.XadvanceOffset * m_fontScale;
                    }
                }



                // Set Padding based on selected font style
                if ((m_style & FontStyles.Bold) == FontStyles.Bold) // Checks for any combination of Bold Style.
                {
                    style_padding = m_fontAsset.BoldStyle * 2;
                    xadvance_multiplier = 1.07f; // Increase xAdvance for bold characters.         
                }
                else
                {
                    style_padding = m_fontAsset.NormalStyle * 2;
                    xadvance_multiplier = 1.0f;
                }


                // Setup Vertices for each character.
                Vector3 top_left = new Vector3(0 + m_xAdvance + ((m_cached_GlyphInfo.xOffset - m_padding - style_padding) * m_fontScale), (m_cached_GlyphInfo.yOffset + m_baselineOffset + m_padding) * m_fontScale - lineOffset * baseScale, 0);
                Vector3 bottom_left = new Vector3(top_left.x, top_left.y - ((m_cached_GlyphInfo.height + m_padding * 2) * m_fontScale), 0);
                Vector3 top_right = new Vector3(bottom_left.x + ((m_cached_GlyphInfo.width + m_padding * 2 + style_padding * 2) * m_fontScale), top_left.y, 0);
                Vector3 bottom_right = new Vector3(top_right.x, bottom_left.y, 0);


                // Check if we need to Shear the rectangles for Italic styles
                if ((m_style & FontStyles.Italic) == FontStyles.Italic)
                {
                    // Shift Top vertices forward by half (Shear Value * height of character) and Bottom vertices back by same amount. 
                    float shear_value = m_fontAsset.ItalicStyle * 0.01f;
                    Vector3 topShear = new Vector3(shear_value * ((m_cached_GlyphInfo.yOffset + m_padding + style_padding) * m_fontScale), 0, 0);
                    Vector3 bottomShear = new Vector3(shear_value * (((m_cached_GlyphInfo.yOffset - m_cached_GlyphInfo.height - m_padding - style_padding)) * m_fontScale), 0, 0);

                    top_left = top_left + topShear;
                    bottom_left = bottom_left + bottomShear;
                    top_right = top_right + topShear;
                    bottom_right = bottom_right + bottomShear;
                }


                // Track Word Count per line and for the object
                if (character_Count > 0 && (char.IsWhiteSpace((char)charCode) || char.IsPunctuation((char)charCode)))
                {
                    if (char.IsLetterOrDigit(temp_textInfo.characterInfo[character_Count - 1].character))
                    {
                        wordCount += 1;
                        temp_textInfo.lineInfo[lineNumber].wordCount += 1;
                    }
                }


                // Setup Mesh for visible characters. ie. not a SPACE / LINEFEED / CARRIAGE RETURN.
                if (charCode != 32 && charCode != 9 && charCode != 10 && charCode != 13)
                {
                    // Determine the bounds of the Mesh.             
                    //meshExtents.min = new Vector2(Mathf.Min(meshExtents.min.x, bottom_left.x), Mathf.Min(meshExtents.min.y, bottom_left.y));
                    //meshExtents.max = new Vector2(Mathf.Max(meshExtents.max.x, top_right.x), Mathf.Max(meshExtents.max.y, top_left.y));

                    // Determine the extend of each line
                    LineInfo lineInfo = temp_textInfo.lineInfo[lineNumber];
                    Extents lineExtents = lineInfo.lineExtents;
                    temp_textInfo.lineInfo[lineNumber].lineExtents.min = new Vector2(Mathf.Min(lineExtents.min.x, bottom_left.x), Mathf.Min(lineExtents.min.y, bottom_left.y));
                    temp_textInfo.lineInfo[lineNumber].lineExtents.max = new Vector2(Mathf.Max(lineExtents.max.x, top_right.x), Mathf.Max(lineExtents.max.y, top_left.y));


                    if (m_enableWordWrapping && top_right.x > m_lineLength)
                    {


                        // Check if further wrapping is possible or if we need to increase the line length
                        if (wrappingIndex == m_SaveWordWrapState.previous_WordBreak)
                        {
                            if (isAffectingWordWrapping)
                            {
                                m_lineLength = Mathf.Round(top_right.x * 100 + 0.5f) / 100f;//m_lineLength = top_right.x;
                                GenerateTextMesh();
                                isAffectingWordWrapping = false;
                            }

                            Debug.Log("Line " + lineNumber + " Cannot wrap lines anymore.");
                            return null;
                        }

                        // Restore to previously stored state
                        character_Count = m_SaveWordWrapState.total_CharacterCount + 1;
                        visibleCharacter_Count = m_SaveWordWrapState.visible_CharacterCount;
                        m_textInfo.lineInfo[lineNumber] = m_SaveWordWrapState.lineInfo;

                        m_htmlColor = m_SaveWordWrapState.vertexColor;
                        m_style = m_SaveWordWrapState.fontStyle;
                        m_baselineOffset = m_SaveWordWrapState.baselineOffset;
                        m_fontScale = m_SaveWordWrapState.fontScale;
                        i = m_SaveWordWrapState.previous_WordBreak;
                        wrappingIndex = i;  // Used to dectect when line length can no longer be reduced.

                        lineNumber += 1;
                        // Check to make sure Array is large enough to hold a new line.
                        if (lineNumber >= temp_textInfo.lineInfo.Length)
                            Array.Resize(ref temp_textInfo.lineInfo, Mathf.NextPowerOfTwo(lineNumber));


                        lineOffset += (m_fontAssetArray[m_fontIndex].fontInfo.LineHeight + m_lineSpacing);
                        m_xAdvance = 0;
                        continue;
                    }

                    //visibleCharacter_Count += 1;
                }
                else
                {   // This is a Space, Tab, LineFeed or Carriage Return              
                    
                    // Track # of spaces per line which is used for line justification.
                    if (charCode == 9 || charCode == 32)
                    {
                        //m_lineExtents[lineNumber].spaceCount += 1;
                        temp_textInfo.lineInfo[lineNumber].spaceCount += 1;
                        temp_textInfo.spaceCount += 1;
                    }

                    // We store the state of numerous variables for the most recent Space, LineFeed or Carriage Return to enable them to be restored 
                    // for Word Wrapping.
                    m_SaveWordWrapState.previous_WordBreak = i;
                    m_SaveWordWrapState.total_CharacterCount = character_Count;
                    m_SaveWordWrapState.visible_CharacterCount = visibleCharacter_Count;
                    m_SaveWordWrapState.maxLineLength = m_xAdvance;
                    m_SaveWordWrapState.fontScale = m_fontScale;
                    m_SaveWordWrapState.baselineOffset = m_baselineOffset;
                    m_SaveWordWrapState.fontStyle = m_style;
                    m_SaveWordWrapState.vertexColor = m_htmlColor;                                 
                    m_SaveWordWrapState.lineInfo = temp_textInfo.lineInfo[lineNumber];                    
                }


                // Store Rectangle positions for each Character.
                temp_textInfo.characterInfo[character_Count].bottomLeft = bottom_left;
                temp_textInfo.characterInfo[character_Count].topRight = top_right;
                temp_textInfo.characterInfo[character_Count].lineNumber = (short)lineNumber;

                //temp_textInfo.characterInfo[character_Count].baseLine = top_right.y - (m_cached_GlyphInfo.yOffset + m_padding) * m_fontScale;
                //temp_textInfo.characterInfo[character_Count].topLine = temp_textInfo.characterInfo[character_Count].baseLine + (m_fontAssetArray[m_fontIndex].fontInfo.Ascender + m_alignmentPadding.y) * m_fontScale; // Ascender              
                //temp_textInfo.characterInfo[character_Count].bottomLine = temp_textInfo.characterInfo[character_Count].baseLine + (m_fontAssetArray[m_fontIndex].fontInfo.Descender - m_alignmentPadding.w) * m_fontScale; // Descender          

                maxAscender = temp_textInfo.characterInfo[character_Count].topLine > maxAscender ? temp_textInfo.characterInfo[character_Count].topLine : maxAscender;
                maxDescender = temp_textInfo.characterInfo[character_Count].bottomLine < maxDescender ? temp_textInfo.characterInfo[character_Count].bottomLine : maxDescender;

                //temp_textInfo.characterInfo[character_Count].aspectRatio = m_cached_GlyphInfo.width / m_cached_GlyphInfo.height;
                //temp_textInfo.characterInfo[character_Count].scale = m_fontScale;
                temp_textInfo.lineInfo[lineNumber].characterCount += 1;

                //Debug.Log("Character #" + i + " is [" + (char)charCode + "] ASCII (" + charCode + ")");

                // Store LineInfo 
                if (lineNumber != lastLineNumber)
                {
                    temp_textInfo.lineInfo[lineNumber].firstCharacterIndex = character_Count;
                    temp_textInfo.lineInfo[lineNumber - 1].lastCharacterIndex = character_Count - 1;
                    temp_textInfo.lineInfo[lineNumber - 1].characterCount = temp_textInfo.lineInfo[lineNumber - 1].lastCharacterIndex - temp_textInfo.lineInfo[lineNumber - 1].firstCharacterIndex + 1;
                    temp_textInfo.lineInfo[lineNumber - 1].lineLength = temp_textInfo.characterInfo[character_Count - 1].topRight.x - m_padding * m_fontScale;
                }

              
                           
                lastLineNumber = lineNumber;

                // Handle Tabulation Stops. Tab stops at every 25% of Font Size.
                if (charCode == 9)
                {
                    m_xAdvance = (int)(m_xAdvance / (m_fontSize * 0.25f) + 1) * (m_fontSize * 0.25f);
                }
                else
                    m_xAdvance += (m_cached_GlyphInfo.xAdvance * xadvance_multiplier * m_fontScale) + m_characterSpacing;


                // Handle Line Feed as well as Word Wrapping
                if (charCode == 10 || charCode == 13)
                {
                    lineNumber += 1;

                    // Check to make sure Array is large enough to hold a new line.
                    if (lineNumber >= temp_textInfo.lineInfo.Length)                    
                        Array.Resize(ref temp_textInfo.lineInfo, Mathf.NextPowerOfTwo(lineNumber));                     
                    
                    
                    lineOffset += (m_fontAssetArray[m_fontIndex].fontInfo.LineHeight + m_lineSpacing);
                    m_xAdvance = 0;
                }

                character_Count += 1;
                prev_charCode = charCode;
            }


            temp_textInfo.lineInfo[lineNumber].lastCharacterIndex = character_Count - 1;
            temp_textInfo.lineInfo[lineNumber].characterCount = temp_textInfo.lineInfo[lineNumber].lastCharacterIndex - temp_textInfo.lineInfo[lineNumber].firstCharacterIndex + 1;
            temp_textInfo.lineInfo[lineNumber].lineLength = temp_textInfo.characterInfo[character_Count - 1].topRight.x - m_padding * m_fontScale;
     
            //m_textMetrics = new TMPro_TextMetrics();
            temp_textInfo.characterCount = character_Count;
            temp_textInfo.lineCount = lineNumber + 1;
            temp_textInfo.wordCount = wordCount;

            //for (int i = 0; i < lineNumber + 1; i++)
            //{
            //    Debug.Log("Line: " + (i + 1) + "  # Char: " + temp_textInfo.lineInfo[i].characterCount
            //                                 + "  Word Count: " + temp_textInfo.lineInfo[i].wordCount
            //                                 + "  Space: " + temp_textInfo.lineInfo[i].spaceCount
            //                                 + "  First:" + temp_textInfo.lineInfo[i].firstCharacterIndex + "  Last [" + temp_textInfo.characterInfo[temp_textInfo.lineInfo[i].lastCharacterIndex].character
            //                                 + "] at Index: " + temp_textInfo.lineInfo[i].lastCharacterIndex + "  Length: " + temp_textInfo.lineInfo[i].lineLength 
            //                                 + " Line Extents: " + temp_textInfo.lineInfo[i].lineExtents);

            //    //Debug.Log("line: " + (i + 1) + "  m_lineExtents Count: " + m_lineExtents[i].characterCount + "  lineInfo: " + m_textInfo.lineInfo[i].characterCount);
            //    //Debug.DrawLine(new Vector3(m_textInfo.lineInfo[i].lineLength, 2, 0), new Vector3(m_textInfo.lineInfo[i].lineLength, -2, 0), Color.red, 30f);
            //}

            return temp_textInfo;
        }


        //public Vector2[] SetTextWithSpaces(string text, int numPositions)
        //{
        //    m_spacePositions = new Vector2[numPositions];

        //    this.text = text;

        //    return m_spacePositions;
        //}


        /// <summary>
        /// <para>Formatted string containing a pattern and a value representing the text to be rendered.</para>
        /// <para>ex. TextMeshPro.SetText ("Number is {0:1}.", 5.56f);</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text">String containing the pattern."</param>
        /// <param name="arg0">Value is a float.</param>
        public void SetText (string text, float arg0)
        {
            SetText(text, arg0, 255, 255);
        }

        /// <summary>
        /// <para>Formatted string containing a pattern and a value representing the text to be rendered.</para>
        /// <para>ex. TextMeshPro.SetText ("First number is {0} and second is {1:2}.", 10, 5.756f);</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text">String containing the pattern."</param>
        /// <param name="arg0">Value is a float.</param>
        /// <param name="arg1">Value is a float.</param>
        public void SetText (string text, float arg0, float arg1)            
        {
            SetText(text, arg0, arg1, 255);
        }

        /// <summary>
        /// <para>Formatted string containing a pattern and a value representing the text to be rendered.</para>
        /// <para>ex. TextMeshPro.SetText ("A = {0}, B = {1} and C = {2}.", 2, 5, 7);</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text">String containing the pattern."</param>
        /// <param name="arg0">Value is a float.</param>
        /// <param name="arg1">Value is a float.</param>
        /// <param name="arg2">Value is a float.</param>
        public void SetText (string text, float arg0, float arg1, float arg2)        
        {
            // Early out if nothing has been changed from previous invocation.
            if (text == old_text && arg0 == old_arg0 && arg1 == old_arg1 && arg2 == old_arg2)
            {
                return;
            }

            old_text = text;
            old_arg1 = 255;
            old_arg2 = 255;

            int decimalPrecision = 0;
            int index = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == 123) // '{'
                {
                    // Check if user is requesting some decimal precision. Format is {0:2}
                    if (text[i + 2] == 58) // ':'
                    {
                        decimalPrecision = text[i + 3] - 48;
                    }

                    switch (text[i + 1] - 48)
                    {
                        case 0: // 1st Arg                        
                            old_arg0 = arg0;
                            AddFloatToCharArray(arg0, ref index, decimalPrecision);
                            break;                        
                        case 1: // 2nd Arg
                            old_arg1 = arg1;
                            AddFloatToCharArray(arg1, ref index, decimalPrecision);
                            break;                       
                        case 2: // 3rd Arg
                            old_arg2 = arg2;
                            AddFloatToCharArray(arg2, ref index, decimalPrecision);
                            break;                       
                    }

                    if (text[i + 2] == 58)
                        i += 4;
                    else
                        i += 2;

                    continue;
                }
                m_input_CharArray[index] = c;
                index += 1;
            }

            m_input_CharArray[index] = (char)0;
            m_charArray_Length = index; // Set the length to where this '0' termination is.

#if UNITY_EDITOR
            // Create new string to be displayed in the Input Text Box of the Editor Panel.
            m_text = new string(m_input_CharArray, 0, index);           
#endif

            m_inputSource = TextInputSources.SetText;
            isInputParsingRequired = true;
            havePropertiesChanged = true;
            /* ScheduleUpdate(); */
        }




        /// <summary>
        /// Character array containing the text to be displayed.
        /// </summary>
        /// <param name="charArray"></param>
        public void SetCharArray(char[] charArray)
        {
            if (charArray == null || charArray.Length == 0)
                return;

            // Check to make sure chars_buffer is large enough to hold the content of the string.
            if (m_char_buffer.Length <= charArray.Length)
            {
                int newSize = Mathf.NextPowerOfTwo(charArray.Length + 1);
                m_char_buffer = new int[newSize];
            }

            int index = 0;

            for (int i = 0; i < charArray.Length; i++)
            {
                if (charArray[i] == 92 && i < charArray.Length - 1)
                {
                    switch ((int)charArray[i + 1])
                    {
                        case 116: // \t Tab
                            m_char_buffer[index] = (char)9;
                            i += 1;
                            index += 1;
                            continue;
                        case 110: // \n LineFeed
                            m_char_buffer[index] = (char)10;
                            i += 1;
                            index += 1;
                            continue;
                    }
                }

                m_char_buffer[index] = charArray[i];
                index += 1;
            }
            m_char_buffer[index] = (char)0;

            m_inputSource = TextInputSources.SetCharArray;
            havePropertiesChanged = true;
            isInputParsingRequired = true;
        }

    }
}