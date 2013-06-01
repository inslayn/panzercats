using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoSingleton<GameManager> {

    public enum Screens {
        Title,
        Lobby,
        InGame,
        Credits,
    }

    U9ViewStack stack;

    [SerializeField]
    List<U9View> views;

    Dictionary<Screens, System.Type> screenToTypeMap;

    void Awake() {
        Instance = this;

        stack = GetComponent<U9ViewStack>();

        screenToTypeMap = new Dictionary<Screens, System.Type>() {
            { Screens.Title, typeof(TitleScreen) },
            { Screens.Lobby, typeof(LobbyScreen) },
            { Screens.InGame, typeof(InGameScreen) },
            { Screens.Credits, typeof(CreditsScreen) },
        };
    }

    public U9Transition Show(Screens screen) {
        U9View view = GetViewFromType(screenToTypeMap[screen]);

        if(view)
            return stack.GetPushViewTransition(view);

        return U9T.Null();
    }

    U9View GetViewFromType(System.Type type) {
        U9View view = null;

        foreach(U9View v in views) {
            if(views.GetType() == type) {
                view = v;
                break;
            }
        }

        if(view)
            return view;

        return null;
    }
}
