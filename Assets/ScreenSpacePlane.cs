using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class ScreenSpacePlane : MonoBehaviour {
	public Camera targetCamera;
	public float distance = 100f;

	Mesh _localMesh;
	Vector3[] _vertices;

	void OnEnable() {
		_vertices = new Vector3[] {
			new Vector3 (-1f, -1f, 0f),
			new Vector3 (1f, -1f, 0f),
			new Vector3 (-1f, 1f, 0f),
			new Vector3 (1f, 1f, 0f)
		};

		_localMesh = new Mesh ();
		_localMesh.MarkDynamic ();
		_localMesh.vertices = _vertices;
		_localMesh.uv = new Vector2[]{ 
			new Vector2 (0f, 0f), new Vector2 (1f, 0f), new Vector2 (0f, 1f), new Vector2 (1f, 1f) };
		_localMesh.triangles = new int[]{ 0, 3, 1, 0, 2, 3 };
		_localMesh.RecalculateBounds ();
		_localMesh.RecalculateNormals ();

		var mf = GetComponent<MeshFilter> ();
		if (mf != null)
			mf.sharedMesh = _localMesh;

		var mc = GetComponent<MeshCollider> ();
		if (mc != null)
			mc.sharedMesh = _localMesh;
	}
	void Update () {
		var cameraSize = targetCamera.orthographicSize;
		transform.SetParent (targetCamera.transform, false);
		transform.localPosition = Vector3.forward * distance;
		transform.localRotation = Quaternion.identity;
		transform.localScale = new Vector3 (targetCamera.aspect * cameraSize, cameraSize, 1f);
	}
}
