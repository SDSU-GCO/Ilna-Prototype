using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Latios
{
    [AddComponentMenu("Level/Procedural/Pool Spawner")]
    public class ProceduralPoolSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject m_prefab;
        [SerializeField] private Transform m_pathGenerator;
        [SerializeField] private float m_spawnRate = 1f;
        [SerializeField] private float m_minSpawnOffset = -6f;
        [SerializeField] private float m_maxSpawnOffset = -2f;
        [SerializeField] private float m_minSpawnAngle = -30f;
        [SerializeField] private float m_maxSpawnAngle = 30f;
        [SerializeField] private float m_scrollMultiplier = 1f;

        private List<GameObject> m_pool;
        private List<GameObject> m_active;

        private GameManager m_manager;

        private float m_stepTime = 0f;
        private float m_cullPoint;

        private void Awake()
        {
            Assert.IsNotNull(m_prefab, $"Error: Prefab missing on ProceduralPoolSpawner of {gameObject.name}");
            Assert.IsNotNull(m_pathGenerator, $"Error: Path generator missing on ProceduralPoolSpawner of {gameObject.name}");

            // Calculate the pool size based on quadruple the distance from the spawn point to the left of the screen.
            Camera cam = Camera.main;
            m_manager = GameManager.gameManager;

            float left = cam.transform.position.x - cam.orthographicSize * cam.aspect;
            float distance = transform.position.x - left;
            double expectedSpawns = m_spawnRate * distance / (m_scrollMultiplier * m_manager.scrollSpeed);
            int count = 4 * (int)(expectedSpawns + 1);
            m_pool = new List<GameObject>(count);
            m_active = new List<GameObject>(count);

            m_cullPoint = -2f * distance + m_prefab.transform.position.x;

            // Fill pool
            for (int i = 0; i < count; i++)
            {
                GameObject go = Instantiate(m_prefab);
                go.SetActive(false);
                m_pool.Add(go);
            }
        }

        private void FixedUpdate()
        {
            // Spawn objects randomly
            while (Random.value <= m_stepTime)
            {
                m_stepTime = 0f;

                float spawnY = m_pathGenerator.position.y + Random.Range(m_minSpawnOffset, m_maxSpawnOffset);
                float spawnRot = Random.Range(m_minSpawnAngle, m_maxSpawnAngle);

                GameObject go;
                if (m_pool.Count == 0)
                {
                    go = Instantiate(m_prefab);
                }
                else
                {
                    go = m_pool[m_pool.Count - 1];
                    m_pool.RemoveAt(m_pool.Count - 1);
                }
                go.transform.position += new Vector3(transform.position.x, spawnY, 0f);
                go.transform.Rotate(0f, 0f, spawnRot);
                go.SetActive(true);
                m_active.Add(go);
            }
            m_stepTime += Time.deltaTime * m_spawnRate;

            float scrollFactor = (float)m_manager.scrollSpeed * m_scrollMultiplier;

            // Scroll objects
            foreach (GameObject go in m_active)
            {
                go.transform.position -= new Vector3(scrollFactor * Time.deltaTime, 0f, 0f);
            }

            // Cull objects and return to the pool
            for (int i = 0; i < m_active.Count; i++)
            {
                GameObject go = m_active[i];
                if (go.transform.position.x < m_cullPoint)
                {
                    m_active[i] = m_active[m_active.Count - 1];
                    m_active.RemoveAt(m_active.Count - 1);
                    go.transform.position = m_prefab.transform.position;
                    go.transform.rotation = m_prefab.transform.rotation;
                    go.SetActive(false);
                    m_pool.Add(go);
                }
            }
        }
    }
}