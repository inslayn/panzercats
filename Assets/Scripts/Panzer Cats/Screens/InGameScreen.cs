using UnityEngine;
using System.Collections;

public class InGameScreen : U9SlideView {
    void Update() {
        if(Input.GetKeyUp(KeyCode.Escape)) {
            GameManager.Instance.HideCurrentScreen();
        }
    }
}
