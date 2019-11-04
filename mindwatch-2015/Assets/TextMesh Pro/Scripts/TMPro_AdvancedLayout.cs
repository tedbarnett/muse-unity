// Copyright (C) 2014 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using System.Collections;


namespace TMPro
{

    [ExecuteInEditMode]
    public class TMPro_AdvancedLayout : MonoBehaviour
    {

        public bool isEnabled
        {
            get { return m_isEnabled; }
            set { m_textMeshProComponent.isAdvancedLayoutComponentPresent = value == true ? true : false; }
        }
        [SerializeField]
        private bool m_isEnabled = true;
        [SerializeField]

        private bool isRedrawRequired;

        public AnimationCurve TranslationCurve;
        //public AnimationCurve RotationCurve;
        public AnimationCurve ScaleCurve;



        private TextMeshPro m_textMeshProComponent;
        private Renderer m_renderer;
        private MeshFilter m_meshFilter;
        private Material m_sharedMaterial;
        private Mesh m_mesh;


        [SerializeField]
        private bool propertiesChanged;


        void Awake()
        {
            Debug.Log("Advanced Layout Component was added.");

            m_renderer = GetComponent<Renderer>();
            //m_meshFilter = GetComponent<MeshFilter>();
            //m_mesh = m_meshFilter.sharedMesh;
            m_sharedMaterial = m_renderer.sharedMaterial;

            // Get Reference to the TextMeshPro Component     
            m_textMeshProComponent = GetComponent<TextMeshPro>();
            m_textMeshProComponent.isAdvancedLayoutComponentPresent = true;
        }




        void OnDestroy()
        {
            m_textMeshProComponent.isAdvancedLayoutComponentPresent = false;
            Debug.Log("Advanced Layout Component was removed.");
        }


        void Update()
        {
            if (propertiesChanged)
            {
                m_textMeshProComponent.isAdvancedLayoutComponentPresent = m_isEnabled;

                // Make sure TextMeshPro Component settings are correct.
                if (m_isEnabled)
                {
                    m_textMeshProComponent.enableWordWrapping = false;
                    m_textMeshProComponent.alignment = AlignmentTypes.Left;
                    m_textMeshProComponent.anchor = AnchorPositions.BaseLine;
                }

                propertiesChanged = false;
            }

            if (isRedrawRequired)
            {
                //Debug.Log("TMPro Mesh has been updated.");
                //DrawMesh();
                //isRedrawRequired = false;
            }

            //DrawMesh();

        }


        //public void RedrawMesh()
        //{
        //    isRedrawRequired = true;
        //}


        public void DrawMesh()
        {
            Matrix4x4 matrix;


            TMPro_MeshInfo meshInfos = m_textMeshProComponent.meshInfo;
            TextInfo textInfos = m_textMeshProComponent.textInfo;
            TMPro_CharacterInfo[] characters_Info = textInfos.characterInfo;
            int characterCount = textInfos.characterCount;
            Vector3[] vertices = meshInfos.vertices;
            Vector2[] uv0s = meshInfos.uv0s;
            Vector2[] uv2s = meshInfos.uv2s;
            Color32[] vertexColors = meshInfos.vertexColors;
            Vector3[] normals = meshInfos.normals;
            Vector4[] tangents = meshInfos.tangents;

            float curveScale = 1;
            char charCode;
            float xScale = 0;
            float xAdvance = 0;
            GlyphInfo glyph;

            // Draw Scale Changes First        
            for (int i = 0; i < characterCount; i++)
            {
                charCode = characters_Info[i].character;
                glyph = m_textMeshProComponent.font.characterDictionary[charCode];

                int vertIndex = characters_Info[i].vertexIndex;
                if (characters_Info[i].isVisible)
                {

                    float charMidLine = (characters_Info[i].bottomLeft.x + characters_Info[i].topRight.x) / 2; // Middle X-Axis of character.          

                    // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
                    Vector3 offset = new Vector3(charMidLine, 0, 0);
                    vertices[vertIndex + 0] += -offset;
                    vertices[vertIndex + 1] += -offset;
                    vertices[vertIndex + 2] += -offset;
                    vertices[vertIndex + 3] += -offset;

                    // Apply Scale Matrix first
                    float x = characterCount > 1 ? (float)i / (characterCount - 1) : 0;
                    xScale = ScaleCurve.Evaluate(x);

                    matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), new Vector3(xScale, xScale, 1));

                    vertices[vertIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertIndex + 0]);
                    vertices[vertIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertIndex + 1]);
                    vertices[vertIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertIndex + 2]);
                    vertices[vertIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertIndex + 3]);

                    // Redo the spacing for each character.
                    float adjusted_pos = glyph.xOffset * characters_Info[i].scale * xScale + (characters_Info[i].topRight.x - characters_Info[i].bottomLeft.x) / 2 * xScale;

                    offset = new Vector3(adjusted_pos + xAdvance, 0, 0);
                    vertices[vertIndex + 0] += offset;
                    vertices[vertIndex + 1] += offset;
                    vertices[vertIndex + 2] += offset;
                    vertices[vertIndex + 3] += offset;

                    // Update Scale packed in UV2 for SDF Shader. // Skip this for surface shader.
                    uv2s[vertIndex + 0].y *= xScale;
                    uv2s[vertIndex + 1].y *= xScale;
                    uv2s[vertIndex + 2].y *= xScale;
                    uv2s[vertIndex + 3].y *= xScale;
                }

                xAdvance += glyph.xAdvance * characters_Info[i].scale * xScale + m_textMeshProComponent.characterSpacing;
            }

            int lastIndex = characters_Info[characterCount - 1].vertexIndex;
            float startPos = vertices[0].x;
            float lastCharPos = vertices[lastIndex + 2].x; // Position of middle of last character.

            for (int i = 0; i < characterCount; i++)
            {
                int vertIndex = characters_Info[i].vertexIndex;

                if (characters_Info[i].isVisible)
                {

                    float charMidLine = (vertices[vertIndex + 0].x + vertices[vertIndex + 2].x) / 2; // Middle X-Axis of character.          

                    // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
                    Vector3 offset = new Vector3(charMidLine, 0, 0);
                    vertices[vertIndex + 0] += -offset;
                    vertices[vertIndex + 1] += -offset;
                    vertices[vertIndex + 2] += -offset;
                    vertices[vertIndex + 3] += -offset;

                    float x0 = (charMidLine - startPos) / (lastCharPos - startPos); // Character position relative to X-Axis normalized (0 - 1) with curve.                                  
                    float x1 = x0 + 0.0001f;
                    float y0 = TranslationCurve.Evaluate(x0) * curveScale; // Y position for middle of character.
                    float y1 = TranslationCurve.Evaluate(x1) * curveScale;

                    Vector3 horizontal = new Vector3(1, 0, 0);
                    Vector3 normal = new Vector3(-(y1 - y0), (x1 * (lastCharPos - startPos) + startPos) - charMidLine, 0);
                    //Vector3 dir = new Vector3(x1 * (lastCharPos - startPos) + startPos, y1) - new Vector3(charMidLine, y0);

                    //Debug.DrawLine(new Vector3(charMidLine, y0, 0), new Vector3(charMidLine, y0, 0) + dir * 1000, Color.green, 60);
                    Debug.DrawLine(new Vector3(charMidLine, y0, 0), normal, Color.green, 60);

                    Vector3 tangent = new Vector3(x1 * (lastCharPos - startPos) + startPos, y1) - new Vector3(charMidLine, y0);

                    //Debug.Log(((x1 * (lastCharPos - startPos) + startPos) - charMidLine));

                    float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;

                    Vector3 cross = Vector3.Cross(horizontal, tangent);
                    float angle = cross.z > 0 ? dot : 360 - dot;

                    matrix = Matrix4x4.TRS(new Vector3(0, y0, 0), Quaternion.Euler(0, 0, angle), new Vector3(1, 1, 1));

                    vertices[vertIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertIndex + 0]);
                    vertices[vertIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertIndex + 1]);
                    vertices[vertIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertIndex + 2]);
                    vertices[vertIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertIndex + 3]);

                    //offset = new Vector3(adjusted_pos + xAdvance, 0, 0);
                    vertices[vertIndex + 0] += offset;
                    vertices[vertIndex + 1] += offset;
                    vertices[vertIndex + 2] += offset;
                    vertices[vertIndex + 3] += offset;

                }
            }


            // Upload Mesh Data
            Mesh mesh = m_textMeshProComponent.mesh;
            mesh.vertices = vertices;
            mesh.uv = uv0s;
            mesh.uv2 = uv2s;
            mesh.colors32 = vertexColors;

            // If Surface Shader, upload Normals & Tangents to the mesh.
            if (m_sharedMaterial.HasProperty(ShaderUtilities.ID_Shininess))
            {
                mesh.normals = normals;
                mesh.tangents = tangents;
            }

            mesh.RecalculateBounds();
            // Setting Mesh Bounds manually is more efficient.
            //m_mesh.bounds = new Bounds(new Vector3((meshExtents.Max.x + meshExtents.Min.x) / 2, (meshExtents.Max.y + meshExtents.Min.y) / 2, 0) + anchorOffset, new Vector3(meshExtents.Max.x - meshExtents.Min.x, meshExtents.Max.y - meshExtents.Min.y, 0));
        }
    }
}