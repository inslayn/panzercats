using UnityEngine;
using System.Collections;

public class LobbyScreen : U9SlideView {
    void BackToMenu() {
        GameManager.Instance.HideCurrentScreen();
    }

    void JoinTheFight() {
        GameManager.Instance.Show(GameManager.Screens.InGame, true).Begin();
    }
}
