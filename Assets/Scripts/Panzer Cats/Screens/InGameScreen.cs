using UnityEngine;
using System.Collections;

public class InGameScreen : U9SlideView {
    [SerializeField]
    UILabel ip;

    protected override void EndDisplay() {
        base.EndDisplay();
        ip.text = NetworkingManager.Instance.ServerIP;
    }

    void Update() {
        if(Input.GetKeyUp(KeyCode.Escape)) {
			Application.LoadLevel(0);
        }
    }
}
