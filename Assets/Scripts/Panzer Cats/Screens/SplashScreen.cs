using UnityEngine;
using System.Collections;

public class SplashScreen : U9FadeView {
    float delay = 1.5f;

    protected override void Start() {
        base.Start();
        Invoke("LoadTitles", delay);
    }

    void LoadTitles() {
        GameManager.Instance.Show(GameManager.Screens.Title).Begin();
    }
}
