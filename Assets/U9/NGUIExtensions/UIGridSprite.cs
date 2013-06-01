using UnityEngine;
using System.Collections;

public class UIGridSprite : UISprite {
	
	[SerializeField] UIGridSpriteInfo gridSpriteInfo;

	public UIGridSpriteInfo GridSpriteInfo {
		get {
			return this.gridSpriteInfo;
		}
		set {
			gridSpriteInfo = value;
			MarkAsChanged();
		}
	}

	[SerializeField] int gridWidth, gridHeight;

	public int GridHeight {
		get {
			return this.gridHeight;
		}
		set {
			gridHeight = value;
			MarkAsChanged();
		}
	}

	public int GridWidth {
		get {
			return this.gridWidth;
		}
		set {
			gridWidth = value;
			MarkAsChanged();
		}
	}	
	
	protected override void OnStart ()
	{
		base.OnStart ();
		mChanged = true;
	}
	
	public override void MakePixelPerfect ()
	{
		base.MakePixelPerfect ();
		Vector3 scale = cachedTransform.localScale;
		scale.x = 2f * GridSpriteInfo.GridSize * GridWidth;
		scale.y = 2f * GridSpriteInfo.GridSize * GridHeight;
		cachedTransform.localScale = scale;
		MarkAsChanged();
	}
	
	public override void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
				
#if UNITY_3_5_4
		Color col = color;
#else
		Color32 col = color;
#endif
		
		for( int i = 0, ni = 2*gridWidth ; i < ni ; i++ ) {
			float x1 = (float)i/ni;
			float x2 = (float)(i+1)/ni;
			for( int j = 0, nj = 2*gridHeight ; j < nj ; j++ ) {
				float y1 = -(float)j/nj;
				float y2 = -(float)(j+1)/nj;
				
				Texture tex = mainTexture;
				Rect spriteUVRect = NGUIMath.ConvertToTexCoords( gridSpriteInfo.GetSpriteUVs(i,j,ni,nj), tex.width, tex.height);
				
				Vector2 uv0 = new Vector2(spriteUVRect.xMin, spriteUVRect.yMin);
				Vector2 uv1 = new Vector2(spriteUVRect.xMax, spriteUVRect.yMax);
				
				verts.Add(new Vector3(x2, y1, 0f));
				verts.Add(new Vector3(x2, y2, 0f));
				verts.Add(new Vector3(x1, y2, 0f));
				verts.Add(new Vector3(x1, y1, 0f));
				
				uvs.Add(uv1);
				uvs.Add(new Vector2(uv1.x, uv0.y));
				uvs.Add(uv0);
				uvs.Add(new Vector2(uv0.x, uv1.y));

				cols.Add(col);
				cols.Add(col);
				cols.Add(col);
				cols.Add(col);
			}
		}
	}
					
	
}
