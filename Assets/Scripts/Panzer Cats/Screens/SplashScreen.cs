using UnityEngine;
using System.Collections;

public class SplashScreen : U9FadeView {
    protected override void EndHide() {
        base.EndHide();
        LoadTitles();
    }

    void LoadTitles() {
        GameManager.Instance.Show(GameManager.Screens.Title);
    }
}
