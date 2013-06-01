using UnityEngine;
using System.Collections;

public class UISheenSprite : UISprite {

	public float xOffset = 0f, yOffset = 0f;

	public override void OnFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
		base.OnFill (verts, uvs, cols);
	}

	/// <summary>
	/// Tiled sprite fill function.
	/// </summary>

	void TiledFill (BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
		Texture tex = material.mainTexture;
		if (tex == null) return;

		Rect rect = mInner;

		if (atlas.coordinates == UIAtlas.Coordinates.TexCoords)
		{
			rect = NGUIMath.ConvertToPixels(rect, tex.width, tex.height, true);
		}

		Vector2 scale = cachedTransform.localScale;
		float pixelSize = atlas.pixelSize;
		float width = Mathf.Abs(rect.width / scale.x) * pixelSize;
		float height = Mathf.Abs(rect.height / scale.y) * pixelSize;

		// Safety check. Useful so Unity doesn't run out of memory if the sprites are too small.
		if (width < 0.01f || height < 0.01f)
		{
			Debug.LogWarning("The tiled sprite (" + NGUITools.GetHierarchy(gameObject) + ") is too small.\nConsider using a bigger one.");

			width = 0.01f;
			height = 0.01f;
		}

		Vector2 min = new Vector2(rect.xMin / tex.width, rect.yMin / tex.height);
		Vector2 max = new Vector2(rect.xMax / tex.width, rect.yMax / tex.height);
		Vector2 clipped = max;

		Color colF = color;
		colF.a *= mPanel.alpha;
		Color32 col = atlas.premultipliedAlpha ? NGUITools.ApplyPMA(colF) : colF;
		float y = 0f;

		while (y < 1f)
		{
			float x = 0f;
			clipped.x = max.x;

			float y2 = y + height + yOffset;

			if (y2 > 1f)
			{
				clipped.y = min.y + (max.y - min.y) * (1f - y) / (y2 - y);
				y2 = 1f;
			}

			while (x < 1f)
			{
				float x2 = x + width + xOffset;

				if (x2 > 1f)
				{
					clipped.x = min.x + (max.x - min.x) * (1f - x) / (x2 - x);
					x2 = 1f;
				}

				verts.Add(new Vector3(x2, -y, 0f));
				verts.Add(new Vector3(x2, -y2, 0f));
				verts.Add(new Vector3(x, -y2, 0f));
				verts.Add(new Vector3(x, -y, 0f));

				uvs.Add(new Vector2(clipped.x, 1f - min.y));
				uvs.Add(new Vector2(clipped.x, 1f - clipped.y));
				uvs.Add(new Vector2(min.x, 1f - clipped.y));
				uvs.Add(new Vector2(min.x, 1f - min.y));

				cols.Add(col);
				cols.Add(col);
				cols.Add(col);
				cols.Add(col);

				x += width;
			}
			y += height;
		}
	}

}
