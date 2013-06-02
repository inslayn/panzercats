using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoSingleton<GameManager> {

    public enum Screens {
        Splash,
        Title,
        Menu,
        Lobby,
        InGame,
        Credits,
    }

    U9ViewStack stack;

    [SerializeField]
    List<U9View> views;

    [SerializeField]
    GameObject background;

    Dictionary<Screens, System.Type> screenToTypeMap;

    void Awake() {
        Instance = this;

        stack = GetComponent<U9ViewStack>();

        screenToTypeMap = new Dictionary<Screens, System.Type>() {
            { Screens.Splash, typeof(SplashScreen) },
            { Screens.Title, typeof(TitleScreen) },
            { Screens.Menu, typeof(MenuScreen) },
            { Screens.Lobby, typeof(LobbyScreen) },
            { Screens.InGame, typeof(InGameScreen) },
            { Screens.Credits, typeof(CreditsScreen) },
        };
    }

    void Start() {
        Show(Screens.Splash);
    }

    public U9Transition Show(Screens screen, bool disableBackground = false) {
        U9View view = GetViewFromType(screenToTypeMap[screen]);

        if(disableBackground) {
            if(background.activeSelf)
                background.SetActive(false);
        }
        else {
            if(!background.activeSelf)
                background.SetActive(true);
        }

        if(view)
            return stack.GetPushViewTransition(view);

        return U9T.Null();
    }

    public void HideCurrentScreen() {
        stack.GetPopViewTransition().Begin();
    }

    U9View GetViewFromType(System.Type type) {
        U9View view = null;

        foreach(U9View v in views) {
            if(v.GetType() == type) {
                view = v;
                break;
            }
        }

        if(view)
            return view;

        return null;
    }
}
