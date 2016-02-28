using UnityEngine;
using System.Collections;

public class Planter : MonoBehaviour {
	public GameObject[] plants;
	public Collider groundCollider;
	public float range = 1f;

	float _lastHitTime;
	Vector3 _lastHitPoint;

	void OnEnable() {
		_lastHitTime = float.MinValue;
	}
	void Update () {
		if (Input.GetMouseButton (0)) {
			RaycastHit hit;
			var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (groundCollider.Raycast (ray, out hit, float.MaxValue)) {
				_lastHitTime = Time.timeSinceLevelLoad;
				_lastHitPoint = hit.point;

					var p = Instantiate (plants [Random.Range (0, plants.Length)]);
					p.transform.SetParent (transform, false);
					p.transform.position = hit.point;
			}
		}
	}
	void OnDrawGizmos() {
		if (!Application.isPlaying)
			return; 
		
		if ((Time.timeSinceLevelLoad - _lastHitTime) < 3f) {
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere (_lastHitPoint, range);
		}
	}
}
