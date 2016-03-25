﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GardenSystem {
        
	public class TimeAnimator : Garden.ModifierAbstract {
		public enum StencilEnum { Birth = 0, Die }
        public string propTime = "_AnimTex_T";
        public float lifetime = 60f;
		public Epoch[] epochs;

		List<Transform> _tmpTransforms = new List<Transform>();

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

        public IList<Transform> DeadPlants() {
            _tmpTransforms.Clear ();
			foreach (var p in garden.Plants())
                if (p.time >= lifetime)
					_tmpTransforms.Add (p.transform);
			return _tmpTransforms;
        }
		public void SetStencil(PlantData p, StencilEnum stencil) {
			var istencil = (int)stencil;
			foreach (var e in epochs) {
				if (e.Match (p, istencil)) {
					p.stencil = istencil;
					return;
				}
			}
		}

		[System.Serializable]
		public class Epoch {
			public float fromTime;
			public float toTime;
			public int stencil;
			public float speed;

			public bool Match(PlantData p) {
				return Match (p, p.stencil);
			}
			public bool Match(PlantData p, int epochStencil) {
				return fromTime <= p.time && p.time < toTime && stencil == epochStencil;
			}
			public float Delta(float dt) {
				return dt * speed;
			}
		}
    }

	public partial class PlantData {
		public Renderer renderer;
		public MaterialPropertyBlock block;
		public float time;
		public int stencil;

		partial void Init() {
			this.renderer = transform.GetComponentInChildren<Renderer>();
			this.renderer.GetPropertyBlock(this.block = new MaterialPropertyBlock());
			this.time = 0f;
			this.stencil = 0;
		}

		public void SetTime(string prop, float time) {
			this.time = time;
			block.SetFloat (prop, time);
			renderer.SetPropertyBlock (block);
		}
		public void AddTime(string prop, float dt, float lifetime) {
			var time = this.time + dt;
			if (time >= lifetime)
				time = lifetime;
			SetTime(prop, time);
		}
	}
}
