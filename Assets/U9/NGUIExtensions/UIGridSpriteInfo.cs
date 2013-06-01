using UnityEngine;
using System.Collections;

public class UIGridSpriteInfo : MonoBehaviour {
	// Cached and saved values
	[SerializeField] UIAtlas mAtlas;
	
	public UIAtlas Atlas {
		get {
			return this.mAtlas;
		}
		set {
			mAtlas = value;
		}
	}
	
	[SerializeField]
	float gridSize = 1;
	
	[SerializeField]
	float gridPadding = 10;

	[SerializeField] string[] 	topLeftCornerSprites = null, 
								topRightCornerSprites = null, 
								bottomRightCornerSprites = null,
								bottomLeftCornerSprites = null,
								leftEdgeSprites = null,
								topEdgeSprites = null,
								rightEdgeSprites = null,
								bottomEdgeSprites = null,
								centerSprites = null;
	
	

	public float GridSize {
		get {
			return this.gridSize;
		}
	}	
	
	// (x,y) index into sprites for different corners / edges
	// x == 0 // left
	// x == 1 // middle
	// x == 2 // right
	// y == 0 // top
	// y == 1 // middle
	// y == 2 // bottom
	string[,][] sprites;
	string[,][] Sprites {
		get {
			if( sprites == null ) {
				sprites = new string[3,3][];
				sprites[0,0] = topLeftCornerSprites;
				sprites[0,1] = leftEdgeSprites;
				sprites[0,2] = bottomLeftCornerSprites;
				sprites[1,0] = topEdgeSprites;
				sprites[1,1] = centerSprites;
				sprites[1,2] = bottomEdgeSprites;
				sprites[2,0] = topRightCornerSprites;
				sprites[2,1] = rightEdgeSprites;
				sprites[2,2] = bottomRightCornerSprites;
			}
			return sprites;
		}
	}
	void Awake() {
		
	}
	
	public Rect GetSpriteUVs( int i, int j, int gridWidth, int gridHeight ) {
		int x,y;
		if( i == 0 ) {
			x = 0;
		} else if( i == gridWidth-1 ) {
			x = 2;
		} else {
			x = 1; 
		}
		if( j == 0 ) {
			y = 0;
		} else if( j == gridHeight-1 ) {
			y = 2;
		} else {
			y = 1; 
		}
		string[] spriteArray = Sprites[x,y];
		UIAtlas.Sprite sprite = mAtlas.GetSprite( spriteArray[Random.Range(0,spriteArray.Length)] );
		Rect r = sprite.outer;
		r.xMin += gridPadding;
		r.xMax -= gridPadding;
		r.yMin += gridPadding;
		r.yMax -= gridPadding;
		return r;
	}
	
}
