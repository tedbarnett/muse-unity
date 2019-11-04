// Copyright (C) 2014 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using UnityEditor;
using System.Threading;
using System.IO;


namespace TMPro.EditorUtilities
{
    public static class TMPro_DistanceTransform
    {
        private static EventWaitHandle[] _WaitHandles;

        private static Color[] Output;

        private static System.Diagnostics.Stopwatch timer;

        private class Pixel
        {
            public float alpha;
            public float distance;
            public int dX, dY;
            public Vector2 gradient;
        }


        private static Pixel[] innerMask;
        private static Pixel[] outerMask;


        // Use this for initialization
        public static void Generate(Color[] Source, int width, int height, int Spread, int ScaleDownFactor)
        {
            // Setup Timer for debug purposes     
            timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            // Pixel Arrays which will contain the results of the distance transform of the Inner & Outer masks.
            innerMask = new Pixel[width * height];
            outerMask = new Pixel[width * height];


            // Event Handler for ThreadPool.
            _WaitHandles = new EventWaitHandle[] { new AutoResetEvent(false), new AutoResetEvent(false) };

            // Calculate Distance Transform for Inner Mask
            ThreadPool.QueueUserWorkItem(InnerMask =>
            {
                //System.TimeSpan timeStamp = timer.Elapsed;

                innerMask = Calculate3X3EDT(Source, 1, width, height, Spread);
                //Debug.Log("Inner Mask processed in [" + (timer.Elapsed - timeStamp) + "]");        

                _WaitHandles[0].Set();
            });

            // Calculate Distance Transform for Outer Mask
            ThreadPool.QueueUserWorkItem(OuterMask =>
            {
                //System.TimeSpan timeStamp = timer.Elapsed;

                outerMask = Calculate3X3EDT(Source, 0, width, height, Spread);
                //Debug.Log("Outer Mask processed in [" + (timer.Elapsed - timeStamp) + "]");

                _WaitHandles[1].Set();
            });

            WaitHandle.WaitAll(_WaitHandles);   // Wait for both Threads to be completed.

            //Debug.Log("Inner & Outer Mask Processing Completed.");
            //string logfile = string.Empty;

            // The final Ouput passed back to the Font Editor Window
            Output = new Color[width / ScaleDownFactor * height / ScaleDownFactor];

            // Store Resulting process of Inner and Outer Mask in Output and apply downscaling factor    
            float scale = 1f / Spread;
            int s1 = ScaleDownFactor / 2;
            int ScaledDownWidth = width / ScaleDownFactor;

            //timeStamp = timer.Elapsed;
            for (int y = s1; y < height; y += ScaleDownFactor)
            {
                for (int x = s1; x < width; x += ScaleDownFactor)
                {
                    int x1 = (x - s1) / ScaleDownFactor;
                    int y1 = (y - s1) / ScaleDownFactor;

                    float inner0 = Mathf.Clamp01(innerMask[x + y * width].distance * scale);
                    float outer0 = Mathf.Clamp01(outerMask[x + y * width].distance * scale);


                    //if (ScaleDownFactor > 1)
                    //{
                    //    float inner1 = Mathf.Clamp01(innerMask[(x - 1) + y * width].distance * scale);
                    //    float inner2 = Mathf.Clamp01(innerMask[x + (y - 1) * width].distance * scale);
                    //    float inner3 = Mathf.Clamp01(innerMask[(x - 1) + (y - 1) * width].distance * scale);

                    //    float outer1 = Mathf.Clamp01(outerMask[(x - 1) + y * width].distance * scale);
                    //    float outer2 = Mathf.Clamp01(outerMask[x + (y - 1) * width].distance * scale);
                    //    float outer3 = Mathf.Clamp01(outerMask[(x - 1) + (y - 1) * width].distance * scale);

                    //    inner0 = (inner0 + inner1 + inner2 + inner3) / 4;
                    //    outer0 = (outer0 + outer1 + outer2 + outer3) / 4;
                    //}


                    //if (ScaleDownFactor > 1)
                    //{                  
                    //    int dx0 = x; int dy0 = y;

                    //    // Find smallest Inner distance
                    //    float inner_dst = innerMask[x + y * width].distance;

                    //    // Check dst against (x - 1, y)
                    //    if (inner_dst > innerMask[(x - 1) + y * width].distance)
                    //    {
                    //        dx0 = x - 1; dy0 = y;
                    //        inner_dst = innerMask[(x - 1) + y * width].distance;
                    //    }

                    //    // Check dst against (x - 1, y - 1)
                    //    if (inner_dst > innerMask[(x - 1) + (y - 1) * width].distance)
                    //    {
                    //        dx0 = x - 1; dy0 = y - 1;
                    //        inner_dst = innerMask[(x - 1) + (y - 1) * width].distance;
                    //    }

                    //    // Check dst against (x, y - 1)
                    //    if (inner_dst > innerMask[x + (y - 1) * width].distance)
                    //    {
                    //        dx0 = x; dy0 = y - 1;
                    //        inner_dst = innerMask[x + (y - 1) * width].distance;
                    //    }

                    //    int dX = innerMask[dx0 + dy0 * width].dX;
                    //    int dY = innerMask[dx0 + dy0 * width].dY;
                    //    float delta = ApproximateEdgeDelta(dX, dY, innerMask[(x + dX) + (y + dY) * width].alpha);

                    //    inner0 = Mathf.Sqrt((dX + 0.5f * Mathf.Sign(dX)) * (dX + 0.5f * Mathf.Sign(dX)) + (dY + 0.5f * Mathf.Sign(dY)) * (dY + 0.5f * Mathf.Sign(dY))) + delta;
                    //    inner0 = Mathf.Clamp01(inner0 * scale);

                    //    //float outer_dst = 0;
                    //    dx0 = x; dy0 = y;

                    //    // Find smallest Inner distance
                    //    float outer_dst = outerMask[x + y * width].distance;

                    //    // Check dst against (x - 1, y)
                    //    if (outer_dst > outerMask[(x - 1) + y * width].distance)
                    //    {
                    //        dx0 = x - 1; dy0 = y;
                    //        outer_dst = outerMask[(x - 1) + y * width].distance;
                    //    }

                    //    // Check dst against (x - 1, y - 1)
                    //    if (outer_dst > outerMask[(x - 1) + (y - 1) * width].distance)
                    //    {
                    //        dx0 = x - 1; dy0 = y - 1;
                    //        outer_dst = outerMask[(x - 1) + (y - 1) * width].distance;
                    //    }

                    //    // Check dst against (x, y - 1)
                    //    if (outer_dst > outerMask[x + (y - 1) * width].distance)
                    //    {
                    //        dx0 = x; dy0 = y - 1;
                    //        outer_dst = outerMask[x + (y - 1) * width].distance;
                    //    }

                    //    dX = outerMask[dx0 + dy0 * width].dX;
                    //    dY = outerMask[dx0 + dy0 * width].dY;     
                    //    delta = ApproximateEdgeDelta(dX, dY, outerMask[(x + dX) + (y + dY) * width].alpha);

                    //    outer0 = Mathf.Sqrt((dX + 0.5f * Mathf.Sign(dX)) * (dX + 0.5f * Mathf.Sign(dX)) + (dY + 0.5f * Mathf.Sign(dY)) * (dY + 0.5f * Mathf.Sign(dY))) + delta;  
                    //    outer0 = Mathf.Clamp01(outer0 * scale);
                    //    //Debug.Log("Min Outer Distance for (" + x + "," + y + ") at (" + dx0 + "," + dy0 + ") is " + outer0 + " with dX/dY (" + outerMask[dx0 + dy0 * width].dX + "," + outerMask[dx0 + dy0 * width].dY + ")");
                    //}



                    if (ScaleDownFactor > 1)
                    {
                        inner0 = innerMask[x + y * width].distance;
                        float inner1 = innerMask[(x - 1) + y * width].distance;
                        float inner2 = innerMask[x + (y - 1) * width].distance;
                        float inner3 = innerMask[(x - 1) + (y - 1) * width].distance;

                        outer0 = outerMask[x + y * width].distance;
                        float outer1 = outerMask[(x - 1) + y * width].distance;
                        float outer2 = outerMask[x + (y - 1) * width].distance;
                        float outer3 = outerMask[(x - 1) + (y - 1) * width].distance;

                        inner0 = Mathf.Clamp01((inner0 + inner1 + inner2 + inner3) / 4 * scale);
                        outer0 = Mathf.Clamp01((outer0 + outer1 + outer2 + outer3) / 4 * scale);
                    }

                    ///Debug.Log(inner + " " + outer);

                    float alpha = 0.5f + (inner0 - outer0) * 0.5f;


                    //Debug.Log("(" + x + "," + y + ") Dx/Dy (" + outerMask[x + y * width].dX + "," + outerMask[x + y * width].dY + ") Scale Distance: " + d);

                    Output[x1 + y1 * ScaledDownWidth] = new Color(0, 0, 0, alpha);
                    //logfile += "(" + x + "," + y + ")\t\t\tDistance: " + Mathf.Max(innerMask[x + y * width].distance, outerMask[x + y * width].distance).ToString("f6") + "\t DxDy (" + outerMask[x + y * width].dX + "," + outerMask[x + y * width].dY + ")\t\t Inner: " + inner0.ToString("f4") + "\t\t Outer: " + outer0.ToString("f4") + "\t\t Alpha: " + alpha.ToString("f4") + "\n";
                }

                //logfile += "\n";
            }
            //Debug.Log("Output processed in [" + (timer.Elapsed - timeStamp) + "]");

            // Free up allocations for pixel arrays    	
            innerMask = null;
            outerMask = null;


            timer.Stop();
            Debug.Log("Distance Transform process took: [" + timer.Elapsed + "].");

            TMPro_EventManager.ON_COMPUTE_DT_EVENT(null, new Compute_DT_EventArgs(Compute_DistanceTransform_EventTypes.Completed, Output));// Generate Event to notify and share results with Font Editor Window.

            //var pngData = DistanceMap.EncodeToPNG();	
            //File.WriteAllText("Assets/Logs/Distance Calcs.txt", logfile);
            //File.WriteAllBytes("Assets/Textures/New Texture.png", pngData);	
        }


        // Compute Edge Delta can be imbeded into 
        private static void ComputeEdgeGradient(Pixel p, Color[] colors, int mask, int index, int width)
        {
            // NOTE: Still need to add check against Pixel being along the edges which would fail

            float sqrt2 = 1.41421356f; // Mathf.Sqrt(2f);       

            if (index < width + 1 || index > colors.Length - width - 2)
                return;

            // estimate gradient of edge pixel using surrounding pixels
            if (mask == 1)
            {
                float g = -(1f - colors[index - 1 - width].a)
                            - (1f - colors[index - 1 + width].a)
                            + (1f - colors[index + 1 - width].a)
                            + (1f - colors[index + 1 + width].a);

                p.gradient.x = g + (1f - colors[index + 1].a) - (1f - colors[index - 1].a) * sqrt2;
                p.gradient.y = g + (1f - colors[index + width].a) - (1f - colors[index - width].a) * sqrt2;
                p.gradient.Normalize();
            }
            else
            {
                float g = -colors[index - 1 - width].a
                            - colors[index - 1 + width].a
                            + colors[index + 1 - width].a
                            + colors[index + 1 + width].a;

                p.gradient.x = g + (colors[index + 1].a) - (colors[index - 1].a) * sqrt2;
                p.gradient.y = g + (colors[index + width].a) - (colors[index - width].a) * sqrt2;
                p.gradient.Normalize();
            }

        }

        private static float ApproximateEdgeDelta(float gx, float gy, float a)
        {
            // (gx, gy) can be either the local pixel gradient or the direction to the pixel
            //return 0.5f - a;

            if (gx == 0f || gy == 0f)
            {
                // linear function is correct if both gx and gy are zero
                // and still fair if only one of them is zero
                return 0.5f - a;
            }


            // normalize (gx, gy)
            float length = Mathf.Sqrt(gx * gx + gy * gy);
            gx = gx / length;
            gy = gy / length;

            // reduce symmetrical equation to first octant only
            // gx >= 0, gy >= 0, gx >= gy
            gx = Mathf.Abs(gx);
            gy = Mathf.Abs(gy);
            if (gx < gy)
            {
                float temp = gx;
                gx = gy;
                gy = temp;
            }

            // compute delta
            float a1 = 0.5f * gy / gx;
            if (a < a1)
            {
                // 0 <= a < a1
                return 0.5f * (gx + gy) - Mathf.Sqrt(2f * gx * gy * a);
            }
            if (a < (1f - a1))
            {
                // a1 <= a <= 1 - a1
                return (0.5f - a) * gx;
            }
            // 1-a1 < a <= 1
            return -0.5f * (gx + gy) + Mathf.Sqrt(2f * gx * gy * (1f - a));
        }

        private static Pixel[] Calculate3X3EDT(Color[] input, int mask, int width, int height, int spread)
        {

            int inf = width * height;
            Pixel[] maskOutput = new Pixel[inf];

            Pixel p1; // Test Pixel being considered as potential closest. 

            int pRx = 0, pRy = 0;

            // Forward Scan & Initialization
            for (int y = 0; y < height; y++)
            {
                if (mask == 0f)
                {
                    float pct = (float)y / (height - 1);
                    TMPro_EventManager.ON_COMPUTE_DT_EVENT(null, new Compute_DT_EventArgs(Compute_DistanceTransform_EventTypes.Processing, pct));
                    //Debug.Log("Phase I pct [" + pct + "]");
                }

                for (int x = 0; x < width; x++)
                {
                    int index = x + y * width;
                    Pixel p = new Pixel();
                    maskOutput[index] = p;

                    //Check which Mask is being processed. Propagation makes it possible to generate the mask in-line with Algorithm
                    p.alpha = mask == 0 ? input[index].a : 1f - input[index].a;

                    float pF;

                    if (p.alpha <= 0)
                        pF = inf;
                    else if (p.alpha < 1)
                    {
                        // Distance set to EdgeGradient for AA Pixels. No further processing needed for them.                   
                        ComputeEdgeGradient(p, input, mask, index, width);
                        pF = ApproximateEdgeDelta(p.gradient.x, p.gradient.y, p.alpha);
                        p.dX = 0;
                        p.dY = 0;
                        p.distance = pF;
                        continue;
                    }
                    else
                    {
                        // Distance set to Zero for Alpha = 1; No further processing needed for them.
                        pF = 0;
                        p.dX = 0; p.dY = 0;
                        p.distance = 0;
                        continue;
                    }

                    // Scan Lower Neighbour   
                    if (y > 0)
                    {
                        p1 = maskOutput[index - width];

                        if (p1.distance != inf)
                        {
                            int dX = p1.dX;
                            int dY = p1.dY - 1;

                            float delta = ApproximateEdgeDelta(dX, dY, maskOutput[index + dX + dY * width].alpha);
                            float qF = Mathf.Sqrt(dX * dX + dY * dY) + delta;

                            if (qF < pF)
                            {
                                pF = qF;
                                pRx = dX;
                                pRy = dY;
                            }
                        }
                    }
                    // Scan Left Neighbour
                    if (x > 0)
                    {
                        p1 = maskOutput[index - 1];

                        if (p1.distance != inf)
                        {
                            int dX = p1.dX - 1;
                            int dY = p1.dY;

                            float delta = ApproximateEdgeDelta(dX, dY, maskOutput[index + dX + dY * width].alpha);
                            float qF = Mathf.Sqrt(dX * dX + dY * dY) + delta;

                            if (qF < pF)
                            {
                                pF = qF;
                                pRx = dX;
                                pRy = dY;
                            }
                        }
                    }
                    // Scan Lower-Left Neighbour
                    if (x > 0 && y > 0)
                    {
                        p1 = maskOutput[index - 1 - width];

                        if (p1.distance != inf)
                        {
                            int dX = p1.dX - 1;
                            int dY = p1.dY - 1;

                            float delta = ApproximateEdgeDelta(dX, dY, maskOutput[index + dX + dY * width].alpha);
                            float qF = Mathf.Sqrt(dX * dX + dY * dY) + delta;

                            if (qF < pF)
                            {
                                pF = qF;
                                pRx = dX;
                                pRy = dY;
                            }
                        }
                    }
                    // Scan Lower-Right Neighbour
                    if (x < width - 1 && y > 0)
                    {
                        p1 = maskOutput[index + 1 - width];

                        if (p1.distance != inf)
                        {
                            int dX = p1.dX + 1;
                            int dY = p1.dY - 1;

                            float delta = ApproximateEdgeDelta(dX, dY, maskOutput[index + dX + dY * width].alpha);
                            float qF = Mathf.Sqrt(dX * dX + dY * dY) + delta;

                            if (qF < pF)
                            {
                                pF = qF;
                                pRx = dX;
                                pRy = dY;
                            }
                        }
                    }

                    p.distance = pF;
                    p.dX = pRx;
                    p.dY = pRy;

                }
            }

            // Backward Scan & Initialization
            for (int y = height - 1; y >= 0; y--)
            {
                if (mask == 0f)
                {
                    float pct = ((height - 1) - (float)y) / (height - 1);
                    TMPro_EventManager.ON_COMPUTE_DT_EVENT(null, new Compute_DT_EventArgs(Compute_DistanceTransform_EventTypes.Processing, pct));
                    //Debug.Log("Phase II pct [" + pct + "]");
                }

                for (int x = width - 1; x >= 0; x--)
                {
                    int index = x + y * width;
                    Pixel p = maskOutput[index];

                    if (p.alpha > 0 && p.alpha < 1 || p.distance == 0)
                    {
                        continue;
                    }

                    float pF = p.distance;

                    pRx = p.dX;
                    pRy = p.dY;

                    // Scan Upper Neighbour	
                    if (y < height - 1)
                    {
                        p1 = maskOutput[index + width];

                        if (p1.distance != inf)
                        {
                            int dX = p1.dX;
                            int dY = p1.dY + 1;

                            float delta = ApproximateEdgeDelta(dX, dY, maskOutput[index + dX + dY * width].alpha);
                            float qF = Mathf.Sqrt(dX * dX + dY * dY) + delta;

                            if (qF <= pF)
                            {
                                pF = qF;
                                pRx = dX;
                                pRy = dY;
                            }
                        }
                    }
                    // Scan Right Neighbour
                    if (x < width - 1)
                    {
                        p1 = maskOutput[index + 1];

                        if (p1.distance != inf)
                        {
                            int dX = p1.dX + 1;
                            int dY = p1.dY;

                            float delta = ApproximateEdgeDelta(dX, dY, maskOutput[index + dX + dY * width].alpha);
                            float qF = Mathf.Sqrt(dX * dX + dY * dY) + delta;

                            if (qF < pF)
                            {
                                pF = qF;
                                pRx = dX;
                                pRy = dY;
                            }
                        }
                    }
                    // Scan Upper-Right Neighbour
                    if (x < width - 1 && y < height - 1)
                    {
                        p1 = maskOutput[index + 1 + width];

                        if (p1.distance != inf)
                        {
                            int dX = p1.dX + 1;
                            int dY = p1.dY + 1;

                            float delta = ApproximateEdgeDelta(dX, dY, maskOutput[index + dX + dY * width].alpha);
                            float qF = Mathf.Sqrt(dX * dX + dY * dY) + delta;

                            if (qF < pF)
                            {
                                pF = qF;
                                pRx = dX;
                                pRy = dY;
                            }
                        }
                    }
                    // Scan Upper-Left Neighboud
                    if (x > 0 && y < height - 1)
                    {
                        p1 = maskOutput[index - 1 + width];

                        if (p1.distance != inf)
                        {
                            int dX = p1.dX - 1;
                            int dY = p1.dY + 1;

                            float delta = ApproximateEdgeDelta(dX, dY, maskOutput[index + dX + dY * width].alpha);
                            float qF = Mathf.Sqrt(dX * dX + dY * dY) + delta;

                            if (qF < pF)
                            {
                                pF = qF;
                                pRx = dX;
                                pRy = dY;
                            }
                        }
                    }

                    p.distance = pF;
                    p.dX = pRx;
                    p.dY = pRy;

                }
            }

            return maskOutput;
        }
    }
}