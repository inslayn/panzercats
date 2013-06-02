using UnityEngine;
using System.Collections;

public class MenuScreen : U9SlideView {

    void OnCredits() {
        GameManager.Instance.Show(GameManager.Screens.Credits).Begin();
    }

    void OnCreate() {
        NetworkingManager.Instance.ServerIP = Network.player.ipAddress;
        NetworkingManager.Instance.CreateServer();
        GameManager.Instance.Show(GameManager.Screens.InGame, true).Begin();
    }

    void OnJoin() {
        GameManager.Instance.Show(GameManager.Screens.Lobby).Begin();
    }
}
