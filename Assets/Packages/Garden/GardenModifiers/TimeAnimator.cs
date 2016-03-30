using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GardenSystem {
        
	public class TimeAnimator : Garden.ModifierAbstract {
        public string propTime = "_AnimTex_T";
        public float lifetime = 60f;
		public Epoch[] epochs;

        List<Plant> _tmpDeads = new List<Plant>();

    	void Update () {
            var dt = Time.deltaTime;
			foreach (var p in garden.Plants()) {
				foreach (var e in epochs) {
					if (e.Match (p)) {
						p.AddTime (propTime, e.Delta(dt), lifetime);
						break;
					}
				}
			}
    	}

        public override void Add (Plant p) {
            p.SetTime (propTime, 0f);
        }

        public IList<Plant> DeadPlants() {
            _tmpDeads.Clear ();
			foreach (var p in garden.Plants())
                if (p.time >= lifetime)
					_tmpDeads.Add (p);
			return _tmpDeads;
        }
		public void SetStencil(Plant p, int stencil) {
			foreach (var e in epochs) {
				if (e.Match (p, stencil)) {
					p.stencil = stencil;
					return;
				}
			}
			Debug.LogFormat ("No Match on stencil={0}", stencil);
		}

		[System.Serializable]
		public class Epoch {
			public float fromTime;
			public float toTime;
			public int stencil;
			public float speed;

			public bool Match(Plant p) {
				return Match (p, p.stencil);
			}
			public bool Match(Plant p, int epochStencil) {
				return fromTime <= p.time && p.time < toTime && stencil == epochStencil;
			}
			public float Delta(float dt) {
				return dt * speed;
			}
		}
    }

	public partial class Plant {
        [HideInInspector]
		public Renderer rndr;
        [HideInInspector]
		public MaterialPropertyBlock block;
        [HideInInspector]
		public float time;
        [HideInInspector]
		public int stencil;

		partial void Init() {
			this.rndr = transform.GetComponentInChildren<Renderer>();
			this.rndr.GetPropertyBlock(this.block = new MaterialPropertyBlock());
			this.time = 0f;
			this.stencil = 0;
		}

		public void SetTime(string prop, float time) {
			this.time = time;
			block.SetFloat (prop, time);
			rndr.SetPropertyBlock (block);
		}
		public void AddTime(string prop, float dt, float lifetime) {
			var time = this.time + dt;
			if (time >= lifetime)
				time = lifetime;
			SetTime(prop, time);
		}
	}
}
