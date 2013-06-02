using UnityEngine;
using System.Collections;

public class TitleScreen : U9SlideView {
    void Update() {
        if(Input.GetKeyUp(KeyCode.Return)) {
            GameManager.Instance.Show(GameManager.Screens.Menu).Begin();
        }
    }
}
