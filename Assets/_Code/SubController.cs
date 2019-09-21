using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Latios
{
    [AddComponentMenu("Gameplay/Controllers/Sub Controller")]
    public class SubController : MonoBehaviour
    {
        [SerializeField] private float2 m_maxSpeed            = new float2(5f, 5f);
        [SerializeField] private float  m_controlRecoveryTime = 0.2f;  // How long until the submarine regains complete control after a collision
        [SerializeField] private int    m_maxHealth           = 5;  // Initial health of the submarine

        private Rigidbody2D m_rigidbody;
        private GameManager m_gameManager;
        private float       m_control = 1f;  // Factor between 0 and 1 dictating how much control the player has vs physics. 1 means complete player control.
        private float4      m_aabb;  // Local space axis-aligned bounding box of the sub
        private float4      m_playableAreaAabb;  // World space axis-aligned bounding box of what the camera can see

        private void Start()
        {
            m_rigidbody          = GetComponent<Rigidbody2D>();
            m_gameManager        = GameManager.gameManager;
            m_gameManager.health = m_maxHealth;

            // Calculate the world-space AABB of the sub from its colliders
            List<Collider2D> colliders = new List<Collider2D>();
            m_rigidbody.GetAttachedColliders(colliders);
            m_aabb = new float4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
            foreach (var collider in colliders)
            {
                var    bounds = collider.bounds;
                float3 min    = bounds.min;
                float3 max    = bounds.max;
                m_aabb.xy     = math.min(m_aabb.xy, min.xy);
                m_aabb.zw     = math.max(m_aabb.zw, max.xy);
            }

            // Set the origin of the AABB to the position of the sub.
            float3 pos   = transform.position;
            m_aabb.xyzw -= pos.xyxy;

            // Calculate the playable area AABB
            var    cam    = Camera.main;
            float3 campos = cam.transform.position;
            float2 camExtents;
            camExtents.y       = cam.orthographicSize;
            camExtents.x       = camExtents.y * cam.aspect;
            m_playableAreaAabb = new float4(-camExtents, camExtents) + campos.xyxy;
        }

        private void Update()
        {
            float2 move     = new float2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * m_maxSpeed;
            float2 velocity = math.lerp(m_rigidbody.velocity, move, m_control);

            // Clamp the velocity to 0 if bounds are exceeded.
            // Todo: Use slider joint connected to the camera instead?
            float2 position = m_rigidbody.position;
            bool2  clamp    = position + m_aabb.xy < m_playableAreaAabb.xy | position + m_aabb.zw > m_playableAreaAabb.zw;
            position = math.clamp(position, m_playableAreaAabb.xy - m_aabb.xy, m_playableAreaAabb.zw - m_aabb.zw);
            m_rigidbody.position = position;
            velocity        = math.select(velocity, float2.zero, clamp);

            // We let Physics drive the submarine movement.
            // This works well here because we are moving through free space and want our sub to bounce off rocks.
            // In most games, especially platformers, this is a terrible approach.
            m_rigidbody.velocity = velocity;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // We need to be careful here to prevent multiple coroutines running at once.
            bool startCoroutine = m_control >= 1f;
            m_control           = 0f;
            m_gameManager.health--;
            if (startCoroutine)
            {
                StartCoroutine(RecoverControl());
            }
        }

        IEnumerator RecoverControl()
        {
            while (m_control < 1f)
            {
                yield return null;
                m_control += Time.deltaTime / m_controlRecoveryTime;
                m_control  = math.saturate(m_control);  // Saturate clamps between 0 and 1.
            }
        }
    }
}

