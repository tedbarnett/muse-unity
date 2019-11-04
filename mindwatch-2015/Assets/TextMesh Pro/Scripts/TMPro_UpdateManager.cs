using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace TMPro
{
    [ExecuteInEditMode]
    public class TMPro_UpdateManager : MonoBehaviour
    {
        private static List<TextMeshPro> m_objectList;

        void Awake()
        {        
            //Debug.Log("TMPro_UpdateManager has been added.");
        }


        public void ScheduleObjectForUpdate(TextMeshPro obj)
        {
            //Debug.Log("Text Object ID:" + obj.GetInstanceID() + " has been scheduled for update. Object visibility is:" + obj.renderer.isVisible);
            
            if (m_objectList == null)
                m_objectList = new List<TextMeshPro>();
                   
                m_objectList.Add(obj);
        }


        void OnPreCull()
        {
            Debug.Log("OnPreCull() called.");
            // Iterate through list of object to update            
            if (m_objectList != null && m_objectList.Count > 0)
            {
                for (int i = 0; i < m_objectList.Count; i++)
                {
                    //m_objectList[i].UpdateMesh();
                   
                    //Debug.Log("Object's visibility is:" + pair.Value.renderer.isVisible);
                }

                // Clear Dictionary 
                m_objectList.Clear();

#if UNITY_EDITOR
                UnityEditor.SceneView.RepaintAll(); // Refresh Sceneview in the Editor
#endif 
            }         
        }
    }
}
