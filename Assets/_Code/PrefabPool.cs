using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Latios
{
    // This is a fancy pool that lets objects be fetched from the pool by prefab.
    // This pool is non-lazy to avoid sudden instantiation spikes.
    public class PrefabPool : ScriptableObject
    {
        private Dictionary<GameObject, List<GameObject> > m_poolMap = new Dictionary<GameObject, List<GameObject> >();

        public void Create(GameObject prefab, int poolCount)
        {
            if (m_poolMap.TryGetValue(prefab, out List<GameObject> pool))
            {
                for (int i = pool.Count; i < poolCount; i++)
                {
                    GameObject newObject = Instantiate(prefab);
                    newObject.SetActive(false);
                    pool.Add(newObject);
                }
            }
            else
            {
                List<GameObject> newPool = new List<GameObject>(poolCount);
                for (int i = 0; i < poolCount; i++)
                {
                    GameObject newObject = Instantiate(prefab);
                    newObject.SetActive(false);
                    newPool.Add(newObject);
                }
                m_poolMap.Add(prefab, newPool);
            }
        }

        public GameObject GetAndActivate(GameObject prefab)
        {
            if (m_poolMap.TryGetValue(prefab, out List<GameObject> pool))
            {
                foreach (var go in pool)
                {
                    if (!go.activeSelf)
                    {
                        go.SetActive(true);
                        return go;
                    }
                }
            }
            throw new InvalidOperationException("Error: Prefab does not exist in prefabPool.");
        }

        public void ReleaseAndDeactivate(GameObject prefab, GameObject liveObject)
        {
            if (m_poolMap.TryGetValue(prefab, out List<GameObject> pool))
            {
                foreach (var go in pool)
                {
                    if (go == liveObject)
                    {
                        go.SetActive(false);
                        return;
                    }
                }
            }
            throw new InvalidOperationException("Error: Prefab does not exist in prefabPool.");
        }
    }
}

