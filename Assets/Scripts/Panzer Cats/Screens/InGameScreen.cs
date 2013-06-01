using UnityEngine;
using System.Collections;

public class InGameScreen : U9View {

    protected override U9Transition CreateDisplayTransition(bool force) {
        return U9T.Null();
    }

    protected override U9Transition CreateHideTransition(bool force) {
        return U9T.Null();
    }
}
