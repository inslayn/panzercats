using UnityEngine;
using UnityEditor;

public class U9EditorUtils : MonoBehaviour {

	[MenuItem ("U9/Delete Player Prefs")]
	public static void DeletePlayerPrefs() {
		if( EditorUtility.DisplayDialog( "Delete Player Prefs?", "Are you sure you want to delete the player prefs?", "Yep!", "Bugger, no I don't!" ) ) {
			PlayerPrefs.DeleteAll();
		} else {
			if( !EditorUtility.DisplayDialog( "PHEW!", "Bet you're glad I added this confirm dialogue in now eh?", "Yep, I bow to Dave's greatness!", "I bow to nobody... but my prefs also get deleted!" ) ) {
				PlayerPrefs.DeleteAll();
			}
		}
	}
	
	[MenuItem ("Assets/Snag resource path")]
	public static void SnagResourcePath() {
		Object o = Selection.activeObject;
		ClipboardHelper.clipBoard = AssetDatabase.GetAssetPath( o ).Replace("Assets/Decromancer/Resources/","").Replace(".prefab","".Replace(".psd",""));
	}
	
	[MenuItem ("U9/Group Selected Objects %g")]
	public static void GroupSelectedObjects() {
		GameObject[] selectedObjects = Selection.gameObjects;
		
		GameObject group = new GameObject("Group");
		Vector3 pos = Vector3.zero;
		foreach( GameObject go in selectedObjects ) {
			pos += go.transform.position;
			Vector3 scale = go.transform.localScale;
			go.transform.parent = group.transform;
			go.transform.localScale = scale;
		}
		
		pos /= (float)selectedObjects.Length;
		
		group.transform.position = pos;
	}
	
}
