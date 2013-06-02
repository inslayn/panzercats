using UnityEngine;
using System.Collections;

public class TitleScreen : U9SlideView {
	protected override void EndDisplay() {
		Invoke("Menu", 2f);
	}
	
	void Menu() {
       GameManager.Instance.Show(GameManager.Screens.Menu).Begin();
	}
}
