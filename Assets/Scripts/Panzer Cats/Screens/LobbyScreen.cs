using UnityEngine;
using System.Collections;

public class LobbyScreen : U9SlideView {
    [SerializeField]
    UIInput input;

    void BackToMenu() {
        GameManager.Instance.HideCurrentScreen();
    }

    void JoinTheFight() {
        NetworkingManager.Instance.JoinServer(input.text);
        GameManager.Instance.Show(GameManager.Screens.InGame, true).Begin();
    }
}
