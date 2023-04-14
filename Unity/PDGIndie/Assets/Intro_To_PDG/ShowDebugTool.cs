using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CNC
{
    public class ShowDebugTool : MonoBehaviour
    {
        int m_catchFrameTimingsCount = 10;
        int m_curcatchFrameTimingsCount = 0;
        public List<FrameTiming> catchFrameTimings = new List<FrameTiming>();
        public FrameTiming[] frameTimings = new FrameTiming[1];
        public float maxResolutionWidthScale = 1.0f;
        public float maxResolutionHeightScale = 1.0f;
        public float minResolutionWidthScale = 0.5f;
        public float minResolutionHeightScale = 0.5f;
        public float scaleWidthIncrement = 0.1f;
        public float scaleHeightIncrement = 0.1f;
        float m_widthScale = 1.0f;
        float m_heightScale = 1.0f;
        double m_gpuFrameTime;
        double m_cpuFrameTime;

        float deltaTime = 0.0f;
        GUIStyle style = null;
        Rect rect;
        private void Awake()
        {
            deltaTime = 0;
            rect = new Rect(Screen.width / 2, 0, Screen.width, Screen.height * 2 / 100);
            style = new GUIStyle();
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = 25;
            style.normal.textColor = Color.white;
        }

        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;


            FrameTimingManager.CaptureFrameTimings();
            FrameTimingManager.GetLatestTimings((uint) frameTimings.Length, frameTimings);

            //平均一下，防止耗时跳动
            if (m_curcatchFrameTimingsCount < m_catchFrameTimingsCount)
            {
                m_curcatchFrameTimingsCount++;
                catchFrameTimings.Add(frameTimings[0]);
            }
            else
            {
                double temp_gpuFrameTime = 0;
                double temp_cpuFrameTime = 0;

                for (int i = 0; i < catchFrameTimings.Count; i++)
                {
                    temp_gpuFrameTime += (double) catchFrameTimings[i].gpuFrameTime;
                    temp_cpuFrameTime += (double) catchFrameTimings[i].cpuFrameTime;
                }

                m_gpuFrameTime = temp_gpuFrameTime / (double) catchFrameTimings.Count;
                m_cpuFrameTime = temp_cpuFrameTime / (double) catchFrameTimings.Count;

                m_curcatchFrameTimingsCount = 0;
                catchFrameTimings.Clear();
            }
        }

        void OnGUI()
        {

            int rezWidth = (int) Mathf.Ceil(ScalableBufferManager.widthScaleFactor * Screen.currentResolution.width);
            int rezHeight = (int) Mathf.Ceil(ScalableBufferManager.heightScaleFactor * Screen.currentResolution.height);
            var reportMsg = string.Format("Scale: {0:F3}x{1:F3}\nResolution: {2}x{3}\nScaleFactor: {4:F3}x{5:F3}\nGPU: {6:F3} CPU: {7:F3}\nULodBias:{8}\nChunk:{9}",
            m_widthScale, m_heightScale, rezWidth, rezHeight, ScalableBufferManager.widthScaleFactor, ScalableBufferManager.heightScaleFactor,
            m_gpuFrameTime, 
            m_cpuFrameTime,
            0,
            0);

            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string fpsText = string.Format("FrameTime:{0:0.0} ms ,FPS:{1:0.},Ping:{2:0} ms", msec, fps,0);
            GUI.Label(rect, fpsText + "\n" + reportMsg, style);
        }
    }
}
