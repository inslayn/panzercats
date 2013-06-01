using UnityEngine;
using System.Collections.Generic;

public class ZStack : MonoSingleton<ZStack> {
	
	const float interval = 0.1f;
	
	class TransformToZMap {
		internal Transform Transform { get; set; }
		internal float Z { get; set; }
	}
	
	List<TransformToZMap> transformToZMaps = new List<TransformToZMap>();
	
	void Awake() {
		Instance = this;
	}
	
	public float GetNewZ( Transform t, float minimum = 0f ) {
		float maxZ = minimum;
		foreach( TransformToZMap m in transformToZMaps ) {
			if( m.Z < maxZ ) {
				maxZ = m.Z;
			}
		}
		maxZ += interval;
		transformToZMaps.Add( new TransformToZMap() { Transform = t, Z = maxZ } );
		return maxZ;
	}
	
	public void RemoveZ( Transform t ) {
		transformToZMaps.RemoveAll( (obj) => { return (obj.Transform == t); } );
	}
	
}
