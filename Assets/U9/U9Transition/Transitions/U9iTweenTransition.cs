using UnityEngine;
using System.Collections;

public class U9iTweenTransition : U9Transition
{
	
	public delegate void iTweenDelegate( GameObject go, Hashtable args );
	
	iTweenDelegate tweenDelegate;
	GameObject go;
	Hashtable args;

	string name;
	static long uid = 0;

	public U9iTweenTransition( iTweenDelegate tweenDelegate, GameObject go, Hashtable args ) {
		this.tweenDelegate = tweenDelegate;
		this.go = go;
		this.args = args;
	}
	
	public override void Begin ()
	{
		base.Begin ();
		
		args.Add( "oncomplete", (iTween.iTweenCallback)TweenComplete );
		name = "t" + uid++;
		args.Add ("name", name );
		//args.Add( "ignoretimescale", true );
		tweenDelegate( go, args );
	}
			
	void TweenComplete() {
		OnEnded(this);
	}

	public override void Stop ()
	{
		base.Stop ();
		iTween.StopByName (go, name);
	}

    public override string ToString()
    {
        return string.Format("[U9iTweenTransition={0}]", tweenDelegate.Method.Name);
    }
}

