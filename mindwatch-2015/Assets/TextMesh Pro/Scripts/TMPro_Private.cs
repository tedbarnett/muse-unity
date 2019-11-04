// Copyright (C) 2014 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



namespace TMPro
{  

    public partial class TextMeshPro : MonoBehaviour
    {

        [SerializeField]
        private string m_text;

        [SerializeField]
        private TextMeshProFont m_fontAsset;

        private Material m_fontMaterial;

        private Material m_sharedMaterial;

        private bool m_isOverlay = false;

        [SerializeField]
        private Color32 m_fontColor = Color.white;

        [SerializeField]
        private Color32 m_faceColor = Color.white;

        [SerializeField]
        private Color32 m_outlineColor = Color.black;

        private float m_outlineWidth = 0.0f;

        [SerializeField]
        private float m_fontSize = 36;
        private float m_currentFontSize; // Temporary Font Size affected by tags

        [SerializeField]
        private float m_characterSpacing = 0;

        [SerializeField]
        private float m_lineLength = 72;

        [SerializeField]
        private Vector4 m_textRectangle;

        [SerializeField]
        private float m_lineSpacing = 0;

        [SerializeField]
        private AnchorPositions m_anchor = AnchorPositions.TopLeft;

        [SerializeField]
        private AlignmentTypes m_lineJustification = AlignmentTypes.Left;

        [SerializeField]
        private bool m_enableKerning = false;

        private bool m_anchorDampening = false;

        [SerializeField]
        private bool m_overrideHtmlColors = false;

        [SerializeField]
        private bool m_enableExtraPadding = false;
        [SerializeField]
        private bool checkPaddingRequired;

        [SerializeField]
        private bool m_enableWordWrapping = false;
        [SerializeField]
        private float m_wordWrappingRatios = 0.4f; // Controls word wrapping ratios between word or characters.

        [SerializeField]
        private TextureMappingOptions m_horizontalMapping = TextureMappingOptions.Character;

        [SerializeField]
        private TextureMappingOptions m_verticalMapping = TextureMappingOptions.Character;


        [SerializeField]
        private bool isAffectingWordWrapping = false; // Used internally to control which properties affect word wrapping.

        [SerializeField]
        private bool isInputParsingRequired = false; // Used to determine if the input text needs to be reparsed.

        [SerializeField]
        private bool havePropertiesChanged;  // Used to track when properties of the text object have changed.

        [SerializeField]
        private bool hasFontAssetChanged = false; // Used to track when font properties have changed.

        [SerializeField]
        private bool m_isRichText = true; // Used to enable or disable Rich Text.


        private enum TextInputSources { Text = 0, SetText = 1, SetCharArray = 2 };
        [SerializeField]
        private TextInputSources m_inputSource;
        private string old_text; // Used by SetText to determine if the text has changed.
        private float old_arg0, old_arg1, old_arg2; // Used by SetText to determine if the args have changed.

        private int m_fontIndex;

        private float m_fontScale; // Scaling of the font based on Atlas true Font Size and Rendered Font Size.  
        private bool m_isRecalculateScaleRequired = false;

        private Vector3 m_lossyScale; // Used for Tracking lossy scale changes in the transform;
        private float m_xAdvance; // Tracks x advancement from character to character.
        //private bool m_isCheckingTextLength = false;
        //private float m_textLength;
        private int[] m_text_buffer = new int[8];

        private float max_LineWrapLength = 999;

        private Vector3 m_anchorOffset; // The amount of offset to be applied to the vertices. 


        private TextInfo m_textInfo; // Class which holds information about the Text object such as characters, lines, mesh data as well as metrics.
        //private TMPro_CharacterInfo[] m_characters_Info; // Data structre that contains information about the text object and characters contained in it.
        //private TMPro_TextMetrics m_textMetrics;
     

        private char[] m_htmlTag = new char[16]; // Maximum length of rich text tag. This is pre-allocated to avoid GC.


        [SerializeField]
        private Renderer m_renderer;
        private MeshFilter m_meshFilter;
        private Mesh m_mesh;
        private Transform m_transform;


        private Color32 m_htmlColor = new Color(255, 255, 255, 128);
        private FontStyles m_style = FontStyles.Normal;
        private float m_tabSpacing = 0;
        private float m_spacing = 0;
        private Vector2[] m_spacePositions = new Vector2[8];

        private float m_baselineOffset; // Used for superscript and subscript.
        private float m_padding = 0; // Holds the amount of padding required to display the mesh correctly as a result of dilation, outline thickness, softness and similar material properties.
        private Vector4 m_alignmentPadding; // Holds the amount of padding required to account for Outline Width and Dilation with regards to text alignment.
        private bool m_isUsingBold = false; // Used to ensure GetPadding & Ratios take into consideration bold characters.

        private Vector2 k_InfinityVector = new Vector2(Mathf.Infinity, Mathf.Infinity);

        private bool m_isFirstAllocation; // Flag to determine if this is the first allocation of the buffers.
        private int m_max_characters = 8; // Determines the initial allocation and size of the character array / buffer.
        private int m_max_numberOfLines = 4; // Determines the initial allocation and maximum number of lines of text. 

        private int[] m_char_buffer; // This array holds the characters to be processed by GenerateMesh();
        private char[] m_input_CharArray = new char[256]; // This array hold the characters from the SetText();
        private int m_charArray_Length = 0;

        //private Mesh_Extents[] m_lineExtents; // Struct that holds information about each line which is used for UV Mapping.

        //private IFormatProvider NumberFormat = System.Globalization.NumberFormatInfo.CurrentInfo; // Speeds up accessing this interface.
        private readonly float[] k_Power = { 5e-1f, 5e-2f, 5e-3f, 5e-4f, 5e-5f, 5e-6f, 5e-7f, 5e-8f, 5e-9f, 5e-10f }; // Used by FormatText to enable rounding and avoid using Mathf.Pow.

        private GlyphInfo m_cached_GlyphInfo; // Glyph / Character information is cached into this variable which is faster than having to fetch from the Dictionary multiple times.
        private GlyphInfo m_cached_Underline_GlyphInfo; // Same as above but for the underline character which is used for Underline.

        private WordWrapState m_SaveWordWrapState = new WordWrapState(); // Struct which holds various states / variables used in conjunction with word wrapping.

        // Mesh Declaration 
        private Vector3[] m_vertices;
        private Vector3[] m_normals;
        private Vector4[] m_tangents;
        private Vector2[] m_uvs;
        private Vector2[] m_uv2s;
        private Color32[] m_vertColors;
        private int[] m_triangles;

        //private Camera m_sceneCamera;
        private Bounds m_default_bounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 0));

        [SerializeField]
        private bool m_ignoreCulling = true; // Not implemented yet.
        [SerializeField]
        private bool m_isOrthographic = false;

        [SerializeField]
        private int m_sortingLayerID;
        [SerializeField]
        private int m_sortingOrder;


        //Special Cases
        private int m_maxVisibleCharacters = -1;
        private int m_maxVisibleLines = -1;
        //private int m_safetyCount = 0;

        // Multi Material & Font Handling
        // Forced to use a class until 4.5 as Structs do not serialize. 
        //private class TriangleList
        //{
        //    public int[] triangleIndex;
        //}

        //private TriangleList[] m_triangleListArray = new TriangleList[16];
        [SerializeField]
        private TextMeshProFont[] m_fontAssetArray;
        //[SerializeField]
        private List<Material> m_sharedMaterials = new List<Material>(16);
        private int m_selectedFontAsset;

        // MASKING RELATED PROPERTIES
        /*
        [SerializeField]
        private bool isMaskUpdateRequired;      
        [SerializeField]
        private MaskingTypes m_mask;
        [SerializeField]
        private MaskingOffsetMode m_maskOffsetMode;
        [SerializeField]
        private Vector4 m_maskOffset;
        [SerializeField]
        private Vector2 m_maskSoftness;
        [SerializeField]
        private Vector2 m_vertexOffset;
        */

        // ADVANCED LAYOUT COMPONENT ** Still work in progress
        private TMPro_AdvancedLayout m_advancedLayoutComponent;
        private bool m_isAdvanceLayoutComponentPresent;
        private TMPro_MeshInfo m_meshInfo;


        // ** Still needs to be implemented **
        //private Camera managerCamera;
        //private TMPro_UpdateManager m_updateManager;
        //private bool isAlreadyScheduled;

        // DEBUG Variables
        private System.Diagnostics.Stopwatch m_StopWatch;

       
        void Awake()
        {
            //Debug.Log("TextMeshPro OnEnable() has been called. HavePropertiesChanged = " + havePropertiesChanged); // has been called on Object ID:" + gameObject.GetInstanceID());      
      
            // Get Reference to AdvancedLayoutComponent if one is attached.
            m_advancedLayoutComponent = GetComponent<TMPro_AdvancedLayout>();
            //isAdvancedLayoutComponentPresent = m_advancedLayoutComponent == null ? false : true;

            // Reset max line lenght
            max_LineWrapLength = 999;

            // Cache Reference to the Mesh Renderer.
            m_renderer = GetComponent<Renderer>();
            m_renderer.sortingLayerID = m_sortingLayerID;
            m_renderer.sortingOrder = m_sortingOrder; 
        

            // Cache Reference to the transform;
            m_transform = transform;

            // Cache a reference to the Mesh Filter.
            m_meshFilter = GetComponent<MeshFilter>();
            if (m_meshFilter == null)
                m_meshFilter = gameObject.AddComponent<MeshFilter>();

            // Cache a reference to our mesh.
            if (m_mesh == null)
            {
                //Debug.Log("Creating new mesh."); 
                m_mesh = new Mesh();
                m_mesh.hideFlags = HideFlags.HideAndDontSave;

                m_meshFilter.mesh = m_mesh;                
                //m_mesh.bounds = new Bounds(transform.position, new Vector3(1000, 1000, 0));               
            }           
            m_meshFilter.hideFlags = HideFlags.HideInInspector;
                      
            // Load the font asset and assign material to renderer.
            LoadFontAsset();

            // Allocate our initial buffers.          
            m_char_buffer = new int[m_max_characters];
            //m_lineExtents = new Mesh_Extents[m_max_numberOfLines];
            m_cached_GlyphInfo = new GlyphInfo();
            m_vertices = new Vector3[0]; // 
            m_isFirstAllocation = true;          
            m_textInfo = new TextInfo();
            m_fontAssetArray = new TextMeshProFont[16];
            m_textInfo.lineInfo = new LineInfo[m_max_numberOfLines];
            m_textInfo.meshInfo = new TMPro_MeshInfo();      
         

            // Check if we have a font asset assigned. Return if we don't because no one likes to see purple squares on screen.
            if (m_fontAsset == null)
            {
                Debug.LogWarning("Please assign a Font Asset to this " + transform.name + " gameobject.");
                return;
            }

            //// Set flags to cause ensure our text is parsed and text redrawn. 
            isInputParsingRequired = true;
            havePropertiesChanged = true;
            
            ForceMeshUpdate(); // Added to force OnWillRenderObject() to be called in case object is not visible so we get initial bounds for the mesh.
            ///* ScheduleUpdate();         
        }


     
        void OnEnable()
        {          
            
            //Debug.Log("TextMeshPro OnEnable() has been called. HavePropertiesChanged = " + havePropertiesChanged); // has been called on Object ID:" + gameObject.GetInstanceID());      
                    
#if UNITY_EDITOR
            // Register Callbacks for various events.
            TMPro_EventManager.MATERIAL_PROPERTY_EVENT += ON_MATERIAL_PROPERTY_CHANGED;
            TMPro_EventManager.FONT_PROPERTY_EVENT += ON_FONT_PROPERTY_CHANGED;
            TMPro_EventManager.TEXTMESHPRO_PROPERTY_EVENT += ON_TEXTMESHPRO_PROPERTY_CHANGED;
#endif
            
            // Since Structures don't serialize in Unity 4.3 or below, we must re-allocate this array.          
            //m_textInfo.lineInfo = new LineInfo[m_max_numberOfLines];

            //if (m_meshInfo == null)
            //    Debug.Log("Mesh Info is null.");
        }


       void OnDisable()
        {
            //Debug.Log("TextMeshPro OnDisable() for " + this.name + " with ID: " + this.GetInstanceID() + " has been called.");

#if UNITY_EDITOR
            // Un-register the event this object was listening to
            TMPro_EventManager.MATERIAL_PROPERTY_EVENT -= ON_MATERIAL_PROPERTY_CHANGED;
            TMPro_EventManager.FONT_PROPERTY_EVENT -= ON_FONT_PROPERTY_CHANGED;
            TMPro_EventManager.TEXTMESHPRO_PROPERTY_EVENT -= ON_TEXTMESHPRO_PROPERTY_CHANGED;
#endif
                    
        }


        void OnDestroy()
        {
            // Destroy the mesh if we have one.
            //if (m_mesh != null)
            //{
            //    DestroyImmediate(m_mesh);
            //}
        }


        void Reset()
        {
            //Debug.Log("Reset() has been called.");  
            //LoadFontAsset();
            isInputParsingRequired = true;
            havePropertiesChanged = true;
        }


#if UNITY_EDITOR
        void OnValidate()
        {
            // Additional Properties could be added to sync up Serialized Properties & Properties.
            font = m_fontAsset;
            text = m_text;
            sortingLayerID = m_sortingLayerID;
            sortingOrder = m_sortingOrder;
            //maskOffset = m_maskOffset;
        }



        // Event received when custom material editor properties are changed.
        void ON_MATERIAL_PROPERTY_CHANGED(bool isChanged, Material mat)
        {
            //Debug.Log("ON_MATERIAL_PROPERTY_CHANGED event received. Targeted Material is: " + mat.name + "  m_sharedMaterial: " + m_sharedMaterial.name + "  m_renderer.sharedMaterial: " + m_renderer.sharedMaterial);         

            if (m_renderer.sharedMaterial == null)
            {
                if (m_fontAsset != null)
                {
                    m_renderer.sharedMaterial = m_fontAsset.material;
                    Debug.LogWarning("No Material was assigned to " + name + ". " + m_fontAsset.material.name + " was assigned.");
                }
                else
                    Debug.LogWarning("No Font Asset assigned to " + name + ". Please assign a Font Asset.");
            }

            if (m_fontAsset.atlas.GetInstanceID() != m_renderer.sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
            {
                m_renderer.sharedMaterial = m_sharedMaterial;
                //m_renderer.sharedMaterial = m_fontAsset.material;
                Debug.LogWarning("Font Asset Atlas doesn't match the Atlas in the newly assigned material. Select a matching material or a different font asset.");
            }

            if (m_renderer.sharedMaterial != m_sharedMaterial) //    || m_renderer.sharedMaterials.Contains(mat))
            {
                //Debug.Log("ON_MATERIAL_PROPERTY_CHANGED Called on Target ID: " + GetInstanceID() + ". Previous Material:" + m_sharedMaterial + "  New Material:" + m_renderer.sharedMaterial); // on Object ID:" + GetInstanceID() + ". m_sharedMaterial: " + m_sharedMaterial.name + "  m_renderer.sharedMaterial: " + m_renderer.sharedMaterial.name);         
                m_sharedMaterial = m_renderer.sharedMaterial;
            }

            m_padding =  ShaderUtilities.GetPadding(m_renderer.sharedMaterials, m_enableExtraPadding, m_isUsingBold);
            m_alignmentPadding = ShaderUtilities.GetFontExtent(m_sharedMaterial);
            havePropertiesChanged = true;
            /* ScheduleUpdate(); */
        }


        // Event received when font asset properties are changed in Font Inspector
        void ON_FONT_PROPERTY_CHANGED(bool isChanged, TextMeshProFont font)
        {
            if (font == m_fontAsset)
            {
                //Debug.Log("ON_FONT_PROPERTY_CHANGED event received.");
                havePropertiesChanged = true;
                hasFontAssetChanged = true;
                /* ScheduleUpdate(); */
            }
        }


        // Event received when UNDO / REDO Event alters the properties of the object.
        void ON_TEXTMESHPRO_PROPERTY_CHANGED(bool isChanged, TextMeshPro obj)
        {
            if (obj == this)
            {
                //Debug.Log("Undo / Redo Event Received by Object ID:" + GetInstanceID());
                havePropertiesChanged = true;
                isInputParsingRequired = true;
                /* ScheduleUpdate(); */
            }
        }
#endif


        // Function which loads either the default font or a newly assigned font asset. This function also assigned the appropriate material to the renderer.
        void LoadFontAsset()
        {          
            //Debug.Log("TextMeshPro LoadFontAsset() has been called."); // Current Font Asset is " + (font != null ? font.name: "Null") );
            
            if (m_fontAsset == null)  
            {
                
                //Debug.Log("Font Asset are Null. Trying to Load Default Arial SDF Font.");
                m_fontAsset = Resources.Load("Fonts/ARIAL SDF", typeof(TextMeshProFont)) as TextMeshProFont;
                if (m_fontAsset == null)
                {
                    Debug.LogWarning("There is no Font Asset assigned to " + gameObject.name + ".");
                    return;
                }

                if (m_fontAsset.characterDictionary == null)
                {
                    Debug.Log("Dictionary is Null!");
                }

                m_renderer.sharedMaterial = m_fontAsset.material;
                m_sharedMaterial = m_fontAsset.material;
                m_sharedMaterial.SetFloat("_ZTestMode", 4);
                m_renderer.receiveShadows = false;
                m_renderer.castShadows = false; // true;
                // Get a Reference to the Shader
            }
            else
            {
                if (m_fontAsset.characterDictionary == null)
                {
                    //Debug.Log("Reading Font Definition and Creating Character Dictionary.");
                    m_fontAsset.ReadFontDefinition();
                }

                //Debug.Log("Font Asset name:" + font.material.name);

                // If font atlas texture doesn't match the assigned material font atlas, switch back to default material specified in the Font Asset.
                if (m_renderer.sharedMaterial == null || m_renderer.sharedMaterial.mainTexture == null || m_fontAsset.atlas.GetInstanceID() != m_renderer.sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
                {
                    m_renderer.sharedMaterial = m_fontAsset.material;
                    m_sharedMaterial = m_fontAsset.material; 
                }
                else
                {
                    m_sharedMaterial = m_renderer.sharedMaterial;
                }

                m_sharedMaterial.SetFloat("_ZTestMode", 4);

                // Check if we are using the SDF Surface Shader
                if (m_sharedMaterial.passCount > 1)
                {
                    m_renderer.receiveShadows = false;
                    m_renderer.castShadows = true;
                }
                else
                {
                    m_renderer.receiveShadows = false;
                    m_renderer.castShadows = false;
                }
            }

            m_padding = ShaderUtilities.GetPadding(m_renderer.sharedMaterials, m_enableExtraPadding, m_isUsingBold);
            m_alignmentPadding = ShaderUtilities.GetFontExtent(m_sharedMaterial);

            if (!m_fontAsset.characterDictionary.TryGetValue(95, out m_cached_Underline_GlyphInfo)) //95
                Debug.Log("Underscore character wasn't found in the current Font Asset. No characters assigned for Underline.");

            m_sharedMaterials.Add(m_sharedMaterial);          
            // Hide Material Editor Component
            //m_renderer.sharedMaterial.hideFlags = HideFlags.None;
        }


        /// <summary>
        /// Function under development to utilize an Update Manager instead of normal event functions like LateUpdate() or OnWillRenderObject().
        /// </summary>
        void ScheduleUpdate()
        {
            return;
            /*
            if (!isAlreadyScheduled)
            {
                m_updateManager.ScheduleObjectForUpdate(this);
                isAlreadyScheduled = true;
            }
            */
        }


        void UpdateMask()
        {
            //Debug.Log("Changing property block.");
            
            //MaterialPropertyBlock block = new MaterialPropertyBlock();
            //Vector4 offsetDelta = new Vector4(m_mesh.bounds.center.x + m_maskOffset.x, m_mesh.bounds.center.y + m_maskOffset.y, m_mesh.bounds.extents.x + m_maskOffset.z, m_mesh.bounds.extents.y + m_maskOffset.w); 
            //block.AddVector(ShaderUtilities.ID_MaskCoord, m_maskOffset);
            //block.AddFloat(ShaderUtilities.ID_MaskSoftnessX, m_maskSoftness.x);
            //block.AddFloat(ShaderUtilities.ID_MaskSoftnessY, m_maskSoftness.y);
            //block.AddFloat(ShaderUtilities.ID_VertexOffsetX, m_vertexOffset.x);
            //block.AddFloat(ShaderUtilities.ID_VertexOffsetY, m_vertexOffset.y);

            //m_renderer.SetPropertyBlock(block);          
        }


        // Function to allocate the necessary buffers to render the text. This function is called whenever the buffer size needs to be increased.
        void SetMeshArrays(int size)
        {
            // Should add a check to make sure we don't try to create a mesh that contains more than 65535 vertices.

            int sizeX4 = size * 4;
            int sizeX6 = size * 6;

            m_vertices = new Vector3[sizeX4];
            m_normals = new Vector3[sizeX4];
            m_tangents = new Vector4[sizeX4];

            m_uvs = new Vector2[sizeX4];
            m_uv2s = new Vector2[sizeX4];
            m_vertColors = new Color32[sizeX4];

            m_triangles = new int[sizeX6];

            // Setup Triangle Structure 
            for (int i = 0; i < size; i++)
            {
                int index_X4 = i * 4;
                int index_X6 = i * 6;

                m_vertices[0 + index_X4] = Vector3.zero;
                m_vertices[1 + index_X4] = Vector3.zero;
                m_vertices[2 + index_X4] = Vector3.zero;
                m_vertices[3 + index_X4] = Vector3.zero;

                m_uvs[0 + index_X4] = Vector2.zero;
                m_uvs[1 + index_X4] = Vector2.zero;
                m_uvs[2 + index_X4] = Vector2.zero;
                m_uvs[3 + index_X4] = Vector2.zero;

                m_normals[0 + index_X4] = new Vector3(0, 0, -1);
                m_normals[1 + index_X4] = new Vector3(0, 0, -1);
                m_normals[2 + index_X4] = new Vector3(0, 0, -1);
                m_normals[3 + index_X4] = new Vector3(0, 0, -1);

                m_tangents[0 + index_X4] = new Vector4(-1, 0, 0, 1);
                m_tangents[1 + index_X4] = new Vector4(-1, 0, 0, 1);
                m_tangents[2 + index_X4] = new Vector4(-1, 0, 0, 1);
                m_tangents[3 + index_X4] = new Vector4(-1, 0, 0, 1);

                // Setup Triangles based on whether or not Shadow Mode is Enabled.       
                m_triangles[0 + index_X6] = 0 + index_X4;
                m_triangles[1 + index_X6] = 1 + index_X4;
                m_triangles[2 + index_X6] = 2 + index_X4;
                m_triangles[3 + index_X6] = 3 + index_X4;
                m_triangles[4 + index_X6] = 2 + index_X4;
                m_triangles[5 + index_X6] = 1 + index_X4;
            }

            //Debug.Log("Size:" + size + "  Vertices:" + m_vertices.Length + "  Triangles:" + m_triangles.Length + " Mesh - Vertices:" + m_mesh.vertices.Length + "  Triangles:" + m_mesh.triangles.Length);

            m_mesh.vertices = m_vertices;
            m_mesh.uv = m_uvs;
            m_mesh.normals = m_normals;
            m_mesh.tangents = m_tangents;
            m_mesh.triangles = m_triangles;

            //Debug.Log("Bounds were updated.");
            m_mesh.bounds = m_default_bounds;
        }


        // Function called internally when a new material is assigned via the fontMaterial property.
        void SetFontMaterial(Material mat)
        {
            // Check in case Object is disabled. If so, we don't have a valid reference to the Renderer.
            // This can occur when the Duplicate Material Context menu is used on an inactive object.
            if (m_renderer == null)
                m_renderer = GetComponent<Renderer>();

            m_renderer.material = mat;
            m_fontMaterial = m_renderer.material;
            m_sharedMaterial = m_fontMaterial;
            m_padding = ShaderUtilities.GetPadding(m_renderer.sharedMaterials, m_enableExtraPadding, m_isUsingBold);
            m_alignmentPadding = ShaderUtilities.GetFontExtent(m_sharedMaterial);
        }


        // Function called internally when a new shared material is assigned via the fontSharedMaterial property.
        void SetSharedFontMaterial(Material mat)
        {
            // Check in case Object is disabled. If so, we don't have a valid reference to the Renderer.
            // This can occur when the Duplicate Material Context menu is used on an inactive object.
            if (m_renderer == null)
                m_renderer = GetComponent<Renderer>();

            m_renderer.sharedMaterial = mat;
            m_sharedMaterial = m_renderer.sharedMaterial;
            m_padding = ShaderUtilities.GetPadding(m_renderer.sharedMaterials, m_enableExtraPadding, m_isUsingBold);
            m_alignmentPadding = ShaderUtilities.GetFontExtent(m_sharedMaterial);
        }


        // This function will create an instance of the Font Material.
        void SetOutlineThickness(float thickness)
        {            
            thickness = Mathf.Clamp01(thickness);
            m_renderer.material.SetFloat(ShaderUtilities.ID_OutlineWidth, thickness);

            if (m_fontMaterial == null)
                m_fontMaterial = m_renderer.material;

            m_fontMaterial = m_renderer.material;               
        }


        // This function will create an instance of the Font Material.
        void SetFaceColor(Color32 color)
        {
            m_renderer.material.SetColor(ShaderUtilities.ID_FaceColor, color);

            if (m_fontMaterial == null)
                m_fontMaterial = m_renderer.material;

            //Debug.Log("Material ID:" + m_fontMaterial.GetInstanceID());
            //m_faceColor = m_renderer.material;
        }


        // This function will create an instance of the Font Material.
        void SetOutlineColor(Color32 color)
        {
            m_renderer.material.SetColor(ShaderUtilities.ID_OutlineColor, color);

            if (m_fontMaterial == null)
                m_fontMaterial = m_renderer.material;

            //Debug.Log("Material ID:" + m_fontMaterial.GetInstanceID());
            //m_faceColor = m_renderer.material;
        }


        // Sets the Render Queue and Ztest mode 
        void SetShaderType()
        {
            if (m_isOverlay)
            {
                // Changing these properties results in an instance of the material           
                m_renderer.material.SetFloat("_ZTestMode", 8);
                m_renderer.material.renderQueue = 4000;

                m_sharedMaterial = m_renderer.material;
            }
            else
            {   // TODO: This section needs to be tested.
                //m_renderer.material.SetFloat("_ZWriteMode", 0);
                m_renderer.material.SetFloat("_ZTestMode", 4);
                m_renderer.material.renderQueue = -1;
                m_sharedMaterial = m_renderer.material;
            }

            //if (m_fontMaterial == null)
            //    m_fontMaterial = m_renderer.material;
        }


        // Set Perspective Correction Mode based on whether Camera is Orthographic or Perspective
        void SetPerspectiveCorrection()
        {
            if (m_isOrthographic)
                m_sharedMaterial.SetFloat(ShaderUtilities.ID_PerspectiveFilter, 0.0f);
            else
                m_sharedMaterial.SetFloat(ShaderUtilities.ID_PerspectiveFilter, 0.875f);
        }


        // Function used in conjunection with SetText()
        void AddIntToCharArray(int number, ref int index, int precision)
        {
            if (number < 0)
            {
                m_input_CharArray[index++] = '-';
                number = -number;
            }

            int i = index;
            do
            {
                m_input_CharArray[i++] = (char)(number % 10 + 48);
                number /= 10;
            } while (number > 0);

            int lastIndex = i;

            // Reverse string
            while (index + 1 < i)
            {
                i -= 1;
                char t = m_input_CharArray[index];
                m_input_CharArray[index] = m_input_CharArray[i];
                m_input_CharArray[i] = t;
                index += 1;
            }
            index = lastIndex;
        }


        // Functions used in conjunction with SetText()
        void AddFloatToCharArray(float number, ref int index, int precision)
        {
            if (number < 0)
            {
                m_input_CharArray[index++] = '-';
                number = -number;
            }

            number += k_Power[Mathf.Min(9, precision)];

            int integer = (int)number;
            AddIntToCharArray(integer, ref index, precision);

            if (precision > 0)
            {
                // Add the decimal point
                m_input_CharArray[index++] = '.';

                number -= integer;
                for (int p = 0; p < precision; p++)
                {
                    number *= 10;
                    int d = (int)(number);

                    m_input_CharArray[index++] = (char)(d + 48);
                    number -= d;
                }
            }
        }


        // Converts a string to a Char[]
        void StringToCharArray(string text, ref int[] chars)
        {
            if (text == null)
                return;

            // Check to make sure chars_buffer is large enough to hold the content of the string.
            if (chars.Length <= text.Length)
            {
                int newSize = text.Length > 1024 ? text.Length + 256 : Mathf.NextPowerOfTwo(text.Length + 1);
                //Debug.Log("Resizing the chars_buffer[" + chars.Length + "] to chars_buffer[" + newSize + "].");
                chars = new int[newSize];
            }

            int index = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == 92 && i < text.Length - 1)
                {
                    switch ((int)text[i + 1])
                    {
                        case 116: // \t Tab
                            chars[index] = (char)9;
                            i += 1;
                            index += 1;
                            continue;
                        case 110: // \n LineFeed
                            chars[index] = (char)10;
                            i += 1;
                            index += 1;
                            continue;
                    }
                }

                chars[index] = text[i];
                index += 1;
            }
            chars[index] = (char)0;
        }


        // Copies Content of formatted SetText() to charBuffer.
        void SetTextArrayToCharArray(char[] charArray, ref int[] charBuffer)
        {
            //Debug.Log("SetText Array to Char called.");
            if (charArray == null || m_charArray_Length == 0)
                return;

            // Check to make sure chars_buffer is large enough to hold the content of the string.
            if (charBuffer.Length <= m_charArray_Length)
            {
                int newSize = m_charArray_Length > 1024 ? m_charArray_Length + 256 : Mathf.NextPowerOfTwo(m_charArray_Length + 1);
                charBuffer = new int[newSize];
            }

            int index = 0;

            for (int i = 0; i < m_charArray_Length; i++)
            {
                if (charArray[i] == 92 && i < m_charArray_Length - 1)
                {
                    switch ((int)charArray[i + 1])
                    {
                        case 116: // \t Tab
                            charBuffer[index] = 9;
                            i += 1;
                            index += 1;
                            continue;
                        case 110: // \n LineFeed
                            charBuffer[index] = 10;
                            i += 1;
                            index += 1;
                            continue;
                    }
                }

                charBuffer[index] = charArray[i];
                index += 1;
            }
            charBuffer[index] = 0;
        }


        /// <summary>
        /// Function used in conjunction with GetTextInfo to figure out Array allocations.
        /// </summary>
        /// <param name="chars"></param>
        /// <returns></returns>
        int GetArraySizes(int[] chars)
        {
            //Debug.Log("Set Array Size called.");

            int visibleCount = 0;
            int totalCount = 0;
            int tagEnd = 0;
            m_isUsingBold = false;

            for (int i = 0; chars[i] != 0; i++)
            {
                int c = chars[i];

                if (m_isRichText && c == 60) // if Char '<'
                {
                    // Check if Tag is Valid
                    if (ValidateHtmlTag(chars, i + 1, out tagEnd))
                    {
                        i = tagEnd;
                        if ((m_style & FontStyles.Underline) == FontStyles.Underline) visibleCount += 3;

                        if ((m_style & FontStyles.Bold) == FontStyles.Bold) m_isUsingBold = true;

                        continue;
                    }
                }

                if (c != 32 && c != 9 && c != 10 && c != 13)
                {
                    visibleCount += 1;
                }

                totalCount += 1;
            }
                   
            return totalCount;
        }




        // This function parses through the Char[] to determine how many characters will be visible. It then makes sure the arrays are large enough for all those characters.
        void SetArraySizes(int[] chars)
        {
            //Debug.Log("Set Array Size called.");

            int visibleCount = 0;
            int totalCount = 0;
            int tagEnd = 0;
            m_isUsingBold = false;

            for (int i = 0; chars[i] != 0; i++)
            {
                int c = chars[i];

                if (m_isRichText && c == 60) // if Char '<'
                {
                    // Check if Tag is Valid
                    if (ValidateHtmlTag(chars, i + 1, out tagEnd))
                    {
                        i = tagEnd;
                        if ((m_style & FontStyles.Underline) == FontStyles.Underline) visibleCount += 3;

                        if ((m_style & FontStyles.Bold) == FontStyles.Bold) m_isUsingBold = true;
            
                        continue;
                    }
                }

                if (c != 32 && c != 9 && c != 10 && c != 13)
                {
                    visibleCount += 1;
                }

                totalCount += 1;
            }


            if (m_textInfo.characterInfo == null || totalCount > m_textInfo.characterInfo.Length)
            {                
                m_textInfo.characterInfo = new TMPro_CharacterInfo[totalCount > 1024 ? totalCount + 256 : Mathf.NextPowerOfTwo(totalCount)];              
            }

            // Make sure our Mesh Buffer Allocations can hold these new Quads.
            if (visibleCount * 4 > m_vertices.Length)
            {              
                // If this is the first allocation, we allocated exactly the number of Quads we need. Otherwise, we allocated more since this text object is dynamic.
                if (m_isFirstAllocation)
                {
                    SetMeshArrays(visibleCount);
                    m_isFirstAllocation = false;
                }
                else
                {
                    SetMeshArrays(visibleCount > 1024 ? visibleCount + 256 : Mathf.NextPowerOfTwo(visibleCount));
                }
            }
        }


        // Added to sort handle the potential issue with OnWillRenderObject() not getting called when objects are not visible by camera.
        //void OnBecameInvisible()
        //{
        //    if (m_mesh != null)
        //        m_mesh.bounds = new Bounds(transform.position, new Vector3(1000, 1000, 0));
        //}

      
        // Called for every Camera able to see the Object. 
        void OnWillRenderObject()
        {                      
            // This will be called for each active camera and thus should be optimized as it is not necessary to update the mesh for each camera.
            //Debug.Log("OnWillRenderObject() has been called!");            

            // Check if Transform has changed since last update.          
            if (m_transform.hasChanged)
            {
                m_transform.hasChanged = false;
                // We need to regenerate the mesh if the lossy scale has changed.
                if (m_transform.lossyScale != m_lossyScale)
                {
                    havePropertiesChanged = true;
                    m_lossyScale = m_transform.lossyScale;   
                }                      
            }
            

            if (havePropertiesChanged || m_fontAsset.propertiesChanged)
            {
                //Debug.Log("Properties have changed!"); // Assigned Material is:" + m_sharedMaterial); // New Text is: " + m_text + ".");                

                if (hasFontAssetChanged || m_fontAsset.propertiesChanged)
                {
                    //Debug.Log( m_fontAsset.name);
                    
                    LoadFontAsset();

                    hasFontAssetChanged = false;

                    if (m_fontAsset == null || m_renderer.sharedMaterial == null)
                        return;

                    m_fontAsset.propertiesChanged = false;
                }

                /*
                if (isMaskUpdateRequired)
                {                                                      
                    UpdateMask();
                    isMaskUpdateRequired = false;
                }
                */

                // Reparse Input if modified properties affect word wrapping.
                if (m_enableWordWrapping && isAffectingWordWrapping || isInputParsingRequired)
                {
                    //Debug.Log("Reparsing Text.");
                    isInputParsingRequired = false;

                    switch (m_inputSource)
                    {
                        case TextInputSources.Text:
                            StringToCharArray(m_text, ref m_char_buffer);
                            //isTextChanged = false;
                            break;
                        case TextInputSources.SetText:
                            SetTextArrayToCharArray(m_input_CharArray, ref m_char_buffer);
                            //isSetTextChanged = false;
                            break;
                        case TextInputSources.SetCharArray:
                            break;
                    }
                }

                GenerateTextMesh();              
                havePropertiesChanged = false;
                //isAlreadyScheduled = false;
            }          
        }

        

        
        /// <summary>
        /// This is the main function that is responsible for creating / displaying the text.
        /// </summary>
        void GenerateTextMesh()
        {
            //Debug.Log("GenerateTextMesh has been called.");

            // Early exit if no font asset was assigned. This should not be needed since Arial SDF will be assigned by default.
            if (m_fontAsset.characterDictionary == null)
            {
                Debug.Log("Can't Generate Mesh! No Font Asset has been assigned to Object ID: " + this.GetInstanceID());
                return;
            }

            // Early exit if we don't have any Text to generate.          
            if (m_char_buffer == null || m_char_buffer[0] == (char)0)
            {
                if (m_vertices != null)
                {
                    Array.Clear(m_vertices, 0, m_vertices.Length);
                    m_mesh.vertices = m_vertices;                   
                }
                return;
            }

            
            // Determine how many characters will be visible and make the necessary allocations (if needed).
            SetArraySizes(m_char_buffer);


            m_fontIndex = 0; // Will be used when support for using different font assets or sprites withint the same object will be added.
            m_fontAssetArray[m_fontIndex] = m_fontAsset;

            // Scale the font to approximately match the point size           
            m_fontScale = (m_fontSize / m_fontAssetArray[m_fontIndex].fontInfo.PointSize * (m_isOrthographic ? 1 : 0.1f));
            float baseScale = m_fontScale; // BaseScale keeps the character aligned vertically since <size=+000> results in font of different scale.
            m_currentFontSize = m_fontSize;
           
            int charCode = 0; // Holds the character code of the currently being processed character.
            //int prev_charCode = 0;
            bool isMissingCharacter; // Used to handle missing characters in the Font Atlas / Definition.

            //bool isLineTruncated = false;

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

            bool beginUnderline = false;
            Vector3 underline_start = Vector3.zero; // Used to track where underline starts & ends.
            Vector3 underline_end = Vector3.zero;

            Color32 vertexColor;
            m_htmlColor = m_fontColor;

           
            float lineOffset = 0; // Amount of space between lines (font line spacing + m_linespacing).
            m_xAdvance = 0; // Used to track the position of each character.         

            int lineNumber = 0;
            int wordCount = 0;
            int character_Count = 0; // Total characters in the char[]
            int visibleCharacter_Count = 0; // # of visible characters.

            // Limit Line Length to whatever size fits all characters on a single line.
            m_lineLength = m_lineLength > max_LineWrapLength ? max_LineWrapLength : m_lineLength;
            bool isLineTruncated = false;

            // Initialize struct to track states of word wrapping
            m_SaveWordWrapState = new WordWrapState();
            int wrappingIndex = 0;

            // Need to initialize these Extents Structs
            Extents meshExtents = new Extents(k_InfinityVector, -k_InfinityVector);
            if (m_textInfo.lineInfo == null) m_textInfo.lineInfo = new LineInfo[2];
            for (int i = 0; i < m_textInfo.lineInfo.Length; i++)
            {
                //m_lineExtents[i] = meshExtents;
                m_textInfo.lineInfo[i] = new LineInfo();
                //m_textInfo.lineInfo[i].lineExtents = new Extents(k_InfinityVector, -k_InfinityVector);
            }
                     
            // Tracking of the highest Ascender
            float maxAscender = 0;
            float maxDescender = 0;        
                       
            int lastLineNumber = 0;

            int endTagIndex = 0;
            // Parse through Character buffer to read html tags and begin creating mesh.
            for (int i = 0; m_char_buffer[i] != 0; i++)
            {
                m_tabSpacing = -999;
                m_spacing = -999;
                charCode = m_char_buffer[i];

				//Debug.Log("i:" + i + "  Character [" + (char)charCode + "]");

                if (m_isRichText && charCode == 60)  // '<'
                {
                    // Check if Tag is valid. If valid, skip to the end of the validated tag.
                    if (ValidateHtmlTag(m_char_buffer, i + 1, out endTagIndex))
                    {
                        i = endTagIndex;

                        if (m_isRecalculateScaleRequired)
                        {
                            m_fontScale = (m_currentFontSize / m_fontAssetArray[m_fontIndex].fontInfo.PointSize * (m_isOrthographic ? 1 : 0.1f));
                            isAffectingWordWrapping = true;
                            m_isRecalculateScaleRequired = false;
                        }
                        
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

                isMissingCharacter = false;
               
              
                // Check if we should be using a different font asset
                //if (m_fontIndex != 0)
                //{
                //    // Check if we need to load the new font asset
                //    if (m_fontAssetArray[m_fontIndex] == null)
                //    {
                //        Debug.Log("Loading secondary font asset.");
                //        m_fontAssetArray[m_fontIndex] = Resources.Load("Fonts/Bangers SDF", typeof(TextMeshProFont)) as TextMeshProFont;
                //        //m_sharedMaterials.Add(m_fontAssetArray[m_fontIndex].material);
                //        //m_renderer.sharedMaterials = new Material[] { m_sharedMaterial, m_fontAssetArray[m_fontIndex].material }; // m_sharedMaterials.ToArray();
                //    }
                //}
               
                //Debug.Log("Char [" + (char)charCode + "] is using FontIndex: " + m_fontIndex);

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
                        isMissingCharacter = true;
                    }
                    else
                    {  // At this point the character isn't in the Dictionary, the replacement X isn't either so ... 
                        //continue;
                    }
                }

                // Store some of the text object's information
                m_textInfo.characterInfo[character_Count].character = (char)charCode;
                m_textInfo.characterInfo[character_Count].color = m_htmlColor;
                m_textInfo.characterInfo[character_Count].style = m_style;
                m_textInfo.characterInfo[character_Count].index = (short)i;

              
                // Handle Kerning if Enabled.                 
                if (m_enableKerning && character_Count >= 1)
                {
                    int prev_charCode = m_textInfo.characterInfo[character_Count - 1].character;
                    KerningPairKey keyValue = new KerningPairKey(prev_charCode, charCode);

                    KerningPair pair;

                    m_fontAssetArray[m_fontIndex].kerningDictionary.TryGetValue(keyValue.key, out pair);
                    if (pair != null)
                    {
                        m_xAdvance += pair.XadvanceOffset * m_fontScale;
                    }
                }

                // Set Padding based on selected font style
                if ((m_style & FontStyles.Bold) == FontStyles.Bold) // Checks for any combination of Bold Style.
                {
                    style_padding = m_fontAssetArray[m_fontIndex].BoldStyle * 2;
                    xadvance_multiplier = 1.07f; // Increase xAdvance for bold characters.         
                }
                else
                {
                    style_padding = m_fontAssetArray[m_fontIndex].NormalStyle * 2;
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
                    float shear_value = m_fontAssetArray[m_fontIndex].ItalicStyle * 0.01f;
                    Vector3 topShear = new Vector3(shear_value * ((m_cached_GlyphInfo.yOffset + m_padding + style_padding) * m_fontScale), 0, 0);
                    Vector3 bottomShear = new Vector3(shear_value * (((m_cached_GlyphInfo.yOffset - m_cached_GlyphInfo.height - m_padding - style_padding)) * m_fontScale), 0, 0);

                    top_left = top_left + topShear;
                    bottom_left = bottom_left + bottomShear;
                    top_right = top_right + topShear;
                    bottom_right = bottom_right + bottomShear;
                }

                // Set Characters to not visible by default.
                m_textInfo.characterInfo[character_Count].isVisible = false;


				// Track Word Count per line and for the object
				if (character_Count > 0 && (char.IsWhiteSpace((char)charCode) || char.IsPunctuation((char)charCode)))
				{
					if (char.IsLetterOrDigit(m_textInfo.characterInfo[character_Count - 1].character))
					{
						wordCount += 1;
						m_textInfo.lineInfo[lineNumber].wordCount += 1;
					}
				}


                // Setup Mesh for visible characters. ie. not a SPACE / LINEFEED / CARRIAGE RETURN.
                if (charCode != 32 && charCode != 9 && charCode != 10 && charCode != 13)
                {
                    int index_X4 = visibleCharacter_Count * 4;
                    //int index_X6 = VisibleCharacter_Count * 6;

                    m_textInfo.characterInfo[character_Count].isVisible = true;
                    m_textInfo.characterInfo[character_Count].vertexIndex = (short)(0 + index_X4);

                 
                    m_vertices[0 + index_X4] = bottom_left;
                    m_vertices[1 + index_X4] = top_left;
                    m_vertices[2 + index_X4] = bottom_right;
                    m_vertices[3 + index_X4] = top_right;

                  
                    // Determine the bounds of the Mesh.             
                    meshExtents.min = new Vector2(Mathf.Min(meshExtents.min.x, bottom_left.x), Mathf.Min(meshExtents.min.y, bottom_left.y));
                    meshExtents.max = new Vector2(Mathf.Max(meshExtents.max.x, top_right.x), Mathf.Max(meshExtents.max.y, top_left.y));
                              
                    // Determine the extend of each line
                    LineInfo lineInfo = m_textInfo.lineInfo[lineNumber];
                    Extents lineExtents = lineInfo.lineExtents;
                    m_textInfo.lineInfo[lineNumber].lineExtents.min = new Vector2(Mathf.Min(lineExtents.min.x, bottom_left.x), Mathf.Min(lineExtents.min.y, bottom_left.y));
                    m_textInfo.lineInfo[lineNumber].lineExtents.max = new Vector2(Mathf.Max(lineExtents.max.x, top_right.x), Mathf.Max(lineExtents.max.y, top_left.y));
                    //m_textInfo.lineInfo[lineNumber].characterCount += 1;
                    
                    //m_textInfo.characterInfo[character_Count].charNumber = (short)m_lineExtents[lineNumber].characterCount;                                      

                    
                                     
                    // Handle Word Wrapping if Enabled               
                    if (m_enableWordWrapping && top_right.x > m_lineLength)
                    {
                        //Debug.Log("Line " + lineNumber + " has exceeded the bounds.");
                      
                        // Check if further wrapping is possible or if we need to increase the line length
                        if (wrappingIndex == m_SaveWordWrapState.previous_WordBreak) 
                        {                          
                            if (isAffectingWordWrapping)
                            {
                                m_lineLength = Mathf.Round(top_right.x * 100 + 0.5f) / 100f;//m_lineLength = top_right.x;
                                GenerateTextMesh();
                                isAffectingWordWrapping = false;
                            }
                    
                            //Debug.Log("Line " + lineNumber + " Cannot wrap lines anymore.");
                            return; 
                        }
                                         
                        // Restore to previously stored state
                        character_Count = m_SaveWordWrapState.total_CharacterCount + 1;
                        visibleCharacter_Count = m_SaveWordWrapState.visible_CharacterCount;                                             
						m_textInfo.lineInfo[lineNumber] = m_SaveWordWrapState.lineInfo;
                        meshExtents = m_SaveWordWrapState.meshExtents;
                        m_htmlColor = m_SaveWordWrapState.vertexColor;
                        m_style = m_SaveWordWrapState.fontStyle;
                        m_baselineOffset = m_SaveWordWrapState.baselineOffset;
                        m_fontScale = m_SaveWordWrapState.fontScale;
                        i = m_SaveWordWrapState.previous_WordBreak;
                        wrappingIndex = i;  // Used to dectect when line length can no longer be reduced.

                       
                        lineNumber += 1;
                        // Check to make sure Array is large enough to hold a new line.
                        if (lineNumber >= m_textInfo.lineInfo.Length)
                            ResizeLineExtents(lineNumber);

                        lineOffset += (m_fontAssetArray[m_fontIndex].fontInfo.LineHeight + m_lineSpacing);
                        m_xAdvance = 0;
                        continue;
                    }

                   
                    // Determine what color gets assigned to vertex.     
                    if (isMissingCharacter)
                        vertexColor = Color.red;
                    else if (m_overrideHtmlColors)
                        vertexColor = m_fontColor;
                    else
                        vertexColor = m_htmlColor;

                   
                    if (m_maxVisibleLines > 0 && lineNumber >= m_maxVisibleLines && isLineTruncated == false) // && isLineTruncated == false)
                    {                       
                        isLineTruncated = true;                        
                        m_char_buffer[m_SaveWordWrapState.previous_WordBreak] = (char)0;
                        m_char_buffer[m_SaveWordWrapState.previous_WordBreak - 1] = '.';
                        m_char_buffer[m_SaveWordWrapState.previous_WordBreak - 2] = '.';
                        m_char_buffer[m_SaveWordWrapState.previous_WordBreak - 3] = '.';                        
                        GenerateTextMesh();                     
                        return;                     
                    }
                    
                                    
                    // Set Alpha for Shader to render font as normal or bold. (Alpha channel is being used to let the shader know if the character is bold or normal).
                    if ((m_style & FontStyles.Bold) == FontStyles.Bold)
                    {
                        vertexColor.a = m_fontColor.a < vertexColor.a ? (byte)(m_fontColor.a >> 1) : (byte)(vertexColor.a >> 1);
                        vertexColor.a += 128;
                    }
                    else
                    {
                        vertexColor.a = m_fontColor.a < vertexColor.a ? (byte)(m_fontColor.a >> 1) : (byte)(vertexColor.a >> 1); 
                    }
               
                    m_vertColors[0 + index_X4] = vertexColor;
                    m_vertColors[1 + index_X4] = vertexColor;
                    m_vertColors[2 + index_X4] = vertexColor;
                    m_vertColors[3 + index_X4] = vertexColor;


                    // Apply style_padding only if this is a SDF Shader.
                    if (!m_sharedMaterial.HasProperty(ShaderUtilities.ID_WeightNormal))
                        style_padding = 0;

                    // Setup UVs for the Mesh
                    Vector2 uv0 = new Vector2((m_cached_GlyphInfo.x - m_padding - style_padding) / m_fontAssetArray[m_fontIndex].fontInfo.AtlasWidth, 1 - (m_cached_GlyphInfo.y + m_padding + style_padding + m_cached_GlyphInfo.height) / m_fontAssetArray[m_fontIndex].fontInfo.AtlasHeight);  // bottom left
                    Vector2 uv1 = new Vector2(uv0.x, 1 - (m_cached_GlyphInfo.y - m_padding - style_padding) / m_fontAssetArray[m_fontIndex].fontInfo.AtlasHeight);  // top left
                    Vector2 uv2 = new Vector2((m_cached_GlyphInfo.x + m_padding + style_padding + m_cached_GlyphInfo.width) / m_fontAssetArray[m_fontIndex].fontInfo.AtlasWidth, uv0.y); // bottom right
                    Vector2 uv3 = new Vector2(uv2.x, uv1.y); // top right

                    m_uvs[0 + index_X4] = uv0;
                    m_uvs[1 + index_X4] = uv1;
                    m_uvs[2 + index_X4] = uv2;
                    m_uvs[3 + index_X4] = uv3;

                    visibleCharacter_Count += 1;
                   
                }
                else
                {   // This is a Space, Tab, LineFeed or Carriage Return              
                    
                    // Track # of spaces per line which is used for line justification.
                    if (charCode == 9 || charCode == 32)
                    {                      
                        m_textInfo.lineInfo[lineNumber].spaceCount += 1;
                        m_textInfo.spaceCount += 1;
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
                    m_SaveWordWrapState.meshExtents = meshExtents;                   
					m_SaveWordWrapState.lineInfo = m_textInfo.lineInfo[lineNumber];                  
                }


                // Store Rectangle positions for each Character.
                m_textInfo.characterInfo[character_Count].bottomLeft = bottom_left;
                m_textInfo.characterInfo[character_Count].topRight = top_right;
                m_textInfo.characterInfo[character_Count].lineNumber = (short)lineNumber;

                m_textInfo.characterInfo[character_Count].baseLine = top_right.y - (m_cached_GlyphInfo.yOffset + m_padding) * m_fontScale;
                m_textInfo.characterInfo[character_Count].topLine = m_textInfo.characterInfo[character_Count].baseLine + (m_fontAssetArray[m_fontIndex].fontInfo.Ascender + m_alignmentPadding.y) * m_fontScale; // Ascender              
                m_textInfo.characterInfo[character_Count].bottomLine = m_textInfo.characterInfo[character_Count].baseLine + (m_fontAssetArray[m_fontIndex].fontInfo.Descender - m_alignmentPadding.w) * m_fontScale; // Descender          

                maxAscender = m_textInfo.characterInfo[character_Count].topLine > maxAscender ? m_textInfo.characterInfo[character_Count].topLine : maxAscender;
                maxDescender = m_textInfo.characterInfo[character_Count].bottomLine < maxDescender ? m_textInfo.characterInfo[character_Count].bottomLine : maxDescender;

                m_textInfo.characterInfo[character_Count].aspectRatio = m_cached_GlyphInfo.width / m_cached_GlyphInfo.height;
                m_textInfo.characterInfo[character_Count].scale = m_fontScale;

                m_textInfo.lineInfo[lineNumber].characterCount += 1;

                //Debug.Log("Character #" + i + " is [" + (char)charCode + "] ASCII (" + charCode + ")");

                // Store LineInfo 
                if (lineNumber != lastLineNumber)
                {
                    m_textInfo.lineInfo[lineNumber].firstCharacterIndex = character_Count;
                    m_textInfo.lineInfo[lineNumber - 1].lastCharacterIndex = character_Count - 1;
                    m_textInfo.lineInfo[lineNumber - 1].characterCount = m_textInfo.lineInfo[lineNumber - 1].lastCharacterIndex - m_textInfo.lineInfo[lineNumber - 1].firstCharacterIndex + 1;
                    m_textInfo.lineInfo[lineNumber - 1].lineLength = m_textInfo.characterInfo[character_Count - 1].topRight.x - m_padding * m_fontScale;
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
                    if (lineNumber >= m_textInfo.lineInfo.Length)
                        ResizeLineExtents(lineNumber);

                    lineOffset += (m_fontAssetArray[m_fontIndex].fontInfo.LineHeight + m_lineSpacing);
                    m_xAdvance = 0;
                }
               
                character_Count += 1; 
            
            }

          
            // Add Termination Character to textMeshCharacterInfo which is used by the Advanced Layout Component.     
            if (character_Count < m_textInfo.characterInfo.Length)
                m_textInfo.characterInfo[character_Count].character = (char)0;
         
        
            // DEBUG & PERFORMANCE CHECKS (0.006ms)
            //m_StopWatch.Stop();          

            //for (int i = 0; i < lineNumber + 1; i++)
            //{
            //    Debug.Log("Line: " + (i + 1) + "  # Char: " + m_textInfo.lineInfo[i].characterCount
            //                                 + "  Word Count: " + m_textInfo.lineInfo[i].wordCount
            //                                 + "  Space: " + m_textInfo.lineInfo[i].spaceCount
            //                                 + "  First:" + m_textInfo.lineInfo[i].firstCharacterIndex + "  Last [" + m_textInfo.characterInfo[m_textInfo.lineInfo[i].lastCharacterIndex].character
            //                                 + "] at Index: " + m_textInfo.lineInfo[i].lastCharacterIndex + "  Length: " + m_textInfo.lineInfo[i].lineLength 
            //                                 + " Line Extents: " + m_textInfo.lineInfo[i].lineExtents);

            //    //Debug.Log("line: " + (i + 1) + "  m_lineExtents Count: " + m_lineExtents[i].characterCount + "  lineInfo: " + m_textInfo.lineInfo[i].characterCount);
            //    //Debug.DrawLine(new Vector3(m_textInfo.lineInfo[i].lineLength, 2, 0), new Vector3(m_textInfo.lineInfo[i].lineLength, -2, 0), Color.red, 30f);
            //}
            

            // If there are no visible characters... no need to continue
            if (visibleCharacter_Count == 0)
            {
                if (m_vertices != null)
                {
                    Array.Clear(m_vertices, 0, m_vertices.Length);
                    m_mesh.vertices = m_vertices;
                }
                return;
            }

            // Store last lineInfo
            m_textInfo.lineInfo[lineNumber].lastCharacterIndex = character_Count - 1;
            m_textInfo.lineInfo[lineNumber].characterCount = m_textInfo.lineInfo[lineNumber].lastCharacterIndex - m_textInfo.lineInfo[lineNumber].firstCharacterIndex + 1;
            m_textInfo.lineInfo[lineNumber].lineLength = m_textInfo.characterInfo[character_Count - 1].topRight.x - m_padding * m_fontScale;

            int last_vert_index = visibleCharacter_Count * 4;
            // Partial clear of the vertices array to mark unused vertices as degenerate.
            Array.Clear(m_vertices, last_vert_index, m_vertices.Length - last_vert_index);


            // Add offset to position Text Anchor  
            float prev_anchor_xOffset = m_anchorOffset.x; // Cache previous Anchor position which is needed for Anchor Dampening.
            switch (m_anchor)
            {
                case AnchorPositions.TopLeft:
                    m_anchorOffset = new Vector3(0, 0 - maxAscender, 0);
                    break;

                case AnchorPositions.Left:
                    m_anchorOffset = new Vector3(0, 0 - (maxAscender + maxDescender) / 2, 0);
                    break;

                case AnchorPositions.BottomLeft:
                    m_anchorOffset = new Vector3(0, 0 - maxDescender, 0);
                    break;

                case AnchorPositions.Top:
                    m_anchorOffset = new Vector3(0 - (meshExtents.min.x + meshExtents.max.x) / 2, 0 - maxAscender, 0);
                    break;

                case AnchorPositions.Center:
                    m_anchorOffset = new Vector3(0 - (meshExtents.min.x + meshExtents.max.x) / 2, 0 - (maxAscender + maxDescender) / 2, 0);                   
                    break;

                case AnchorPositions.Bottom:
                    m_anchorOffset = new Vector3(0 - ((meshExtents.min.x + meshExtents.max.x) / 2), 0 - maxDescender, 0);
                    break;

                case AnchorPositions.TopRight:
                    m_anchorOffset = new Vector3(0 - meshExtents.max.x + m_padding * m_fontScale, 0 - maxAscender, 0);                   
                    break;

                case AnchorPositions.Right:
                    m_anchorOffset = new Vector3(0 - meshExtents.max.x + m_padding * m_fontScale, 0 - (maxAscender + maxDescender) / 2, 0);
                    break;

                case AnchorPositions.BottomRight:
                    m_anchorOffset = new Vector3(0 - meshExtents.max.x + m_padding * m_fontScale, 0 - maxDescender, 0);
                    break;
                case AnchorPositions.BaseLine:
                    m_anchorOffset = new Vector3(0, 0, 0);
                    break;
            }

            // Check how much the AnchorOffset.x has changed. If it changes by more than 1/3 of the underline character then adjust it.
            if (m_anchorDampening)
            {
                float offset_delta = Mathf.Abs(prev_anchor_xOffset - m_anchorOffset.x);
                if (prev_anchor_xOffset != 0 && offset_delta < m_cached_Underline_GlyphInfo.width * m_fontScale * 0.4f)
                    m_anchorOffset.x = prev_anchor_xOffset;
            }


            Vector3 justificationOffset = Vector3.zero;
            Vector3 offset = Vector3.zero;
            int vert_index_X4 = 0;
            //int wordCount = 0;
            int lineCount = 0;
            int lastLine = 0;          

            for (int i = 0; i < character_Count; i++)
            {
                int currentLine = m_textInfo.characterInfo[i].lineNumber;
                LineInfo lineInfo = m_textInfo.lineInfo[currentLine];         
                lineCount = currentLine + 1;

                // Process Line Justification
                switch (m_lineJustification)
                {
                    case AlignmentTypes.Left:
                        justificationOffset = Vector3.zero;
                        break;
                    case AlignmentTypes.Center:
                        justificationOffset = new Vector3((meshExtents.min.x + meshExtents.max.x) / 2 - (lineInfo.lineExtents.min.x + lineInfo.lineExtents.max.x) / 2, 0, 0);
                        break;
                    case AlignmentTypes.Right:
                        justificationOffset = new Vector3(meshExtents.max.x - lineInfo.lineExtents.max.x, 0, 0);
                        break;
                    case AlignmentTypes.Justified:
                        charCode = m_textInfo.characterInfo[i].character;
                        char lastCharOfCurrentLine = m_textInfo.characterInfo[lineInfo.lastCharacterIndex].character;
                                        
                        if (char.IsWhiteSpace(lastCharOfCurrentLine) && !char.IsControl(lastCharOfCurrentLine) && currentLine < lineNumber)
                        {   // All lines are justified accept the last one.
                            float gap = (meshExtents.max.x) - (lineInfo.lineExtents.max.x);
                            if (currentLine != lastLine || i == 0)
                                justificationOffset = Vector3.zero;
                            else
                            {
                                if (charCode == 9 || charCode == 32)
                                {                                    
                                    justificationOffset += new Vector3(gap * (1 - m_wordWrappingRatios) / (lineInfo.spaceCount - 1), 0, 0);
                                }
                                else
                                {
                                    //Debug.Log("LineInfo Character Count: " + lineInfo.characterCount);
                                    justificationOffset += new Vector3(gap * m_wordWrappingRatios / (lineInfo.characterCount - lineInfo.spaceCount - 1), 0, 0);
                                }
                            }
                        }
                        else
                            justificationOffset = Vector3.zero; // Keep last line left justified.

                        //Debug.Log("Char [" + (char)charCode + "] Code:" + charCode + "  Offset:" + justificationOffset + "  # Spaces:" + m_lineExtents[currentLine].NumberOfSpaces + "  # Characters:" + m_lineExtents[currentLine].NumberOfChars);
                        lastLine = currentLine;
                        break;
                }

                offset = m_anchorOffset + justificationOffset;

                if (m_textInfo.characterInfo[i].isVisible)
                {
                    Extents lineExtents = lineInfo.lineExtents;
                    // Setup UV2 based on Character Mapping Options Selected
                    switch (m_horizontalMapping)
                    {
                        case TextureMappingOptions.Character:
                            m_uv2s[vert_index_X4 + 0].x = 0;
                            m_uv2s[vert_index_X4 + 1].x = 0;
                            m_uv2s[vert_index_X4 + 2].x = 1;
                            m_uv2s[vert_index_X4 + 3].x = 1;
                            break;

                        case TextureMappingOptions.Line:
                            if (m_lineJustification != AlignmentTypes.Justified)
                            {
                                m_uv2s[vert_index_X4 + 0].x = (m_vertices[vert_index_X4 + 0].x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x);
                                m_uv2s[vert_index_X4 + 1].x = (m_vertices[vert_index_X4 + 1].x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x);
                                m_uv2s[vert_index_X4 + 2].x = (m_vertices[vert_index_X4 + 2].x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x);
                                m_uv2s[vert_index_X4 + 3].x = (m_vertices[vert_index_X4 + 3].x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x);
                                break;
                            }
                            else // Special Case if Justified is used in Line Mode.
                            {
                                m_uv2s[vert_index_X4 + 0].x = (m_vertices[vert_index_X4 + 0].x + justificationOffset.x - meshExtents.min.x) / (meshExtents.max.x - meshExtents.min.x);
                                m_uv2s[vert_index_X4 + 1].x = (m_vertices[vert_index_X4 + 1].x + justificationOffset.x - meshExtents.min.x) / (meshExtents.max.x - meshExtents.min.x);
                                m_uv2s[vert_index_X4 + 2].x = (m_vertices[vert_index_X4 + 2].x + justificationOffset.x - meshExtents.min.x) / (meshExtents.max.x - meshExtents.min.x);
                                m_uv2s[vert_index_X4 + 3].x = (m_vertices[vert_index_X4 + 3].x + justificationOffset.x - meshExtents.min.x) / (meshExtents.max.x - meshExtents.min.x);
                                break;
                            }

                        case TextureMappingOptions.Paragraph:
                            m_uv2s[vert_index_X4 + 0].x = (m_vertices[vert_index_X4 + 0].x + justificationOffset.x - meshExtents.min.x) / (meshExtents.max.x - meshExtents.min.x);
                            m_uv2s[vert_index_X4 + 1].x = (m_vertices[vert_index_X4 + 1].x + justificationOffset.x - meshExtents.min.x) / (meshExtents.max.x - meshExtents.min.x);
                            m_uv2s[vert_index_X4 + 2].x = (m_vertices[vert_index_X4 + 2].x + justificationOffset.x - meshExtents.min.x) / (meshExtents.max.x - meshExtents.min.x);
                            m_uv2s[vert_index_X4 + 3].x = (m_vertices[vert_index_X4 + 3].x + justificationOffset.x - meshExtents.min.x) / (meshExtents.max.x - meshExtents.min.x);
                            break;

                        case TextureMappingOptions.MatchAspect:

                            switch (m_verticalMapping)
                            {
                                case TextureMappingOptions.Character:
                                    m_uv2s[vert_index_X4 + 0].y = 0;
                                    m_uv2s[vert_index_X4 + 1].y = 1;
                                    m_uv2s[vert_index_X4 + 2].y = 0;
                                    m_uv2s[vert_index_X4 + 3].y = 1;
                                    break;

                                case TextureMappingOptions.Line:
                                    m_uv2s[vert_index_X4 + 0].y = (m_vertices[vert_index_X4].y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y);
                                    m_uv2s[vert_index_X4 + 1].y = (m_vertices[vert_index_X4 + 1].y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y);
                                    m_uv2s[vert_index_X4 + 2].y = m_uv2s[vert_index_X4].y;
                                    m_uv2s[vert_index_X4 + 3].y = m_uv2s[vert_index_X4 + 1].y;
                                    break;

                                case TextureMappingOptions.Paragraph:
                                    m_uv2s[vert_index_X4 + 0].y = (m_vertices[vert_index_X4].y - meshExtents.min.y) / (meshExtents.max.y - meshExtents.min.y);
                                    m_uv2s[vert_index_X4 + 1].y = (m_vertices[vert_index_X4 + 1].y - meshExtents.min.y) / (meshExtents.max.y - meshExtents.min.y);
                                    m_uv2s[vert_index_X4 + 2].y = m_uv2s[vert_index_X4].y;
                                    m_uv2s[vert_index_X4 + 3].y = m_uv2s[vert_index_X4 + 1].y;
                                    break;

                                case TextureMappingOptions.MatchAspect:
                                    Debug.Log("ERROR: Cannot Match both Vertical & Horizontal.");
                                    break;
                            }

                            //float xDelta = 1 - (_uv2s[vert_index + 0].y * textMeshCharacterInfo[i].AspectRatio); // Left aligned
                            float xDelta = (1 - ((m_uv2s[vert_index_X4 + 0].y + m_uv2s[vert_index_X4 + 1].y) * m_textInfo.characterInfo[i].aspectRatio)) / 2; // Center of Rectangle
                            //float xDelta = 0;

                            m_uv2s[vert_index_X4 + 0].x = (m_uv2s[vert_index_X4 + 0].y * m_textInfo.characterInfo[i].aspectRatio) + xDelta;
                            m_uv2s[vert_index_X4 + 1].x = m_uv2s[vert_index_X4 + 0].x;
                            m_uv2s[vert_index_X4 + 2].x = (m_uv2s[vert_index_X4 + 1].y * m_textInfo.characterInfo[i].aspectRatio) + xDelta;
                            m_uv2s[vert_index_X4 + 3].x = m_uv2s[vert_index_X4 + 2].x;
                            break;
                    }

                    switch (m_verticalMapping)
                    {
                        case TextureMappingOptions.Character:
                            m_uv2s[vert_index_X4 + 0].y = 0;
                            m_uv2s[vert_index_X4 + 1].y = 1;
                            m_uv2s[vert_index_X4 + 2].y = 0;
                            m_uv2s[vert_index_X4 + 3].y = 1;
                            break;

                        case TextureMappingOptions.Line:
                            m_uv2s[vert_index_X4 + 0].y = (m_vertices[vert_index_X4].y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y);
                            m_uv2s[vert_index_X4 + 1].y = (m_vertices[vert_index_X4 + 1].y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y);
                            m_uv2s[vert_index_X4 + 2].y = m_uv2s[vert_index_X4].y;
                            m_uv2s[vert_index_X4 + 3].y = m_uv2s[vert_index_X4 + 1].y;
                            break;

                        case TextureMappingOptions.Paragraph:
                            m_uv2s[vert_index_X4 + 0].y = (m_vertices[vert_index_X4].y - meshExtents.min.y) / (meshExtents.max.y - meshExtents.min.y);
                            m_uv2s[vert_index_X4 + 1].y = (m_vertices[vert_index_X4 + 1].y - meshExtents.min.y) / (meshExtents.max.y - meshExtents.min.y);
                            m_uv2s[vert_index_X4 + 2].y = m_uv2s[vert_index_X4].y;
                            m_uv2s[vert_index_X4 + 3].y = m_uv2s[vert_index_X4 + 1].y;
                            break;

                        case TextureMappingOptions.MatchAspect:
                            //float yDelta = 1 - (_uv2s[vert_index + 2].x / textMeshCharacterInfo[i].AspectRatio); // Top Corner                       
                            float yDelta = (1 - ((m_uv2s[vert_index_X4 + 0].x + m_uv2s[vert_index_X4 + 2].x) / m_textInfo.characterInfo[i].aspectRatio)) / 2; // Center of Rectangle
                            //float yDelta = 0;

                            m_uv2s[vert_index_X4 + 0].y = yDelta + (m_uv2s[vert_index_X4 + 0].x / m_textInfo.characterInfo[i].aspectRatio);
                            m_uv2s[vert_index_X4 + 1].y = yDelta + (m_uv2s[vert_index_X4 + 2].x / m_textInfo.characterInfo[i].aspectRatio);
                            m_uv2s[vert_index_X4 + 2].y = m_uv2s[vert_index_X4 + 0].y;
                            m_uv2s[vert_index_X4 + 3].y = m_uv2s[vert_index_X4 + 1].y;
                            break;
                    }


                    // Pack UV's so that we can pass Xscale needed for Shader to maintain 1:1 ratio.
                    float xScale = m_textInfo.characterInfo[i].scale * m_transform.lossyScale.z; 

                    float x0 = m_uv2s[vert_index_X4 + 0].x;
                    float y0 = m_uv2s[vert_index_X4 + 0].y;
                    float x1 = m_uv2s[vert_index_X4 + 3].x;
                    float y1 = m_uv2s[vert_index_X4 + 3].y;

                    float dx = Mathf.Floor(x0);
                    float dy = Mathf.Floor(y0);

                    x0 = x0 - dx;
                    x1 = x1 - dx;
                    y0 = y0 - dy;
                    y1 = y1 - dy;

                    m_uv2s[vert_index_X4 + 0] = PackUV(x0, y0, xScale);
                    m_uv2s[vert_index_X4 + 1] = PackUV(x0, y1, xScale);
                    m_uv2s[vert_index_X4 + 2] = PackUV(x1, y0, xScale);
                    m_uv2s[vert_index_X4 + 3] = PackUV(x1, y1, xScale);


                    // Enables control of the visibility of characters && lines.
                    if (m_maxVisibleCharacters != -1 && i >= m_maxVisibleCharacters || m_maxVisibleLines != -1 && currentLine >= m_maxVisibleLines)
                    {
                        m_vertices[vert_index_X4 + 0] *= 0;
                        m_vertices[vert_index_X4 + 1] *= 0;
                        m_vertices[vert_index_X4 + 2] *= 0;
                        m_vertices[vert_index_X4 + 3] *= 0;
                    }
                    else
                    {
                        m_vertices[vert_index_X4 + 0] += offset;
                        m_vertices[vert_index_X4 + 1] += offset;
                        m_vertices[vert_index_X4 + 2] += offset;
                        m_vertices[vert_index_X4 + 3] += offset;
                    }

                    vert_index_X4 += 4;
                }

                m_textInfo.characterInfo[i].bottomLeft += offset;
                m_textInfo.characterInfo[i].topRight += offset;
                m_textInfo.characterInfo[i].topLine += offset.y;
                m_textInfo.characterInfo[i].bottomLine += offset.y;
                m_textInfo.characterInfo[i].baseLine += offset.y;

               
                // Need to address underline and which font to use in the list
                // Create Underline Mesh
                if ((m_textInfo.characterInfo[i].style & FontStyles.Underline) == FontStyles.Underline && m_textInfo.characterInfo[i].character != 10 && m_textInfo.characterInfo[i].character != 13)
                {
                    if (beginUnderline == false)
                    {
                        beginUnderline = true;
                        underline_start = new Vector3(m_textInfo.characterInfo[i].bottomLeft.x, m_textInfo.characterInfo[i].baseLine + font.fontInfo.Underline * m_fontScale, 0);
                    }

                    if (i == character_Count - 1) // End Underline if we are at the last character.
                    {
                        underline_end = new Vector3(m_textInfo.characterInfo[i].topRight.x, m_textInfo.characterInfo[i].baseLine + font.fontInfo.Underline * m_fontScale, 0);
                        DrawUnderlineMesh(underline_start, underline_end, ref last_vert_index);
                    }
                }
                else
                {
                    if (beginUnderline == true)
                    {
                        beginUnderline = false;
                        underline_end = new Vector3(m_textInfo.characterInfo[i - 1].topRight.x, m_textInfo.characterInfo[i - 1].baseLine + font.fontInfo.Underline * m_fontScale, 0);
                        DrawUnderlineMesh(underline_start, underline_end, ref last_vert_index);
                    }
                }
            }


            // METRICS ABOUT THE TEXT OBJECT            
            m_textInfo.characterCount = (short)character_Count;
            m_textInfo.lineCount = (short)lineCount;
            m_textInfo.wordCount = (short)wordCount;

            

            //for (int i = 0; i < character_Count; i++)
            //    Debug.Log("Char [" + m_characters_Info[i].character + "] or ASCII (" + (int)m_characters_Info[i].character + ")");

            // If Advanced Layout Component is present, don't upload the mesh.
            if (m_isAdvanceLayoutComponentPresent == false || m_advancedLayoutComponent.isEnabled == false)
            {
                //Debug.Log("Uploading Mesh normally.");
                // Upload Mesh Data 
                m_mesh.MarkDynamic();

                m_mesh.vertices = m_vertices;
                m_mesh.uv = m_uvs;
                m_mesh.uv2 = m_uv2s;
                m_mesh.colors32 = m_vertColors;

                // Store Mesh information in MeshInfo
                m_textInfo.meshInfo.vertices = m_vertices;
                m_textInfo.meshInfo.uv0s = m_uvs;
                m_textInfo.meshInfo.uv2s = m_uv2s;
                m_textInfo.meshInfo.vertexColors = m_vertColors;

                // Setting Mesh Bounds manually is more efficient.               
                m_mesh.bounds = new Bounds(new Vector3((meshExtents.max.x + meshExtents.min.x) / 2, (meshExtents.max.y + meshExtents.min.y) / 2, 0) + m_anchorOffset, new Vector3(meshExtents.max.x - meshExtents.min.x, meshExtents.max.y - meshExtents.min.y, 0));
                //m_maskOffset = new Vector4(m_mesh.bounds.center.x, m_mesh.bounds.center.y, m_mesh.bounds.size.x, m_mesh.bounds.size.y);
            }
            else
            {
                UpdateMeshData(m_textInfo.characterInfo, character_Count, m_mesh, m_vertices, m_uvs, m_uv2s, m_vertColors, m_normals, m_tangents);
                m_advancedLayoutComponent.DrawMesh();
                //Debug.Log("Advanced Layout Component present & enabled. Not uploading mesh.");         
            }

            // Reset Check Line Flag which is used to determine if a property that affects linelenght has been modified.    
            isAffectingWordWrapping = false;
            //m_safetyCount = 0;
            //Debug.Log("TextMesh Pro Object updated. Line Count:" + m_textInfo.lineInfo.Length);
            //Profiler.EndSample();
            //m_StopWatch.Stop();
            //Debug.Log(m_mesh.bounds);
            //Debug.Log("Done Rendering Text.");
            //Debug.Log("TimeElapsed is:" + (m_StopWatch.ElapsedTicks / 10000f).ToString("f4"));
            //m_StopWatch.Reset();     
        }



        // Draws the Underline
        void DrawUnderlineMesh(Vector3 start, Vector3 end, ref int index)
        {

            int visibleCount = index + 12;          
            // Check to make sure our current mesh buffer allocations can hold these new Quads.  
            if (visibleCount > m_vertices.Length)
            {
                // Check to make sure we can fit underline segments.            
                SetMeshArrays(visibleCount / 4 + 16);
                // Arrays have been resized so we need to re-process the mesh
                GenerateTextMesh();
                return;
            }

            // Adjust the position of the underline based on the lowest character. This matters for subscript character.
            start.y = Mathf.Min(start.y, end.y);
            end.y = Mathf.Min(start.y, end.y);

            float segmentWidth = m_cached_Underline_GlyphInfo.width / 2 * m_fontScale;

            if (end.x - start.x < m_cached_Underline_GlyphInfo.width * m_fontScale)
            {
                segmentWidth = (end.x - start.x) / 2f;
            }
            //Debug.Log("Char H:" + cached_Underline_GlyphInfo.height);

            float underlineThickness = m_cached_Underline_GlyphInfo.height; // m_fontAsset.FontInfo.UnderlineThickness;
            // Front Part of the Underline
            m_vertices[index + 0] = start + new Vector3(0, 0 - (underlineThickness + m_padding) * m_fontScale, 0); // BL
            m_vertices[index + 1] = start + new Vector3(0, m_padding * m_fontScale, 0); // TL
            m_vertices[index + 2] = m_vertices[index + 0] + new Vector3(segmentWidth, 0, 0); // BR
            m_vertices[index + 3] = start + new Vector3(segmentWidth, m_padding * m_fontScale, 0); // TR

            // Middle Part of the Underline
            m_vertices[index + 4] = m_vertices[index + 2]; // BL
            m_vertices[index + 5] = m_vertices[index + 3]; // TL
            m_vertices[index + 6] = end + new Vector3(-segmentWidth, -(underlineThickness + m_padding) * m_fontScale, 0); // BR
            m_vertices[index + 7] = end + new Vector3(-segmentWidth, m_padding * m_fontScale, 0); // TR

            // End Part of the Underline
            m_vertices[index + 8] = m_vertices[index + 6];
            m_vertices[index + 9] = m_vertices[index + 7];
            m_vertices[index + 10] = end + new Vector3(0, -(underlineThickness + m_padding) * m_fontScale, 0);
            m_vertices[index + 11] = end + new Vector3(0, m_padding * m_fontScale, 0);


            // Calculate UV required to setup the 3 Quads for the Underline.
            Vector2 uv0 = new Vector2((m_cached_Underline_GlyphInfo.x - m_padding) / m_fontAsset.fontInfo.AtlasWidth, 1 - (m_cached_Underline_GlyphInfo.y + m_padding + m_cached_Underline_GlyphInfo.height) / m_fontAsset.fontInfo.AtlasHeight);  // bottom left
            Vector2 uv1 = new Vector2(uv0.x, 1 - (m_cached_Underline_GlyphInfo.y - m_padding) / m_fontAsset.fontInfo.AtlasHeight);  // top left
            Vector2 uv2 = new Vector2((m_cached_Underline_GlyphInfo.x + m_padding + m_cached_Underline_GlyphInfo.width / 2) / m_fontAsset.fontInfo.AtlasWidth, uv0.y); // bottom right
            Vector2 uv3 = new Vector2(uv2.x, uv1.y); // top right
            Vector2 uv4 = new Vector2((m_cached_Underline_GlyphInfo.x + m_padding + m_cached_Underline_GlyphInfo.width) / m_fontAsset.fontInfo.AtlasWidth, uv0.y); // End Part - Bottom Right
            Vector2 uv5 = new Vector2(uv4.x, uv1.y); // End Part - Top Right

            // Left Part of the Underline
            m_uvs[0 + index] = uv0; // BL
            m_uvs[1 + index] = uv1; // TL   
            m_uvs[2 + index] = uv2; // BR   
            m_uvs[3 + index] = uv3; // TR

            // Middle Part of the Underline
            m_uvs[4 + index] = new Vector2(uv2.x - uv2.x * 0.001f, uv0.y);
            m_uvs[5 + index] = new Vector2(uv2.x - uv2.x * 0.001f, uv1.y);
            m_uvs[6 + index] = new Vector2(uv2.x + uv2.x * 0.001f, uv0.y);
            m_uvs[7 + index] = new Vector2(uv2.x + uv2.x * 0.001f, uv1.y);

            // Right Part of the Underline
            m_uvs[8 + index] = uv2;
            m_uvs[9 + index] = uv3;
            m_uvs[10 + index] = uv4;
            m_uvs[11 + index] = uv5;


            // UV1 contains Face / Border UV layout.
            float min_UvX = 0;
            float max_UvX = (m_vertices[index + 2].x - start.x) / (end.x - start.x);

            //Calculate the xScale or how much the UV's are getting stretched on the X axis for the middle section of the underline.
            float xScale = m_fontScale * m_transform.lossyScale.z;
            float xScale2 = xScale;

            m_uv2s[0 + index] = PackUV(0, 0, xScale);
            m_uv2s[1 + index] = PackUV(0, 1, xScale);
            m_uv2s[2 + index] = PackUV(max_UvX, 0, xScale);
            m_uv2s[3 + index] = PackUV(max_UvX, 1, xScale);

            min_UvX = (m_vertices[index + 4].x - start.x) / (end.x - start.x);
            max_UvX = (m_vertices[index + 6].x - start.x) / (end.x - start.x);

            m_uv2s[4 + index] = PackUV(min_UvX, 0, xScale2);
            m_uv2s[5 + index] = PackUV(min_UvX, 1, xScale2);
            m_uv2s[6 + index] = PackUV(max_UvX, 0, xScale2);
            m_uv2s[7 + index] = PackUV(max_UvX, 1, xScale2);

            min_UvX = (m_vertices[index + 8].x - start.x) / (end.x - start.x);
            max_UvX = (m_vertices[index + 6].x - start.x) / (end.x - start.x);

            m_uv2s[8 + index] = PackUV(min_UvX, 0, xScale);
            m_uv2s[9 + index] = PackUV(min_UvX, 1, xScale);
            m_uv2s[10 + index] = PackUV(1, 0, xScale);
            m_uv2s[11 + index] = PackUV(1, 1, xScale);


            Color32 underlineColor = new Color32(m_fontColor.r, m_fontColor.g, m_fontColor.b, (byte)(m_fontColor.a / 2));

            m_vertColors[0 + index] = underlineColor;
            m_vertColors[1 + index] = underlineColor;
            m_vertColors[2 + index] = underlineColor;
            m_vertColors[3 + index] = underlineColor;

            m_vertColors[4 + index] = underlineColor;
            m_vertColors[5 + index] = underlineColor;
            m_vertColors[6 + index] = underlineColor;
            m_vertColors[7 + index] = underlineColor;

            m_vertColors[8 + index] = underlineColor;
            m_vertColors[9 + index] = underlineColor;
            m_vertColors[10 + index] = underlineColor;
            m_vertColors[11 + index] = underlineColor;

            index += 12;
        }


        // Used with Advanced Layout Component.
        void UpdateMeshData(TMPro_CharacterInfo[] characterInfo, int characterCount, Mesh mesh, Vector3[] vertices, Vector2[] uv0s, Vector2[] uv2s, Color32[] vertexColors, Vector3[] normals, Vector4[] tangents)
        {
            m_textInfo.characterInfo = characterInfo;
            m_textInfo.characterCount = (short)characterCount;
            //m_meshInfo.mesh = mesh;
            m_meshInfo.vertices = vertices;
            m_meshInfo.uv0s = uv0s;
            m_meshInfo.uv2s = uv2s;
            m_meshInfo.vertexColors = m_vertColors;
            m_meshInfo.normals = normals;
            m_meshInfo.tangents = tangents;
        }


        // Function to pack scale information in the UV2 Channel.
        Vector2 PackUV(float x, float y, float scale)
        {
            x = (x % 4) / 4;
            y = (y % 4) / 4;

            return new Vector2(Mathf.Round(x * 4096) + y, scale);
        }


        // Function to increase the size of the Line Extents Array.
        void ResizeLineExtents(int size)
        {
            size = size > 1024 ? size + 256 : Mathf.NextPowerOfTwo(size + 1);
           
            LineInfo[] temp_lineInfo = new LineInfo[size];
            for (int i = 0; i < m_textInfo.lineInfo.Length; i++)                         
                temp_lineInfo[i] = m_textInfo.lineInfo[i];
                    
            m_textInfo.lineInfo = temp_lineInfo;
        }


        // Convert HEX to INT
        int HexToInt(char hex)
        {
            switch (hex)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'A': return 10;
                case 'B': return 11;
                case 'C': return 12;
                case 'D': return 13;
                case 'E': return 14;
                case 'F': return 15;
                case 'a': return 10;
                case 'b': return 11;
                case 'c': return 12;
                case 'd': return 13;
                case 'e': return 14;
                case 'f': return 15;
            }
            return 15;
        }


        Color32 HexCharsToColor(char[] hexChars)
        {
            if (hexChars.Length == 7)
            {
                byte r = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[2]));
                byte g = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[4]));
                byte b = (byte)(HexToInt(hexChars[5]) * 16 + HexToInt(hexChars[6]));

                return new Color32(r, g, b, 255);
            }
            else
            {
                byte r = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[2]));
                byte g = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[4]));
                byte b = (byte)(HexToInt(hexChars[5]) * 16 + HexToInt(hexChars[6]));
                byte a = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[8]));

                return new Color32(r, g, b, a);
            }
        }


        /// <summary>
        /// Extracts a float value from char[] assuming we know the position of the start, end and decimal point.
        /// </summary>
        /// <param name="chars"></param> The Char[] containing the numerical sequence.
        /// <param name="startIndex"></param> The index of the start of the numerical sequence.
        /// <param name="endIndex"></param> The index of the last number in the numerical sequence.
        /// <param name="decimalPointIndex"></param> The index of the decimal point if any.
        /// <returns></returns>
        float ConvertToFloat(char[] chars, int startIndex, int endIndex, int decimalPointIndex)
        {
            float v = 0;
            float sign = 1;
            decimalPointIndex = decimalPointIndex > 0 ? decimalPointIndex : endIndex + 1; // Check in case we don't have any decimal point

            // Check if negative value
            if (chars[startIndex] == 45) // '-'
            {
                startIndex += 1;
                sign = -1;
            }

            if (chars[startIndex] == 43) startIndex += 1; // '+'
              
              
            for (int i = startIndex; i < endIndex + 1; i++)
            {
                switch (decimalPointIndex - i)
                {
                    case 4:
                        v += (chars[i] - 48) * 1000;
                        break;
                    case 3:
                        v += (chars[i] - 48) * 100;
                        break;
                    case 2:
                        v += (chars[i] - 48) * 10;
                        break;
                    case 1:
                        v += (chars[i] - 48);
                        break;
                    case -1:
                        v += (chars[i] - 48) * 0.1f;
                        break;
                    case -2:
                        v += (chars[i] - 48) * 0.01f;
                        break;
                    case -3:
                        v += (chars[i] - 48) * 0.001f;
                        break;
                }
            }
            return v * sign;
        }


        // Function to identify and validate the rich tag. Returns the position of the > if the tag was valid.
        bool ValidateHtmlTag(int[] chars, int startIndex, out int endIndex)
        {
            Array.Clear(m_htmlTag, 0, 16);
            int tagCharCount = 0;
            int tagCode = 0;
            int colorCode = 0;
            int numSequenceStart = 0;
            int numSequenceEnd = 0;
            int numSequenceDecimalPos = 0;

            endIndex = startIndex;

            bool isValidHtmlTag = false;
            int equalSignValue = 1;

            for (int i = startIndex; chars[i] != 0 && tagCharCount < 16 && chars[i] != 60; i++)
            {
                if (chars[i] == 62) // ASC Code of End Html tag '>'
                {
                    isValidHtmlTag = true;
                    endIndex = i;
                    m_htmlTag[tagCharCount] = (char)0;
                    break;
                }

                m_htmlTag[tagCharCount] = (char)chars[i];
                tagCharCount += 1;

                if (chars[i] == 61) equalSignValue = 0; // Once we encounter the equal sign, we stop adding the tagCode.
                
                tagCode += chars[i] * tagCharCount * equalSignValue;
                colorCode += chars[i] * tagCharCount * (1 - equalSignValue);
                // Get possible positions of numerical values 
                switch ((int)chars[i])
                {
                    case 61: // '='
                        numSequenceStart = tagCharCount;
                        break;
                    case 46: // '.'
                        numSequenceDecimalPos = tagCharCount - 1;
                        break;
                }
            }

            if (!isValidHtmlTag)
            {
                return false;
            }

            //Debug.Log("Tag Code:" + tagCode + "  Color Code: " + colorCode);

            if (m_htmlTag[0] == 35 && tagCharCount == 7) // if Tag begins with # and contains 7 characters. 
            {
                m_htmlColor = HexCharsToColor(m_htmlTag);
                return true;
            }
            else if (m_htmlTag[0] == 35 && tagCharCount == 9) // if Tag begins with # and contains 9 characters. 
            { 
                m_htmlColor = HexCharsToColor(m_htmlTag);              
                return true;
            }                     
            else
            {
                switch (tagCode)
                {
                    case 98: // <b>
                        m_style |= FontStyles.Bold;
                        return true;
                    case 105: // <i>
                        m_style |= FontStyles.Italic;
                        return true;
                    case 117: // <u>
                        m_style |= FontStyles.Underline;
                        return true;
                    case 243: // </b>
                        m_style &= ~FontStyles.Bold;
                        return true;
                    case 257: // </i>
                        m_style &= ~FontStyles.Italic;
                        return true;
                    case 281: // </u>
                        m_style &= ~FontStyles.Underline;
                        return true;
                    case 643: // <sub>
                        m_currentFontSize *= 0.5f; // Subscript characters are half size.
                        m_xAdvance += 10 * m_fontScale;
                        m_baselineOffset = -10;
                        m_isRecalculateScaleRequired = true;
                        return true;
                    case 679: // <pos=000.00>
                        m_tabSpacing = ConvertToFloat(m_htmlTag, numSequenceStart, tagCharCount - 1, numSequenceDecimalPos);             
                        return true;
                    case 685: // <sup>
                        m_currentFontSize *= 0.5f;
                        m_xAdvance += 10 * m_fontScale;
                        m_baselineOffset = m_fontAsset.fontInfo.Ascender;
                        m_isRecalculateScaleRequired = true;
                        return true;
                    case 1020: // </sub>
                        m_currentFontSize *= 2; //m_fontSize / m_fontAsset.FontInfo.PointSize * .1f;
                        m_baselineOffset = 0;
                        m_isRecalculateScaleRequired = true;
                        return true;
                    case 1076: // </sup>
                        m_currentFontSize *= 2; //m_fontSize / m_fontAsset.FontInfo.PointSize * .1f;
                        m_baselineOffset = 0;
                        m_isRecalculateScaleRequired = true;
                        return true;
                    case 1095: // <size=>
                        numSequenceEnd = tagCharCount - 1;
                        float val = 0;

                        if (m_htmlTag[5] == 43) // <size=+00>
                        {
                            val = ConvertToFloat(m_htmlTag, numSequenceStart, numSequenceEnd, numSequenceDecimalPos);
                            m_currentFontSize = m_fontSize + val;                                       
                            m_isRecalculateScaleRequired = true;                     
                            return true;
                        }
                        else if (m_htmlTag[5] == 45) // <size=-00>
                        {
                            val = ConvertToFloat(m_htmlTag, numSequenceStart, numSequenceEnd, numSequenceDecimalPos);
                            m_currentFontSize = m_fontSize + val;
                            m_isRecalculateScaleRequired = true;
                            return true;
                        }
                        else // <size=0000.00>
                        {
                            val = ConvertToFloat(m_htmlTag, numSequenceStart, numSequenceEnd, numSequenceDecimalPos);
                            m_currentFontSize = val;
                            m_isRecalculateScaleRequired = true;                  
                            return true;
                        } 
                    case 1118: // <font=xx>
                        Debug.Log("Font Tag used.");
                        //m_fontIndex = (int)ConvertToFloat(m_htmlTag, numSequenceStart, tagCharCount - 1, numSequenceDecimalPos);

                        //if(m_fontAssetArray[m_fontIndex] == null)
                        //{
                        //    // Load new font asset into index
                        //    m_fontAssetArray[m_fontIndex] = Resources.Load("Fonts/Bangers SDF", typeof(TextMeshProFont)) as TextMeshProFont; // Hard coded right now to a specific font
                        //}

                        //m_fontScale = (m_fontSize / m_fontAssetArray[m_fontIndex].fontInfo.PointSize * (m_isOrthographic ? 1 : 0.1f));
                        //Debug.Log("Font Index = " + m_fontIndex);
                        return true;
                    case 1531: // <space=000.00>
                        m_spacing = ConvertToFloat(m_htmlTag, numSequenceStart, tagCharCount - 1, numSequenceDecimalPos);              
                        return true;                      
                    case 1585: // </size>
                        m_currentFontSize = m_fontSize;
                        m_isRecalculateScaleRequired = true;
                        //m_fontScale = m_fontSize / m_fontAsset.fontInfo.PointSize * .1f;
                        return true;
                    case 1659: // <color=>
                        switch (colorCode)
                        {
                            case 2872: // <color=red>                            
                                m_htmlColor = Color.red;
                                return true;
                            case 3979: // <color=blue>
                                m_htmlColor = Color.blue;
                                return true;
                            case 4956: // <color=black>
                                m_htmlColor = Color.black;
                                return true;
                            case 5128: // <color=green>
                                m_htmlColor = Color.green;
                                return true;
                            case 5247: // <color=white>
                                m_htmlColor = Color.white;
                                return true;
                            case 6373: // <color=orange>
                                m_htmlColor = new Color32(255, 128, 0, 255);
                                return true;
                            case 6632: // <color=purple>
                                m_htmlColor = new Color32(160, 32, 240, 255);
                                return true;
                            case 6722: // <color=yellow>
                                m_htmlColor = Color.yellow;
                                return true;
                        }
                        return false;
                    case 2249: // </color>
                        m_htmlColor = m_fontColor;
                        return true;
                    case 2287: // <sprite=x>
                        Debug.Log("Sprite Tag used.");              
                        return true;                 
                }
            }

            return false;
        }
    }
}