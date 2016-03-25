using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GardenSystem {
        
    public class TimeAnimator : MonoBehaviour {
        public string propTime = "_AnimTex_T";
        public float lifetime = 60f;

        List<PlantData> _plants;
        List<Transform> _tmp;

        void Awake() {
            _plants = new List<PlantData>();
            _tmp = new List<Transform> ();
        }
        void OnDisable() {
            _plants.Clear ();
        }
    	void Update () {
            var dt = Time.deltaTime;
            foreach (var p in _plants)
                p.AddTime (propTime, dt, lifetime);
    	}

        public void Add(Transform plant) {
            _plants.Add (new PlantData (plant));
        }
        public void Remove(Transform plant) {
            _plants.RemoveAll ((p) => p.transform == plant);
        }
        public IList<Transform> DeadPlants() {
            _tmp.Clear ();
            foreach (var p in _plants)
                if (p.time >= lifetime)
                    _tmp.Add (p.transform);
            return _tmp;
        }

        public class PlantData {
            public Transform transform;
            public Renderer renderer;
            public MaterialPropertyBlock block;
            public float time;

            public PlantData(Transform transform) {
                this.transform = transform;
                this.renderer = transform.GetComponentInChildren<Renderer>();
                this.renderer.GetPropertyBlock(this.block = new MaterialPropertyBlock());
                this.time = 0f;
            }
            public void SetTime(string prop, float time) {
                this.time = time;
                block.SetFloat (prop, time);
                renderer.SetPropertyBlock (block);
            }
            public void AddTime(string prop, float dt, float lifetime) {
                time += dt;
                if (time >= lifetime)
                    time = lifetime;
                block.SetFloat (prop, time);
                renderer.SetPropertyBlock (block);
            }
        }
    }
}
