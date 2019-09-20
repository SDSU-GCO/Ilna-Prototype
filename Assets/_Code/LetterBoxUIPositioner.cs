using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Latios
{
    // This complicated little bugger tries to position the UI component in the letterbox if it fits,
    // otherwise it snaps the UI component into the game view as if there was no letterbox.
    [AddComponentMenu("Gameplay/Utitlity/Letter Box UI Positioner")]
    public class LetterBoxUIPositioner : MonoBehaviour
    {
        [SerializeField] private Camera m_camera;
        [SerializeField] private bool   m_applyYOffsetInLetterbox = false;

        private RectTransform m_rectTransform;
        private float         m_yOffset         = 0f;
        private float         m_height          = 100f;
        private float         m_screenHeight    = 1080f;
        private float         m_canvasRefHeight = 1080f;

        void Start()
        {
            if (m_camera == null)
                m_camera = Camera.main;

            m_rectTransform = GetComponent<RectTransform>();
            Assert.IsNotNull(m_rectTransform, $"Error: Letter Box UI Positioner component on {gameObject.name} is not attached to a UI Game Object!");

            var canvas        = GetComponentInParent<Canvas>();
            m_canvasRefHeight = canvas.GetComponent<RectTransform>().rect.height;

            m_height       = m_rectTransform.rect.height;
            m_yOffset      = -m_rectTransform.anchoredPosition.y;
            m_screenHeight = Screen.height;
        }

        void Update()
        {
            float normalizedMargin         = m_camera.rect.y;
            float addOffset                = m_applyYOffsetInLetterbox ? m_yOffset : 0f;
            float requiredNormalizedMargin = (m_height + addOffset) / m_canvasRefHeight;
            float oldY                     = -m_rectTransform.anchoredPosition.y;
            float newY                     = oldY;
            if (requiredNormalizedMargin > normalizedMargin)
            {
                // Move below letterbox
                newY = normalizedMargin * m_canvasRefHeight + m_yOffset;
            }
            else if (m_applyYOffsetInLetterbox)
            {
                // Move to top of letterbox with offset
                newY = m_yOffset;
            }
            else
            {
                // Move to middle of letterbox without offset
                float marginStart = (normalizedMargin - requiredNormalizedMargin) / 2f;
                newY              = marginStart * m_canvasRefHeight;
            }

            // Don't dirty the canvas unless something changed
            if (newY != oldY)
            {
                var pos                           = m_rectTransform.anchoredPosition;
                pos.y                            += -(newY - oldY);
                m_rectTransform.anchoredPosition  = pos;
            }
        }
    }
}

