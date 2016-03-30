using UnityEngine;
using System.Collections;
using Gist;

namespace GardenSystem {

	public class HashGridTester : MonoBehaviour {
		public const float SEED = 100f;

		public Marker marker;
		public float posfreq = 0.1f;
		public float posrad = 1f;
		public float rotfreq = 0.1f;
		public float rotspeed = 0.1f;
		public float interval = 0.01f;

		Vector3 _seed;
		HashGrid<MonoBehaviour> _world;
		float _nextInterval = float.MinValue;

		void Start() {
			_seed = new Vector3 (Random.Range (-SEED, SEED), Random.Range (-SEED, SEED), Random.Range (-SEED, SEED));
			_world = HashGrid.World;
		}
		void Update () {
			var t = Time.timeSinceLevelLoad;
			var pt = t * posfreq;
			marker.transform.position = new Vector3 (
				posrad * Noise (pt + _seed.x, _seed.y),
				posrad * Noise (pt + _seed.y, _seed.z),
				posrad * Noise (pt + _seed.z, _seed.x));

			var qt = t * rotfreq;
			var dt = Time.deltaTime * rotspeed;
			transform.localRotation *= Quaternion.Euler (
				dt * Noise (qt + _seed.x, _seed.y),
				dt * Noise (qt + _seed.y, _seed.z),
				dt * Noise (qt + _seed.z, _seed.x));

			if (_nextInterval <= t) {
				_nextInterval = t + interval;
				var inst = Instantiate (marker.transform);
				inst.SetParent (transform, true);
				inst.position = marker.transform.position;
				var m = inst.GetComponent<Marker> ();
				_world.Add (m);
			}
		}

		float Noise(float x, float y) {
			return 2f * Mathf.PerlinNoise (x, y) - 1f;
		}
	}
}