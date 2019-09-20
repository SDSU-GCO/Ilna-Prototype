using System.Collections;
using UnityEngine;

namespace Latios
{
    [CreateAssetMenu(fileName = "New Score Object.asset", menuName = "Score Object")]
    public class ScoreObject : ScriptableObject
    {
        public int score;
    }
}

