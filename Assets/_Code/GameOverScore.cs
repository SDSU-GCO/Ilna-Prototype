using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

namespace Latios
{
    public class GameOverScore : MonoBehaviour
    {
        [SerializeField] private ScoreObject m_scoreResult;
        private StringBuilder                m_builder = new StringBuilder(200);

        private void Start()
        {
            var text = GetComponent<TMP_Text>();
            m_builder.Append("Game Over\n\nYour score:\t");
            m_builder.Append(m_scoreResult.score);
            text.SetText(m_builder);
        }
    }
}

