using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Latios
{
    [AddComponentMenu("Gameplay/Utility/Next Scene")]
    public class NextScene : MonoBehaviour
    {
        public string sceneName;

        void Update()
        {
            if (Input.GetButtonDown("Submit"))
                SceneManager.LoadScene(sceneName);
        }
    }
}

