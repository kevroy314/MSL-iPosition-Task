using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitOnButton : MonoBehaviour {

    public KeyCode quitKey = KeyCode.Escape;

    void Update()
    {
        if (Input.GetKeyDown(quitKey))
        {
            Application.Quit();
        }
    }
}
