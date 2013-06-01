using UnityEngine;
using System.Collections;
using System;

public class U9Button : MonoBehaviour {

	[SerializeField]
	UISprite buttonSprite = null;

	[SerializeField]
	string spritePrefix = "";

	public enum Status {
		Disabled,
		Normal,
		Hover,
		Pressed
	}
	Status currentStatus = Status.Normal;

	public virtual Status CurrentStatus {
		get {
			return this.currentStatus;
		}
		private set {
			if( currentStatus != value ) {
				currentStatus = value;
				OnStatusChanged(currentStatus);
			}
		}
	}
	
	public class StatusChangeEventArgs : EventArgs {
		public Status Status { get; set; }
	}
	
	bool hidden, disabled;

	bool Hidden {
		get {
			return this.hidden;
		}
		set {
			hidden = value;
		}
	}
	
	public virtual bool Disabled {
		get {
			return this.disabled;
		}
		set {
			if (disabled != value) {
				disabled = value;
				if (value) {
					CurrentStatus = Status.Disabled;
				} else {
					CurrentStatus = Status.Normal;
				}
			}
		}
	}
	
	bool IsEnabled {
		get {
			return !( Hidden || Disabled );
		}
	}
	
	public event System.EventHandler Clicked;
	public event System.EventHandler<StatusChangeEventArgs> StatusChanged;
	
	public object Data { get; set; }
	
	protected virtual void OnClicked() {	
		if( Clicked != null ) {
			Clicked(this,null);
		}
	}
	
	protected virtual void OnStatusChanged( Status newStatus ) {	
		if (buttonSprite) {
			string s = spritePrefix + newStatus.ToString ();
			buttonSprite.spriteName = s;
		}

		if( StatusChanged != null ) {
			StatusChanged(this, new StatusChangeEventArgs() { Status = newStatus } );
		}
	}
	
	protected virtual void OnViewDisplayed() {
		Hidden = false;
	}
	
	protected virtual void OnViewHidden() {
		Hidden = true;
	}
	
	void OnPress (bool isPressed) {
		if( IsEnabled ) {
			if( isPressed ) {
				CurrentStatus = Status.Pressed;
			} else {
				CurrentStatus = Status.Normal;
			}
		}
	}
	
	protected virtual void OnHover (bool isOver) {
		if( IsEnabled ) {
			if( isOver ) {
				CurrentStatus = Status.Hover;
			} else {
				CurrentStatus = Status.Normal;
			}
		}
	}
	
	protected virtual void OnClick() {
		if( IsEnabled ) {
			OnClicked();
		}
	}	
	
}
