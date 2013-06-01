using UnityEngine;
using System.Collections;

public class U9FadeView : U9TransitionView {
	
	[SerializeField]
	MonoFader[] fadables;
	
	public abstract class MonoFader : MonoBehaviour {
		public abstract float Alpha { get; set; }
		public abstract bool Fading { set; }
	}
	
	#region implemented abstract members of U9View
	protected override U9Transition CreateDisplayTransition ( bool force )
	{
		return U9T.T ( iTween.ValueTo, gameObject, iTween.Hash( "time", transitionDuration, "from", 0f, "to", 1f, "easetype", transitionEaseType, "onupdate", "SetAlphas", "ignoretimescale", ignoreTimeScale ) );
	}

	protected override U9Transition CreateHideTransition ( bool force )
	{
		return U9T.T ( iTween.ValueTo, gameObject, iTween.Hash( "time", transitionDuration, "from", 1f, "to", 0f, "easetype", transitionEaseType, "onupdate", "SetAlphas", "ignoretimescale", ignoreTimeScale ) );
	}
	#endregion
	
	protected override void BeginDisplay ()
	{
		base.BeginDisplay ();
		for( int i = 0, ni = fadables.Length ; i < ni ; i++ ) {
			fadables[i].Fading = true;
		}
	}
	
	protected override void EndDisplay ()
	{
		base.EndDisplay ();
		for( int i = 0, ni = fadables.Length ; i < ni ; i++ ) {
			fadables[i].Fading = false;
		}
		SetAlphas( 1f );
	}
	
	protected override void BeginHide ()
	{
		base.BeginHide ();
		for( int i = 0, ni = fadables.Length ; i < ni ; i++ ) {
			fadables[i].Fading = true;
		}
	}
	
	protected override void EndHide ()
	{
		base.EndHide ();	
		for( int i = 0, ni = fadables.Length ; i < ni ; i++ ) {
			fadables[i].Fading = true;
		}
		SetAlphas( 0f );
	}
	
	void SetAlphas( float a ) {
		for( int i = 0, ni = fadables.Length ; i < ni ; i++ ) {
			fadables[i].Alpha = a;
		}
	}

}

