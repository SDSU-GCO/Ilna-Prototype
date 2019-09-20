using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Latios
{
    // This path generator is a stairstep path generator.
    // It randomly generates a step up or a step down depending on the random value.

    // Todo: Make the randomness deterministic using math.random?
    [AddComponentMenu("Level/Procedural/Path Generator")]
    public class PathGenerator : MonoBehaviour
    {
        public float timeInterval = 0.02f;
        public float stepSize     = 0.01f;
        public float clampMin     = -4.5f;
        public float clampMax     = 4.5f;

        private float m_stepTime = 0f;

        private void FixedUpdate()
        {
            while (m_stepTime >= timeInterval)
            {
                m_stepTime -= timeInterval;

                float3 step         = new float3(0f, stepSize, 0f);
                step                = UnityEngine.Random.value <= 0.5f ? step : -step;
                float3 position     = transform.position;
                position           += step;
                position.y          = math.clamp(position.y, clampMin, clampMax);
                transform.position  = position;
            }

            m_stepTime += Time.deltaTime;
        }
    }
}

