using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Gist;

namespace GardenSystem {

    public class Garden : MonoBehaviour {
        public Camera targetCamera;

        public float plantRange = 1f;
        public float interference = 1.2f;
        public ModifierAbstract[] modifiers;

        HashGrid<PlantData> _plants;
        int _typeCount;
        int[] _tmpCountPerType;

    	void OnEnable() {
            _plants = new HashGrid<PlantData> ((p) => p.transform.localPosition);
            InitTypeCount (_typeCount = 0);
			foreach (var mod in modifiers)
				mod.Set (this);
        }
        void OnDisable() {
			_plants.Dispose ();
        }

        public void Add(int typeId, Transform plant) {
            plant.transform.SetParent (transform, false);
			_plants.Add (new PlantData (typeId, plant));
			foreach (var mod in EnabledModifiers())
                mod.Add (plant);
        }
        public void Remove(Transform plant) {
			var p = FindPlant (plant);
            if (p != null) {
				foreach (var mod in EnabledModifiers())
					mod.Remove (p.transform);
				_plants.Remove (p);
            }
        }
		public IEnumerable<PlantData> Plants() {
			foreach (var p in _plants.Points)
				yield return p;
		}
		public PlantData FindPlant(Transform plant) {
			return _plants.Find ((i) => i.transform == plant);
		}
        public IEnumerable<PlantData> Neighbors(Vector3 center, float distance) {
            return _plants.Neighbors (center, distance);
		}

		public void InitTypeCount(int typeCount) {
			if (_tmpCountPerType == null || _tmpCountPerType.Length < typeCount) {
				_typeCount = Mathf.Max (_typeCount, typeCount);
				System.Array.Resize (ref _tmpCountPerType, _typeCount);
			}
		}
		public int Sample(Vector3 localPos) {
			var totalCount = CountNeighbors(_tmpCountPerType, localPos, plantRange);
			var w = WeightFunc(_tmpCountPerType, totalCount);
			int typeId;
			if (RouletteWheelSelection.Sample (out typeId, int.MaxValue, 1f, w, _typeCount))
				return typeId;
			return -1;
		}

        int CountNeighbors(int[] typeCounters, Vector3 center, float radius) {
            System.Array.Clear (typeCounters, 0, typeCounters.Length);

            var total = 0;
            foreach (var p in _plants.Neighbors(center, radius)) {
                typeCounters [p.typeId]++;                    
                total++;
            }
            return total;
        }
        System.Func<int, float> WeightFunc(int[] typeCounters, int totalCount) {
            return (typeId) => {
                return Weight (typeCounters [typeId], totalCount);
            };
        }
        float Weight(int brothers, int total) {
            var aliens = total - brothers;
            if (aliens <= 0 || total <= 0)
                return 1f;
            var rate = aliens / (float)total;
            var t = rate / interference;
            return Mathf.SmoothStep (1f, 0f, t);
        }

		IEnumerable<ModifierAbstract> EnabledModifiers() {
			foreach (var mod in modifiers)
				if (mod.enabled)
					yield return mod;
		}


        public abstract class ModifierAbstract : MonoBehaviour {
			public Garden garden;

			public virtual void Set(Garden garden) {
				this.garden = garden;
			}

			public virtual void Add(Transform transform) {}
			public virtual void Remove(Transform transform) {}

        }
	}

	public partial class PlantData {
		public int typeId;
		public Transform transform;

		public PlantData(int typeId, Transform transform) {
			this.typeId = typeId;
			this.transform = transform;

			Init();
		}

		partial void Init();
	}
}
