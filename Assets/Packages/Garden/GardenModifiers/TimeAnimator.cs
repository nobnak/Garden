using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gist;

namespace GardenSystem {
        
	public class TimeAnimator : Garden.ModifierAbstract {
        public float lifetime = 60f;
        public float alphaScale = 1f;
        public AnimationCurve alphaCurve;
		public Epoch[] epochs;

        List<Plant> _tmpDeads = new List<Plant>();

    	void Update () {
            var dt = Time.deltaTime;
			foreach (var p in garden.Plants()) {
				foreach (var e in epochs) {
					if (e.Match (p)) {
						p.AddTime (e.Delta(dt), lifetime);
						break;
					}
				}
			}
    	}

        public override void Add (Plant p) {
            p.SetTime (0f);
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
        public const string PROP_TIME = "_AnimTex_T";
        public const string PROP_COLOR = "_Color";

        public MaterialPropertyBlockChanied block;
        [HideInInspector]
		public float time;
        [HideInInspector]
        public int stencil;
        public Color color;

        Color _initColor;

		partial void Init() {
			var rndr = transform.GetComponentInChildren<Renderer>();
            this.block = new MaterialPropertyBlockChanied(rndr);
            this.color = _initColor = block.GetDefaultColor(PROP_COLOR);
			this.time = 0f;
			this.stencil = 0;
		}

        public void SetTime(float time) {
            this.time = time;
            block.SetFloat (PROP_TIME, time).Apply ();
        }
        public void AddTime(float dt, float lifetime) {
            var time = this.time + dt;
            if (time >= lifetime)
                time = lifetime;
            SetTime(time);
        }
        public void SetAlpha(float alpha) {
            color.a = alpha * _initColor.a;
            block.SetColor (PROP_COLOR, color).Apply ();
        }
	}
}
