using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Latios
{
    [AddComponentMenu("Level/Scripted/Tile Scroller")]
    public class TileScrollLayer : MonoBehaviour
    {
        [Serializable]
        public struct ScrollTileIngredient
        {
            public GameObject prefab;
            public int repeatCount;

            [HideInInspector] public float leftOuterMargin;
            [HideInInspector] public float leftInnerMargin;
            [HideInInspector] public float rightInnerMargin;
            [HideInInspector] public float rightOuterMargin;
        }

        // Doubles are used extensively because the world moves around the camera and things can get far away from the origin.
#pragma warning disable IDE0044 // Add readonly modifier
        [SerializeField] private double m_scrollStart = 0f;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning disable IDE0044 // Add readonly modifier
        [SerializeField] private double m_scrollMultiplier = 1f;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning disable IDE0044 // Add readonly modifier
        [SerializeField] private bool m_loop;
#pragma warning restore IDE0044 // Add readonly modifier
        [SerializeField] private List<ScrollTileIngredient> m_tileIngredients;

        private struct ScrollTile
        {
            public double start;
            public double end;
            public double anchor;
            public int ingredientIndex;
            public GameObject activeGO;
        }

        private ScrollTile[] m_tiles;

        private double m_lastTileRightInnerMargin;  // The "loop point"
        private double m_scroll;
        private double m_cameraScrollLeftPoint;
        private double m_screenWidthInUnits;
        private GameManager m_manager;
        private PrefabPool m_prefabPool;

        private void Awake()
        {
            m_manager = GameManager.gameManager;
            m_scroll = 0d;
            m_prefabPool = ScriptableObject.CreateInstance<PrefabPool>();

            Camera camera = Camera.main;
            m_screenWidthInUnits = 2 * camera.orthographicSize * camera.aspect;
            m_cameraScrollLeftPoint = camera.transform.position.x - m_screenWidthInUnits / 2;
            int count = m_tileIngredients.Count;
            float2[] bounds = new float2[count];
            int sum = 0;

            // Calculate total tiles and also fetch margins if they exist (otherwise they default to 0s).
            // Also warm up the prefabPool.
            for (int i = 0; i < count; i++)
            {
                ScrollTileIngredient tile = m_tileIngredients[i];
                sum += tile.repeatCount;
                bounds[i] = GetBoundsOfTilePrefab(m_tileIngredients[i].prefab);

                TileMargins margins = tile.prefab.GetComponent<TileMargins>();
                if (margins != null)
                {
                    tile.leftOuterMargin = margins.leftOuterMargin;
                    tile.leftInnerMargin = margins.leftInnerMargin;
                    tile.rightInnerMargin = margins.rightInnerMargin;
                    tile.rightOuterMargin = margins.rightOuterMargin;
                    m_tileIngredients[i] = tile;
                }
                int poolSize = (int)math.ceil(m_screenWidthInUnits / (bounds[i].y - bounds[i].x - tile.leftInnerMargin - tile.rightInnerMargin)) + 1;
                m_prefabPool.Create(tile.prefab, poolSize);
            }

            m_tiles = new ScrollTile[sum];
            sum = 0;

            // Running non-margined start point
            double runningMin = m_scrollStart;

            // Build the tiles from the tile ingredients and the bounds
            for (int i = 0; i < count; i++)
            {
                ScrollTileIngredient tileIngredient = m_tileIngredients[i];
                for (int j = 0; j < tileIngredient.repeatCount; j++)
                {
                    int tileId = sum + j;

                    ScrollTile tile = new ScrollTile
                    {
                        ingredientIndex = i,

                        // Subtract left margins from non-margined start point
                        start = runningMin - tileIngredient.leftInnerMargin - tileIngredient.leftOuterMargin,

                        // Add the distance from the left inner margin to the right outer margin with the non-margined start point
                        end = runningMin - tileIngredient.leftInnerMargin + bounds[i].y - bounds[i].x + tileIngredient.rightOuterMargin,

                        // Add the distance from the left inner margin to the bounds center reference with the non-margined start point
                        // Also factor in the prefab's position because people forget to 0 that out.
                        anchor = runningMin - tileIngredient.leftInnerMargin - bounds[i].x + tileIngredient.prefab.transform.position.x,
                        activeGO = null
                    };
                    m_tiles[tileId] = tile;

                    // Add the distance between the inner margins to the running non-margined start point
                    runningMin += bounds[i].y - bounds[i].x - tileIngredient.rightInnerMargin - tileIngredient.leftInnerMargin;
                }
            }

            m_lastTileRightInnerMargin = runningMin;
        }

        // Iterate through both renderers and colliders to get the bounds.
        private List<Renderer> m_rendererCache = new List<Renderer>();
        private List<Collider2D> m_colliderCache = new List<Collider2D>();

        private float2 GetBoundsOfTilePrefab(GameObject prefab)
        {
            float2 minmax = new float2(float.MaxValue, float.MinValue);
            m_rendererCache.Clear();
            m_colliderCache.Clear();

            prefab.GetComponentsInChildren(m_rendererCache);
            prefab.GetComponentsInChildren(m_colliderCache);

            foreach (Renderer renderer in m_rendererCache)
            {
                float2 bounds = new float2(renderer.bounds.min.x, renderer.bounds.max.x);
                minmax.x = math.min(minmax.x, bounds.x);
                minmax.y = math.max(minmax.y, bounds.y);
            }

            foreach (Collider2D collider in m_colliderCache)
            {
                float2 bounds = new float2(collider.bounds.min.x, collider.bounds.max.x);
                minmax.x = math.min(minmax.x, bounds.x);
                minmax.y = math.max(minmax.y, bounds.y);
            }

            return minmax;
        }

        private void Update()
        {
            // Calculate new scroll point
            double dt = Time.deltaTime;
            m_scroll += dt * m_scrollMultiplier * m_manager.scrollSpeed;
            if (m_loop && m_scroll >= m_lastTileRightInnerMargin)
            {
                m_scroll = m_scroll % m_lastTileRightInnerMargin;
            }

            // Because a tile can be thinner than it's neighboring tiles' margins, we need a brute force algorithm.
            // If this becomes expensive, we might need to draw upon Burst, even though I have been trying to avoid it for this project.

            double2 scrollBounds = new double2(m_scroll + m_cameraScrollLeftPoint, m_scroll + m_cameraScrollLeftPoint + m_screenWidthInUnits);
            double2 positiveScrollBounds = scrollBounds + m_lastTileRightInnerMargin;
            double2 negativeScrollBounds = scrollBounds - m_lastTileRightInnerMargin;

            // Remove all objects that have left the camera view.
            for (int i = 0; i < m_tiles.Length; i++)
            {
                if (m_tiles[i].activeGO == null)
                {
                    continue;
                }

                ScrollTile tile = m_tiles[i];
                double2 tileBounds = new double2(tile.start, tile.end);
                bool offScreen = !BoundsIntersect(tileBounds, scrollBounds);
                if (m_loop)
                {
                    offScreen &= !BoundsIntersect(tileBounds, positiveScrollBounds);
                    //offScreen &= !BoundsIntersect(tileBounds, negativeScrollBounds);
                }
                if (offScreen)
                {
                    int index = tile.ingredientIndex;
                    GameObject prefab = m_tileIngredients[index].prefab;
                    m_prefabPool.ReleaseAndDeactivate(prefab, tile.activeGO);
                    tile.activeGO = null;
                    m_tiles[i] = tile;
                }
            }

            // Get pool objects for tiles that came into view
            for (int i = 0; i < m_tiles.Length; i++)
            {
                ScrollTile tile = m_tiles[i];
                double2 tileBounds = new double2(tile.start, tile.end);
                bool onScreen = BoundsIntersect(tileBounds, scrollBounds);
                if (m_loop)
                {
                    //onScreen |= BoundsIntersect(tileBounds, positiveScrollBounds);
                    onScreen |= BoundsIntersect(tileBounds, negativeScrollBounds);
                }
                if (tile.activeGO == null && onScreen)
                {
                    int index = tile.ingredientIndex;
                    GameObject prefab = m_tileIngredients[index].prefab;
                    tile.activeGO = m_prefabPool.GetAndActivate(prefab);
                    m_tiles[i] = tile;
                }
            }

            // Update positions of all active objects
            for (int i = 0; i < m_tiles.Length; i++)
            {
                if (m_tiles[i].activeGO == null)
                {
                    continue;
                }

                Transform tf = m_tiles[i].activeGO.transform;
                Vector3 pos = tf.position;
                double offset = 0d;
                if (m_loop)
                {
                    if (m_tiles[i].end < scrollBounds.x)
                    {
                        offset = m_lastTileRightInnerMargin;
                    }
                    else if (m_tiles[i].start > scrollBounds.y)
                    {
                        offset = -m_lastTileRightInnerMargin;
                    }
                }
                pos.x = (float)(m_tiles[i].anchor - m_scroll + offset);
                tf.position = pos;
            }
        }

        private bool BoundsIntersect(double2 a, double2 b)
        {
            bool invRes = a.y < b.x || a.x > b.y;
            return !invRes;
        }
    }
}