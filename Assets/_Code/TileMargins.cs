using System.Collections;
using UnityEngine;

namespace Latios
{
    // The inner margins are where adjacent tiles "touch" each other.
    // The outer margins are for when the initial bounds are too small.
    // For example, a light near the edge of the tile has a range that exceeds the edge.
    // By increasing the outer margin by the range of the light, the light source won't visibly turn off when it leaves the screen.

    [AddComponentMenu("Level/Scripted/Tile Margins")]
    public class TileMargins : MonoBehaviour
    {
        public float leftOuterMargin;
        public float leftInnerMargin;
        public float rightInnerMargin;
        public float rightOuterMargin;
    }
}

