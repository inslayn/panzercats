using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoSingleton<GameManager> {

    U9ViewStack stack;

    [SerializeField]
    List<U9View> views;

    void Awake() {
        Instance = this;

        stack = GetComponent<U9ViewStack>();
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
