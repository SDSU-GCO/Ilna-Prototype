using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Latios
{
    [AddComponentMenu("Gameplay/Utility/Quit")]
    public class QuitGame : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                Application.Quit();
            }
        }
    }
}

