using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Latios
{
    [AddComponentMenu("Gameplay/Utility/Game Manager")]
    public class GameManager : MonoBehaviour
    {
        // This is a classic Unity singleton. It uses a static.
        // Statics break domain reload beginning with 2019.3.
        // Statics are not required for singletons in DOTS.
        // Since this is a non-DOTS 2019.2 project, statics are used here.
        // The 2019.3 API to work around statics not resetting is here:
        // https://docs.unity3d.com/2019.3/Documentation/ScriptReference/InitializeOnEnterPlayModeAttribute.html

        public static GameManager gameManager { get; private set; }

        // The world moves around the camera to avoid the need for origin shifting.
        // This means that anything related to scrolling is going to be double precision.
        // This actually has very little cost performance-wise when not using simd.
        public double                        scrollSpeed;
        [SerializeField] private TMP_Text    m_healthUI;
        [SerializeField] private TMP_Text    m_scoreUI;
        [SerializeField] private string      m_gameOverSceneName;
        [SerializeField] private ScoreObject m_scoreObjectToBeWrittenTo;

        public int health { get => m_health; set { m_health = value; m_dirtyUI = true; } }
        public int score { get => m_score; set { m_score                       = value; m_dirtyUI = true; } }
        private int  m_health;
        private int  m_score;
        private bool m_dirtyUI;

        private void Awake()
        {
            if (gameManager == null)
                gameManager = this;
            Assert.AreEqual(gameManager, this, "Error: Multiple Game Managers are in the scene.");
        }

        private StringBuilder m_healthBuilder = new StringBuilder();
        private StringBuilder m_scoreBuilder  = new StringBuilder();
        const string          kHealthHeader   = "Health: ";
        const string          kScoreHeader    = "Score: ";
        private void Update()
        {
            if (m_health <= 0)
            {
                m_scoreObjectToBeWrittenTo.score = m_score;
                SceneManager.LoadScene(m_gameOverSceneName);
            }

            if (m_dirtyUI)
            {
                m_healthBuilder.Clear();
                m_healthBuilder.Append(kHealthHeader);
                m_healthBuilder.Append(health);

                m_scoreBuilder.Clear();
                m_scoreBuilder.Append(kScoreHeader);
                m_scoreBuilder.Append(score);

                m_healthUI.SetText(m_healthBuilder);
                m_scoreUI.SetText(m_scoreBuilder);
            }
        }
    }
}

