using UnityEngine;
using System.Collections;
// U9T.cs
//
// Static class of shorthand convienience methods for creating transitions.
//
// Created by Nick McVroom-Amoakohene <nicholas@unit9.com> on 06/09/2012.
// 
// Copyright (c) 2012 unit9 ltd. www.unit9.com. All rights reserved

//#define USE_HOTWEEN

public static class U9T {
	#region Convienience methods
	
	/// <summary>
	/// Shorthand for the <see cref="U9NullTransition"/>.
	/// </summary>
	public static U9NullTransition Null() {
		return new U9NullTransition();
	}
	
	/// <summary>
	/// Shorthand for the <see cref="U9ParallelTransition"/>.
	/// </summary>
	/// <param name='transitions'>
	/// Transitions.
	/// </param>
	public static U9ParallelTransition P(params U9Transition[] transitions) {
		ValidateTransitionArray(transitions);
		return new U9ParallelTransition(transitions);
	}
	
	/// <summary>
	/// Shorthand for the <see cref="U9SerialTransition"/>.
	/// </summary>
	/// <param name='transitions'>
	/// Transitions.
	/// </param>
	public static U9SerialTransition S(params U9Transition[] transitions) {
		ValidateTransitionArray(transitions);
		return new U9SerialTransition(transitions);
	}
	
#if USE_HOTWEEN
	/// <summary>
	/// Shorthand for the <see cref="U9HOTweenTransition"/>.
	/// Uses the default constructor. For more control, use the other overloads directly.
	/// </summary>
	/// <param name='hoTweenMethod'>
	/// HOTween method.
	/// </param>
	/// <param name='target'>
	/// Target.
	/// </param>
	/// <param name='duration'>
	/// Duration.
	/// </param>
	/// <param name='parms'>
	/// Parameters.
	/// </param>
	public static U9HOTweenTransition T(U9HOTweenTransition.U9HoTweenDelegate hoTweenMethod, object target, float duration, Holoville.HOTween.TweenParms parms) {
		return new U9HOTweenTransition(hoTweenMethod, target, duration, parms);
	}
	
	/// <summary>
	/// Shorthand for the <see cref="U9HOTweenTransition"/>.
	/// Uses the default constructor, with the option to control whether it should be destroyed on completion. For more control, use the other overloads directly.
	/// </summary>
	/// <param name='hoTweenMethod'>
	/// HOTween method.
	/// </param>
	/// <param name='target'>
	/// Target.
	/// </param>
	/// <param name='duration'>
	/// Duration.
	/// </param>
	/// <param name='ignoreTimescale'>
	/// Determines whether this tween should ignore timescale.
	/// </param>
	/// <param name='parms'>
	/// Parameters.
	/// </param>
	public static U9HOTweenTransition T(U9HOTweenTransition.U9HoTweenDelegate hoTweenMethod, object target, float duration, bool ignoreTimescale, Holoville.HOTween.TweenParms parms) {
		return new U9HOTweenTransition(hoTweenMethod, target, duration, ignoreTimescale, parms);
	}
	
	/// <summary>
	/// Shorthand for the <see cref="U9HOTweenTransition"/>.
	/// Uses the default constructor, with the option to control whether it should be destroyed on completion. For more control, use the other overloads directly.
	/// </summary>
	/// <param name='hoTweenMethod'>
	/// HOTween method.
	/// </param>
	/// <param name='target'>
	/// Target.
	/// </param>
	/// <param name='duration'>
	/// Duration.
	/// </param>
	/// <param name='ignoreTimescale'>
	/// Determines whether this tween should ignore timescale.
	/// </param>
	/// <param name='destroyOnComplete'>
	/// Determines whether this tween should be destroyed when it completes.
	/// </param>
	/// <param name='parms'>
	/// Parameters.
	/// </param>
	public static U9HOTweenTransition T(U9HOTweenTransition.U9HoTweenDelegate hoTweenMethod, object target, float duration, bool ignoreTimescale, bool destroyOnComplete, Holoville.HOTween.TweenParms parms) {
		return new U9HOTweenTransition(hoTweenMethod, target, duration, destroyOnComplete, ignoreTimescale, parms);
	}
#endif
	
#if !USE_HOTWEEN
	/// <summary>
	/// Shorthand for the <see cref="U9HOTweenTransition"/>.
	/// Uses the default constructor. For more control, use the other overloads directly.
	/// </summary>
	/// <param name='hoTweenMethod'>
	/// HOTween method.
	/// </param>
	/// <param name='target'>
	/// Target.
	/// </param>
	/// <param name='duration'>
	/// Duration.
	/// </param>
	/// <param name='parms'>
	/// Parameters.
	/// </param>
	public static U9iTweenTransition T( U9iTweenTransition.iTweenDelegate hoTweenMethod, GameObject target, Hashtable args) {
		return new U9iTweenTransition( hoTweenMethod, target, args);
	}
#endif
	
	/// <summary>
	/// Shorthand for the <see cref="U9WaitTransition"/>.
	/// Uses the default constructor. For more control, use the other overloads directly.
	/// </summary>
	/// <param name='waitSeconds'>
	/// Wait seconds.
	/// </param>
	public static U9WaitTransition W(float waitSeconds) {
		return new U9WaitTransition(waitSeconds);
	}
	
	/// <summary>
	/// Shorthand for the <see cref="U9WaitTransition"/>.
	/// Uses the default constructor with the option to ignore timescale. For more control, use the other overloads directly.
	/// </summary>
	/// <param name='waitSeconds'>
	/// Wait seconds.
	/// </param>
	/// <param name='ignoreTimescale'>
	/// Ignore timescale.
	/// </param>
	public static U9WaitTransition W(float waitSeconds, bool ignoreTimescale) {
		return new U9WaitTransition(waitSeconds, ignoreTimescale);
	}
	
	public static void ValidateTransitionArray( U9Transition[] transitions ) {
		for( int i = 0, ni = transitions.Length ; i < ni ; i++ ) {
			if( transitions[i] == null ) {
				transitions[i] = U9T.Null();
			}
		}
	}
	
	/// <summary>
	/// Plays transitions in parallel, with a specified interval between each one.
	/// </summary>
	/// <param name='staggerOffset'>
	/// Start time offset.
	/// </param>
	/// <param name='transitions'>
	/// Transitions to stagger.
	/// </param>
	public static U9ParallelTransition Stagger(float staggerOffset, params U9Transition[] transitions) {
		return Stagger ( staggerOffset, false, transitions );
	}
	
	/// <summary>
	/// Plays transitions in parallel, with a specified interval between each one.
	/// </summary>
	/// <param name='staggerOffset'>
	/// Start time offset.
	/// </param>
	/// <param name='ignoreTimescale'>
	/// Set to true to ignore timescale settings.
	/// </param>
	/// <param name='transitions'>
	/// Transitions to stagger.
	/// </param>
	public static U9ParallelTransition Stagger(float staggerOffset, bool ignoreTimescale, params U9Transition[] transitions) {
		U9ParallelTransition stagger = new U9ParallelTransition();
		
		float currentOffset = 0f;
		
		foreach(U9Transition t in transitions) {
			if(t && !t.IsNull ) {
				stagger.AddTransition(U9T.S(U9T.W(currentOffset, ignoreTimescale), t));
				currentOffset += staggerOffset;
			}
		}
		
		return stagger;
	}
	
	#endregion
}

