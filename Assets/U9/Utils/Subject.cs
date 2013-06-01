using UnityEngine;
using System.Collections.Generic;

public class Subject<T> {
	
	T currentValue;
	
	public delegate void ChangedHandler( T oldValue, T newValue );
	private event ChangedHandler ChangedInternal;
	
	public delegate U9Transition ChangedWithTransitionHandler( T oldValue, T newValue );
	
	List<ChangedWithTransitionHandler> changedWithTransitionHandlers;
	
	public T Value {
		get {
			return this.currentValue;
		}
		set {
			T oldValue = this.currentValue;
			this.currentValue = value;
			//Notify(oldValue);
			U9T.P ( NotifyWithTransition(oldValue) ).Begin();
		}
	}
	
	public Subject() : this( default(T) ) {
		
	}
	
	public Subject( T value ) {
		this.currentValue = value;
		changedWithTransitionHandlers = new List<ChangedWithTransitionHandler>();
	}
	
	public event ChangedHandler Changed {
		add {
			ChangedInternal += value;
			value(currentValue,currentValue);
		}		
		remove {
			ChangedInternal -= value;
		}
	}
	
	public event ChangedWithTransitionHandler ChangedWithTransition {
		add {
			changedWithTransitionHandlers.Add(value);
			value(currentValue,currentValue).Begin();
		}		
		remove {
			changedWithTransitionHandlers.Remove(value);
		}
	}
	
	public void Notify( T oldValue ) {
		if( ChangedInternal != null ) {
			ChangedInternal( oldValue, currentValue );
		}
	}
	
	public U9Transition[] SetValueWithTransition( T value ) {
		T oldValue = this.currentValue;
		this.currentValue = value;
		return NotifyWithTransition(oldValue);
	}
	
	public U9Transition[] NotifyWithTransition( T oldValue ) {
		Notify (oldValue);
		
		List<U9Transition> ts = new List<U9Transition>();
		foreach( ChangedWithTransitionHandler h in changedWithTransitionHandlers ) {
			ts.Add( h(oldValue,currentValue) );
		}
		return ts.ToArray();
	}
	
}
