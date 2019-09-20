using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Latios
{
    // Letterboxes the camera to the specified aspect ratio.
    // This makes the game fair on different devices where on some devices areas might not be clickable.

    [AddComponentMenu("Gameplay/Utility/Letter Box")]
    public class LetterBox : MonoBehaviour
    {
        [SerializeField] private int aspectWidth  = 16;
        [SerializeField] private int aspectHeight = 9;

        Camera m_camera;

        private void Awake()
        {
            m_camera = GetComponent<Camera>();
            Assert.IsNotNull(m_camera, $"Error: Letter Box component on {gameObject.name} requires a camera component!");
            Update();
        }

        void Update()
        {
            float2 pixels         = new float2(Screen.width, Screen.height);
            float2 targetAspect   = new float2(aspectWidth, aspectHeight);
            float2 deaspect       = pixels / targetAspect;
            float2 normalizedSize = deaspect.yx / math.cmax(deaspect);
            var    rect           = m_camera.rect;
            rect.width            = normalizedSize.x;
            rect.height           = normalizedSize.y;
            rect.center           = new float2(0.5f, 0.5f);
            m_camera.rect         = rect;
        }
    }
}

