using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gist;

namespace GardenSystem {

    public class Wind : MonoBehaviour {
        public const float ROUND_IN_DEG = 360f;

        public ScreenNoiseMap noiseMap;
        public float tiltPower = 1f;

        List<PlantData> _plants;

        void OnEnable() {
            _plants = new List<PlantData> ();
        }
        void OnDisable() {
            _plants.Clear ();
        }
    	void Update () {
            Wave ();
        }

        void Wave () {
            foreach (var p in _plants) {
                var tr = p.transform;
                var n = noiseMap.GetYNormalFromWorldPos (tr.position);
                tr.localRotation = Quaternion.FromToRotation (Vector3.up, n) * p.tilt;
            }
        }

        public void Add(Transform plant) {
            var gaussian = BoxMuller.Gaussian ();
            var tilt = Quaternion.Euler (tiltPower * gaussian.x, Random.Range(0f, ROUND_IN_DEG), tiltPower * gaussian.y);            
            _plants.Add (new PlantData(){ transform = plant, tilt = tilt} );
        }
        public void Remove(Transform plant) {
            _plants.RemoveAll (((PlantData obj) => obj.transform == plant));
        }

        public class PlantData {
            public Transform transform;
            public Quaternion tilt;
        }
    }
}