using UnityEngine;
using System.Collections.Generic;

public class UIImageFont : MonoBehaviour {
	
	[SerializeField]
	UIAtlas atlas;

	public UIAtlas Atlas {
		get {
			return this.atlas;
		}
	}
	
	[SerializeField]
	float letterSpacing = 1f;

	public float LetterSpacing {
		get {
			return this.letterSpacing;
		}
	}	
	
	[System.Serializable]
	class UIImageFontCharacter {
		[SerializeField]
		string character;
		[SerializeField]
		string spriteName;

		public string Character {
			get {
				return this.character;
			}
		}

		public string SpriteName {
			get {
				return this.spriteName;
			}
		}
	}
	
	[SerializeField]
	UIImageFontCharacter[] imageFontCharacters;
	
	Dictionary<string,UIAtlas.Sprite> characterMap;

	public Dictionary<string, UIAtlas.Sprite> CharacterMap {
		get {
			if( characterMap == null ) {
				characterMap = new Dictionary<string, UIAtlas.Sprite>();
				foreach( UIImageFontCharacter c in imageFontCharacters ) {
					characterMap.Add( c.Character, atlas.GetSprite( c.SpriteName ) );
				}
			}
			return this.characterMap;
		}
	}	
	
	void Awake() {
		
	}

	
	public UIAtlas.Sprite GetCharacterSprite( char c ) {
		return CharacterMap[c.ToString()];
	}
	
}

