using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gist;

namespace GardenSystem {

	public class Wind : Garden.ModifierAbstract {
        public const float ROUND_IN_DEG = 360f;

        public ScreenNoiseMap noiseMap;
        public float tiltPower = 1f;

    	void Update () {
            Wave ();
        }

        void Wave () {
			foreach (var p in garden.Plants()) {
                var tr = p.transform;
                var n = noiseMap.GetYNormalFromWorldPos (tr.position);
                tr.localRotation = Quaternion.FromToRotation (Vector3.up, n) * p.tilt;
            }
        }

		public override void Add(Transform plant) {
			var p = garden.FindPlant (plant);
			if (p != null)
				p.tilt = InitialTilt();
        }
		public override void Remove(Transform plant) {}

		Quaternion InitialTilt () {
			var gaussian = BoxMuller.Gaussian ();
			var tilt = Quaternion.Euler (tiltPower * gaussian.x, Random.Range (0f, ROUND_IN_DEG), tiltPower * gaussian.y);
			return tilt;
		}
    }

	public partial class PlantData {
		public Quaternion tilt;
	}
}