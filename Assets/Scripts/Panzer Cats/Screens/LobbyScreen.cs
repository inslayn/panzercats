using UnityEngine;
using System.Collections;

public class LobbyScreen : U9SlideView {
    [SerializeField]
    UIInput input;

    protected override void BeginDisplay() {
        base.BeginDisplay();
        
        if(PlayerPrefs.HasKey("IPAddress")) {
            input.text = PlayerPrefs.GetString("IPAddress");
        }
    }

    void BackToMenu() {
        GameManager.Instance.HideCurrentScreen();
    }

    void JoinTheFight() {
        if(PlayerPrefs.HasKey("IPAddress")) {
            string ip = PlayerPrefs.GetString("IPAddress");
            if(!ip.Equals(input.text)) {
                PlayerPrefs.SetString("IPAddress", input.text);
            }
        }
        else {
            PlayerPrefs.SetString("IPAddress", input.text);
        }

        NetworkingManager.Instance.JoinServer(input.text);
        GameManager.Instance.Show(GameManager.Screens.InGame, true).Begin();
    }
}
