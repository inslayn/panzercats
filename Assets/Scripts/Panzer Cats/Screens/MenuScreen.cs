using UnityEngine;
using System.Collections;

public class MenuScreen : U9SlideView {
    void OnCredits() {
        GameManager.Instance.Show(GameManager.Screens.Credits).Begin();
    }

    void OnCreate() {
        GameManager.Instance.Show(GameManager.Screens.InGame, true).Begin();
    }

    void OnJoin() {
        GameManager.Instance.Show(GameManager.Screens.Lobby).Begin();
    }
}
