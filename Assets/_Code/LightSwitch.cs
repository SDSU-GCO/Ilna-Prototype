using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

namespace Latios
{
    [AddComponentMenu("Gameplay/Interactables/Light Switch")]
    public class LightSwitch : MonoBehaviour, IHitableByLaser
    {
#pragma warning disable IDE0044 // Add readonly modifier
        [SerializeField] private int m_points = 1;
#pragma warning restore IDE0044 // Add readonly modifier
        [SerializeField] private Light2D m_offLight;
        [SerializeField] private Light2D m_onLight;

        private Animator m_anim;
        private GameManager m_manager;

        private void Awake()
        {
            m_anim = GetComponent<Animator>();
            m_manager = GameManager.gameManager;

            // Initialize the light state.
            bool on = m_anim.GetBool("On");
            m_offLight.enabled = !on;
            m_onLight.enabled = on;
        }

        private void OnDisable()
        {
            // Reset
            m_anim.SetBool("On", false);
            m_offLight.enabled = true;
            m_onLight.enabled = false;
        }

        public void ProcessHitByLaser()
        {
            // Only turn on the light if it is off.
            if (m_anim.GetBool("On") == false)
            {
                m_anim.SetBool("On", true);
                m_offLight.enabled = false;
                m_onLight.enabled = true;
                m_manager.score += m_points;
            }
        }
    }
}