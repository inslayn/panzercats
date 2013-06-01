using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class U9ViewStack : MonoBehaviour
{
	
	Stack<U9View> viewStack;
	
	void Awake ()
	{
		viewStack = new Stack<U9View> ();	
	}
	
	public U9Transition GetPushViewTransition (U9View newView, bool hideOldView = true, bool force = false)
	{
		
		
		U9View oldView = null;
		
		if (viewStack.Count > 0) {
			oldView = viewStack.Peek ();
		}
		viewStack.Push (newView);
		
		U9Transition hideOldViewTransition = null, displayNewViewTransition = null;
		
		if (oldView && hideOldView) {
			hideOldViewTransition = oldView.GetHideTransition (force);
		}
		displayNewViewTransition = newView.GetDisplayTransition (force);
		
//		Debug.Log ("PUSH");
//		PrintStack();
		
		return U9T.S (hideOldViewTransition, displayNewViewTransition);
	}
	
	public U9Transition GetPopViewTransition (bool force = false)
	{

		PrintStack();

		U9View oldView = null, newView = null;
		if (viewStack.Count > 0) {
			oldView = viewStack.Pop ();
		}
		if (viewStack.Count > 0) {
			newView = viewStack.Peek ();
		}
		
		U9Transition hideOldView = null, displayNewView = null;
		if (oldView) {
			hideOldView = oldView.GetHideTransition (force);
		}
		if (newView) {
			displayNewView = newView.GetDisplayTransition (force);
		}

	
		//Debug.Log ("POP");
		//PrintStack();
		
		return U9T.S (hideOldView, displayNewView);
	}
	
	void PrintStack ()
	{
		foreach (U9View v in viewStack) {
			Debug.Log ("Stack: " + v.name);
		}
	}
}
