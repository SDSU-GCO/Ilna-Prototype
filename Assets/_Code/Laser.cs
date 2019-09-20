using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Latios
{
    [AddComponentMenu("Gameplay/Controllers/Laser")]
    public class Laser : MonoBehaviour
    {
        [SerializeField] private float m_laserStartTime = 0.1f;
        [SerializeField] private float m_laserHoldTime = 0.3f;
        [SerializeField] private float m_laserStopTime = 0.1f;
        [SerializeField] private LayerMask m_laserHitLayerMask;  // Only objects with this layermask can be hit by the laser.

        private LineRenderer m_lineRenderer;
        private AudioSource m_laserSound;
        private Camera m_camera;
        private GameManager m_manager;
        private bool m_firing;

        // Why can this not be a list? I have no idea. Hopefully 10000 is enough!
        private RaycastHit2D[] m_hitsCache = new RaycastHit2D[10000];

        private void Start()
        {
            // Initialize the line renderer to just be the little glow point at the top of the sub.
            m_lineRenderer = GetComponent<LineRenderer>();
            m_lineRenderer.positionCount = 2;
            m_lineRenderer.SetPosition(0, transform.position);
            m_lineRenderer.SetPosition(1, transform.position);

            m_laserSound = GetComponent<AudioSource>();
            m_camera = Camera.main;
            m_manager = GameManager.gameManager;
        }

        private void Update()
        {
            // Set the first point to the top of the sub every frame.
            // The second point will either be updated each frame in the coroutine or set to the first point.
            m_lineRenderer.SetPosition(0, transform.position);
            if (Input.GetButtonDown("Fire1") && !m_firing)
            {
                StartCoroutine(FireLaser());
            }
            else
            {
                m_lineRenderer.SetPosition(1, transform.position);
            }
        }

        private IEnumerator FireLaser()
        {
            m_firing = true;
            m_laserSound.Play();

            // Get the click position in world space.
            float3 target = m_camera.ScreenToWorldPoint(Input.mousePosition);

            // Interpolate the laser towards the click point
            float t = 0f;
            for (; t < m_laserStartTime; t += Time.deltaTime)
            {
                float3 origin = transform.position;
                float3 position = math.lerp(origin, target, t / m_laserStartTime);
                m_lineRenderer.SetPosition(1, position);
                CheckHits(origin.xy, position.xy);
                yield return null;

                // Because the world moves around the camera in this game, the target point needs to shift by the scroll speed
                target.x -= (float)m_manager.scrollSpeed * Time.deltaTime;
            }

            // Keep the laser on the target.
            for (t -= m_laserStartTime; t < m_laserHoldTime; t += Time.deltaTime)
            {
                float3 origin = transform.position;
                m_lineRenderer.SetPosition(1, target);
                CheckHits(origin.xy, target.xy);
                yield return null;

                target.x -= (float)m_manager.scrollSpeed * Time.deltaTime;
            }

            // Interpolate to retract the laser.
            for (t -= m_laserHoldTime; t < m_laserStopTime; t += Time.deltaTime)
            {
                float3 origin = transform.position;
                float3 position = math.lerp(target, origin, t / m_laserStartTime);
                m_lineRenderer.SetPosition(1, position);
                CheckHits(origin.xy, position.xy);
                yield return null;

                target.x -= (float)m_manager.scrollSpeed * Time.deltaTime;
            }
            m_firing = false;
        }

        private List<IHitableByLaser> m_hitableCache = new List<IHitableByLaser>();
        private void CheckHits(float2 a, float2 b)
        {
            // Create a circle with the diameter set to the line width.
            // Send it across the laser's path and collect all the objects it hits.
            float radius = m_lineRenderer.startWidth / 2f;
            int count = Physics2D.CircleCastNonAlloc(a, radius, b - a, m_hitsCache, math.distance(a, b), m_laserHitLayerMask);
            for (int i = 0; i < count; i++)
            {
                // Let each object decide how to handle being hit by the laser if it cares.
                RaycastHit2D hit = m_hitsCache[i];
                m_hitableCache.Clear();
                hit.collider.GetComponents(m_hitableCache);
                foreach (IHitableByLaser hitable in m_hitableCache)
                {
                    hitable?.ProcessHitByLaser();
                }
            }
        }
    }
}