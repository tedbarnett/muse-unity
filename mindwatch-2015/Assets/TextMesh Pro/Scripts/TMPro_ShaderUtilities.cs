// Copyright (C) 2014 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using System.Linq;
using System.Collections;


namespace TMPro
{
    
    public static class ShaderUtilities
    {
        // Shader Property IDs
        public static int ID_MainTex = Shader.PropertyToID("_MainTex");

        public static int ID_FaceColor = Shader.PropertyToID("_FaceColor");
        public static int ID_FaceDilate = Shader.PropertyToID("_FaceDilate");
        public static int ID_Shininess = Shader.PropertyToID("_FaceShininess");

        public static int ID_UnderlayOffsetX = Shader.PropertyToID("_UnderlayOffsetX");
        public static int ID_UnderlayOffsetY = Shader.PropertyToID("_UnderlayOffsetY");
        public static int ID_UnderlayDilate = Shader.PropertyToID("_UnderlayDilate");
        public static int ID_UnderlaySoftness = Shader.PropertyToID("_UnderlaySoftness");

        public static int ID_WeightNormal = Shader.PropertyToID("_WeightNormal");
        public static int ID_WeightBold = Shader.PropertyToID("_WeightBold");


        public static int ID_OutlineWidth = Shader.PropertyToID("_OutlineWidth");
        public static int ID_OutlineSoftness = Shader.PropertyToID("_OutlineSoftness");
        public static int ID_OutlineColor = Shader.PropertyToID("_OutlineColor");
        
        public static int ID_GradientScale = Shader.PropertyToID("_GradientScale");
        public static int ID_ScaleX = Shader.PropertyToID("_ScaleX");
        public static int ID_ScaleY = Shader.PropertyToID("_ScaleY");
        public static int ID_PerspectiveFilter = Shader.PropertyToID("_PerspectiveFilter");

        public static int ID_TextureWidth = Shader.PropertyToID("_TextureWidth");
        public static int ID_TextureHeight = Shader.PropertyToID("_TextureHeight");

        public static int ID_BevelAmount = Shader.PropertyToID("_Bevel");

        public static int ID_GlowColor = Shader.PropertyToID("_GlowColor");
        public static int ID_GlowOffset = Shader.PropertyToID("_GlowOffset");
        public static int ID_GlowPower = Shader.PropertyToID("_GlowPower");      
        public static int ID_GlowOuter = Shader.PropertyToID("_GlowOuter");
       
        public static int ID_LightAngle = Shader.PropertyToID("_LightAngle");

        public static int ID_MaskCoord = Shader.PropertyToID("_MaskCoord");
        public static int ID_MaskSoftnessX = Shader.PropertyToID("_MaskSoftnessX");
        public static int ID_MaskSoftnessY = Shader.PropertyToID("_MaskSoftnessY");
        public static int ID_VertexOffsetX = Shader.PropertyToID("_VertexOffsetX");
        public static int ID_VertexOffsetY = Shader.PropertyToID("_VertexOffsetY");
        

        public static int ID_ShaderFlags = Shader.PropertyToID("_ShaderFlags");
        public static int ID_ScaleRatio_A = Shader.PropertyToID("_ScaleRatioA");
        public static int ID_ScaleRatio_B = Shader.PropertyToID("_ScaleRatioB");
        public static int ID_ScaleRatio_C = Shader.PropertyToID("_ScaleRatioC");
        
        public static string Keyword_Bevel = "BEVEL_ON";
        public static string Keyword_Glow = "GLOW_ON";
        public static string Keyword_Underlay = "UNDERLAY_ON";
        public static string Keyword_Ratios = "RATIOS_OFF";

        private static float m_clamp = 1.0f;

        // Scale Ratios to ensure property ranges are optimum in Material Editor  
        public static void UpdateShaderRatios(Material mat, bool isBold)
        {
            float ratio_A = 1;
            float ratio_B = 1;
            float ratio_C = 1;

            bool isRatioEnabled = !mat.shaderKeywords.Contains(Keyword_Ratios);

            // Compute Ratio A
            float scale = mat.GetFloat(ID_GradientScale);
            float faceDilate = mat.GetFloat(ID_FaceDilate);
            float outlineThickness = mat.GetFloat(ID_OutlineWidth);
            float outlineSoftness = mat.GetFloat(ID_OutlineSoftness);
            float weight = !isBold ? mat.GetFloat(ID_WeightNormal) * 2 / scale : mat.GetFloat(ID_WeightBold) * 2 / scale;        
           
            float t = Mathf.Max(1, weight + faceDilate + outlineThickness + outlineSoftness);

            ratio_A = isRatioEnabled ? (scale - m_clamp) / (scale * t) : 1;
            mat.SetFloat(ID_ScaleRatio_A, ratio_A);

            // Compute Ratio B
            if (mat.HasProperty(ID_GlowOffset))
            {
                float glowOffset = mat.GetFloat(ID_GlowOffset);
                float glowOuter = mat.GetFloat(ID_GlowOuter);
                float range = (weight + faceDilate) * (scale - m_clamp);              
 
                t = Mathf.Max(1, glowOffset + glowOuter);

                ratio_B = isRatioEnabled ? Mathf.Max(0, scale - m_clamp - range) / (scale * t) : 1;
                mat.SetFloat(ID_ScaleRatio_B, ratio_B);
            }

            // Compute Ratio C
            if (mat.HasProperty(ID_UnderlayOffsetX))
            {
                float underlayOffsetX = mat.GetFloat(ID_UnderlayOffsetX);
                float underlayOffsetY = mat.GetFloat(ID_UnderlayOffsetY);
                float underlayDilate = mat.GetFloat(ID_UnderlayDilate);
                float underlaySoftness = mat.GetFloat(ID_UnderlaySoftness);

                float range = (weight + faceDilate) * (scale - m_clamp);
                t = Mathf.Max(1, Mathf.Max(Mathf.Abs(underlayOffsetX), Mathf.Abs(underlayOffsetY)) + underlayDilate + underlaySoftness);

                ratio_C = isRatioEnabled ? Mathf.Max(0, scale - m_clamp - range) / (scale * t) : 1;
                mat.SetFloat(ID_ScaleRatio_C, ratio_C);
            }
        }



        // Function to calculate padding required for Outline Width & Dilation for proper text alignment
        public static Vector4 GetFontExtent(Material material)
        {
            if (!material.HasProperty(ShaderUtilities.ID_GradientScale))
                return Vector4.zero;   // We are using an non SDF Shader.
            
            float scaleRatioA = material.GetFloat(ID_ScaleRatio_A);
            float faceDilate = material.GetFloat(ID_FaceDilate) * scaleRatioA;
            float outlineThickness = material.GetFloat(ID_OutlineWidth) * scaleRatioA;

            float extent = Mathf.Min(1, faceDilate + outlineThickness);
            extent *= material.GetFloat(ID_GradientScale);

            return new Vector4(extent, extent, extent, extent);
        }



        // Function to determine how much extra padding is required as a result of material properties like dilate, outline thickness, softness, glow, etc...
        public static float GetPadding(Material[] materials, bool enableExtraPadding, bool isBold)
        {           
            int extraPadding = enableExtraPadding ? 4 : 0; 

            if (!materials[0].HasProperty(ShaderUtilities.ID_GradientScale))
                return extraPadding;   // We are using an non SDF Shader.

            Vector4 padding = Vector4.zero;
            Vector4 maxPadding = Vector4.zero;

            float faceDilate = 0;
            float faceSoftness = 0;
            float outlineThickness = 0;
            float scaleRatio_A = 0;
            float scaleRatio_B = 0;
            float scaleRatio_C = 0;

            float glowOffset = 0;
            float glowOuter = 0;

            float uniformPadding = 0;
            // Iterate through each of the assigned materials to find the max values to set the padding.
            for (int i = 0; i < materials.Length; i++)
            {            
                // Update Shader Ratios prior to computing padding
                ShaderUtilities.UpdateShaderRatios(materials[i], isBold);

                string[] shaderKeywords = materials[i].shaderKeywords;

                if (materials[i].HasProperty(ShaderUtilities.ID_ScaleRatio_A))
                    scaleRatio_A = materials[i].GetFloat(ShaderUtilities.ID_ScaleRatio_A);

                if (materials[i].HasProperty(ShaderUtilities.ID_FaceDilate))
                    faceDilate = materials[i].GetFloat(ShaderUtilities.ID_FaceDilate) * scaleRatio_A;

                if (materials[i].HasProperty(ShaderUtilities.ID_OutlineSoftness))
                    faceSoftness = materials[i].GetFloat(ShaderUtilities.ID_OutlineSoftness) * scaleRatio_A;

                if (materials[i].HasProperty(ShaderUtilities.ID_OutlineWidth))
                    outlineThickness = materials[i].GetFloat(ShaderUtilities.ID_OutlineWidth) * scaleRatio_A;

                uniformPadding = outlineThickness + faceSoftness + faceDilate;

                // Glow padding contribution
                if (materials[i].HasProperty(ShaderUtilities.ID_GlowOffset) && shaderKeywords.Contains(ShaderUtilities.Keyword_Glow))
                {
                    if (materials[i].HasProperty(ShaderUtilities.ID_ScaleRatio_B))
                        scaleRatio_B = materials[i].GetFloat(ShaderUtilities.ID_ScaleRatio_B);

                    glowOffset = materials[i].GetFloat(ShaderUtilities.ID_GlowOffset) * scaleRatio_B;
                    glowOuter = materials[i].GetFloat(ShaderUtilities.ID_GlowOuter) * scaleRatio_B;
                }

                uniformPadding = Mathf.Max(uniformPadding, faceDilate + glowOffset + glowOuter);

                // Underlay padding contribution
                if (materials[i].HasProperty(ShaderUtilities.ID_UnderlaySoftness) && shaderKeywords.Contains(ShaderUtilities.Keyword_Underlay))
                {
                    if (materials[i].HasProperty(ShaderUtilities.ID_ScaleRatio_C))
                        scaleRatio_C = materials[i].GetFloat(ShaderUtilities.ID_ScaleRatio_C);

                    float offsetX = materials[i].GetFloat(ShaderUtilities.ID_UnderlayOffsetX) * scaleRatio_C;
                    float offsetY = materials[i].GetFloat(ShaderUtilities.ID_UnderlayOffsetY) * scaleRatio_C;
                    float dilate = materials[i].GetFloat(ShaderUtilities.ID_UnderlayDilate) * scaleRatio_C;
                    float softness = materials[i].GetFloat(ShaderUtilities.ID_UnderlaySoftness) * scaleRatio_C;

                    padding.x = Mathf.Max(padding.x, faceDilate + dilate + softness - offsetX);
                    padding.y = Mathf.Max(padding.y, faceDilate + dilate + softness - offsetY);
                    padding.z = Mathf.Max(padding.z, faceDilate + dilate + softness + offsetX);
                    padding.w = Mathf.Max(padding.w, faceDilate + dilate + softness + offsetY);
                }

                padding.x = Mathf.Max(padding.x, uniformPadding);
                padding.y = Mathf.Max(padding.y, uniformPadding);
                padding.z = Mathf.Max(padding.z, uniformPadding);
                padding.w = Mathf.Max(padding.w, uniformPadding);

                padding.x += extraPadding;
                padding.y += extraPadding;
                padding.z += extraPadding;
                padding.w += extraPadding;

                padding.x = Mathf.Min(padding.x, 1);
                padding.y = Mathf.Min(padding.y, 1);
                padding.z = Mathf.Min(padding.z, 1);
                padding.w = Mathf.Min(padding.w, 1);

                maxPadding.x = maxPadding.x < padding.x ? padding.x : maxPadding.x;
                maxPadding.y = maxPadding.y < padding.y ? padding.y : maxPadding.y;
                maxPadding.z = maxPadding.z < padding.z ? padding.z : maxPadding.z;
                maxPadding.w = maxPadding.w < padding.w ? padding.w : maxPadding.w;

            }

            float gradientScale = materials[0].GetFloat(ShaderUtilities.ID_GradientScale);
            padding *= gradientScale;

            // Set UniformPadding to the maximum value of any of its components.
            uniformPadding = Mathf.Max(padding.x, padding.y);
            uniformPadding = Mathf.Max(padding.z, uniformPadding);
            uniformPadding = Mathf.Max(padding.w, uniformPadding);

            return uniformPadding + 0.25f;
        }


    }

}
