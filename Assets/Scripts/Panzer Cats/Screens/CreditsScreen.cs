using UnityEngine;
using System.Collections;

public class CreditsScreen : U9SlideView {
    void BackToMenu() {
        GameManager.Instance.HideCurrentScreen();
    }
}
