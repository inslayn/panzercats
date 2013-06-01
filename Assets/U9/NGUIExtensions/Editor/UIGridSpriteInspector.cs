//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UISlicedSprites.
/// </summary>

[CustomEditor(typeof(UIGridSprite))]
public class UIGridSpriteInspector : UIWidgetInspector
{
	
	protected UIGridSprite mGridSprite;

	override protected bool DrawProperties ()
	{
		if( base.DrawProperties() ) {
			mGridSprite = mWidget as UIGridSprite;
			
			UIGridSpriteInfo gridSpriteInfo = (UIGridSpriteInfo)EditorGUILayout.ObjectField( "Grid Sprite Info", mGridSprite.GridSpriteInfo, typeof(UIGridSpriteInfo), false );
			if( gridSpriteInfo != mGridSprite.GridSpriteInfo ) {
				mGridSprite.GridSpriteInfo = gridSpriteInfo;
				mGridSprite.material = gridSpriteInfo.Atlas.spriteMaterial;
				mGridSprite.mainTexture = gridSpriteInfo.Atlas.texture;
				mGridSprite.MarkAsChanged();
			}
			int width = EditorGUILayout.IntField( "Grid Width", mGridSprite.GridWidth );
			int height = EditorGUILayout.IntField( "Grid Height", mGridSprite.GridHeight );
			if( width != mGridSprite.GridWidth || height != mGridSprite.GridHeight ) {
				mGridSprite.GridWidth = width;
				mGridSprite.GridHeight = height;
				mGridSprite.MakePixelPerfect();
			}
			
			EditorUtility.SetDirty(mGridSprite.gameObject);
			return true;
		} else {
			return false;
		}
	}
}