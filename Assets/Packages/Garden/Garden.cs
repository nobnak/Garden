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

        HashGrid<PlantData> _plants;
        int _typeCount;
        int[] _tmpCountPerType;

    	void OnEnable() {
            _plants = new HashGrid<PlantData> ((p) => p.transform.localPosition);
            InitTypeCount (_typeCount = 0);
        }
        void OnDisable() {
			_plants.Dispose ();
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
        public void Add(int typeId, Transform plant) {
            plant.transform.SetParent (transform, false);
            _plants.Add (new PlantData (){ typeId = typeId, transform = plant });
        }
        public void Remove(Transform plant) {
            var p = _plants.Find((i) => i.transform == plant);
            if (p != null)
                _plants.Remove (p);
        }
        public IEnumerable<PlantData> Neighbors(Vector3 center, float distance) {
            return _plants.Neighbors (center, distance);
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

        public class PlantData {
            public int typeId;
            public Transform transform;
        }
    }
}
