using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Gist;
using UnityEngine.Events;

namespace GardenSystem {

    public class Garden : MonoBehaviour {
		public CameraEvent OnSetCamera;

        public Camera targetCamera;

        public float interference = 1.2f;
        public ModifierAbstract[] modifiers;
        int _typeCount;
        int[] _tmpCountPerType;

    	void OnEnable() {
            InitTypeCount (_typeCount = 0);
			foreach (var mod in modifiers)
				mod.Set (this);
        }

		public void SetCamera(Camera cam) {
			targetCamera = cam;
			OnSetCamera.Invoke (cam);
		}
        public void Add(Plant p) {
            p.transform.SetParent (transform, false);
            HashGrid.World.Add (p);
			foreach (var mod in EnabledModifiers())
                mod.Add (p);
        }
        public void Remove(Plant p) {
			foreach (var mod in EnabledModifiers())
				mod.Remove (p);
            HashGrid.World.Remove (p);
        }
		public IEnumerable<Plant> Plants() {
            foreach (var m in HashGrid.World) {
                var p = m as Plant;
                if (p != null)
                    yield return p;
            }
		}
        public Plant FindPlant(Transform plant) {
            return (Plant)HashGrid.World.Find ((i) => i.transform == plant);
		}
        public IEnumerable<Plant> Neighbors(Vector3 center, float distance) {
            return HashGrid.World.Neighbors<Plant>(center, distance);
		}
        public int Average(out Vector3 averagedCenter, Vector3 center, float distance) {
            var count = 0;
            averagedCenter = Vector3.zero;
            foreach (var n in Neighbors(center, distance)) {
                count++;
                averagedCenter += n.transform.position;
            }
            if (count > 0)
                averagedCenter /= count;
            return count;
        }

		public void InitTypeCount(int typeCount) {
			if (_tmpCountPerType == null || _tmpCountPerType.Length < typeCount) {
				_typeCount = Mathf.Max (_typeCount, typeCount);
				System.Array.Resize (ref _tmpCountPerType, _typeCount);
			}
		}
        public int Sample(Vector3 localPos, float plantRange, out int totalCount) {
			totalCount = CountNeighbors(_tmpCountPerType, localPos, plantRange);
			var w = WeightFunc(_tmpCountPerType, totalCount);
			int typeId;
			if (RouletteWheelSelection.Sample (out typeId, int.MaxValue, 1f, w, _typeCount))
				return typeId;
			return -1;
		}

        int CountNeighbors(int[] typeCounters, Vector3 center, float radius) {
            System.Array.Clear (typeCounters, 0, typeCounters.Length);

            var total = 0;
            foreach (var p in HashGrid.World.Neighbors<Plant>(center, radius)) {
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

			public virtual void Add(Plant p) {}
            public virtual void Remove(Plant p) {}

        }

		[System.Serializable]
		public class CameraEvent : UnityEngine.Events.UnityEvent<Camera> {}
	}
}
