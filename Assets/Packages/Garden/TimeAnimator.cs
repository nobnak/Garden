using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GardenSystem {
        
	public class TimeAnimator : Garden.ModifierAbstract {
        public string propTime = "_AnimTex_T";
        public float lifetime = 60f;

		List<Transform> _tmpTransforms = new List<Transform>();

    	void Update () {
            var dt = Time.deltaTime;
			foreach (var p in garden.Plants())
                p.AddTime (propTime, dt, lifetime);
    	}

        public override void Add(Transform plant) {}
		public override void Remove(Transform plant) {}
        public IList<Transform> DeadPlants() {
            _tmpTransforms.Clear ();
			foreach (var p in garden.Plants())
                if (p.time >= lifetime)
					_tmpTransforms.Add (p.transform);
			return _tmpTransforms;
        }
    }

	public partial class PlantData {
		public Renderer renderer;
		public MaterialPropertyBlock block;
		public float time;

		partial void Init() {
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
