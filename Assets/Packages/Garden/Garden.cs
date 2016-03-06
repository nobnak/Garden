using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace GardenSystem {

    public class Garden : MonoBehaviour {
        public Camera targetCamera;
        public GameObject[] plantfabs;
        public float plantRange = 1f;
        public float interference = 1.2f;

        public Vector3 rotationSpeed = new Vector3 (10f, 0.1f, 0.1f);

        float _lastReproduceTime;
        Vector3 _lastReproduceLocalPos;
        List<PlantData> _plants;
        int _totalCount;

        int[] _tmpTypeCounts;

    	void OnEnable() {
    		_lastReproduceTime = float.MinValue;
            _plants = new List<PlantData> ();
            _totalCount = 0;

            _tmpTypeCounts = new int[plantfabs.Length];
        }
        void OnDrawGizmos() {
            if ((Time.timeSinceLevelLoad - _lastReproduceTime) < 3f) {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere (transform.TransformPoint (_lastReproduceLocalPos), plantRange);
            }
        }
        void Update() {
            var t = Time.timeSinceLevelLoad * rotationSpeed.z;
            var loop = _plants.Count;
            for (var i = 0; i < loop; i++) {
                var tr = _plants [i].obj.transform;
                var localPos = tr.localPosition;
                tr.localRotation = Quaternion.Euler (
                    rotationSpeed.x * Noise(rotationSpeed.y * localPos.x, rotationSpeed.y * localPos.y + t),
                    0f, 
                    rotationSpeed.x * Noise (rotationSpeed.y * localPos.y + t, rotationSpeed.x * localPos.y));
            }
        }

        public bool Reproduce(Vector2 viewportPos) {
            var localPos = transform.InverseTransformPoint (targetCamera.ViewportToWorldPoint (viewportPos));
            localPos += plantRange * Random.insideUnitSphere;
            localPos.y = 0f;

            var totalCount = CountNeighbors (_tmpTypeCounts, localPos, plantRange);

            int typeId;
            if (!RouletteWheelSelection.Sample (out typeId, int.MaxValue, 1f, WeightFunc(_tmpTypeCounts, totalCount), plantfabs.Length))
                return false;

            var plant = Instantiate (plantfabs [typeId]);
            plant.transform.SetParent (transform, false);
            plant.transform.localPosition = localPos;
            _plants.Add (new PlantData (){ typeId = typeId, obj = plant });
            _totalCount++;

            _lastReproduceTime = Time.timeSinceLevelLoad;
            _lastReproduceLocalPos = localPos;
            return true;
        }

        int CountNeighbors(int[] typeCounters, Vector3 center, float radius) {
            System.Array.Clear (typeCounters, 0, typeCounters.Length);

            var r2 = radius * radius;
            var total = 0;
            foreach (var p in _plants) {
                var d2 = (p.obj.transform.localPosition - center).sqrMagnitude;
                if (d2 < r2) {
                    typeCounters [p.typeId]++;                    
                    total++;
                }
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
        float Noise(float x, float y) {
            return 2f * Mathf.PerlinNoise (x, y) - 1f;
        }            

        public class PlantData {
            public int typeId;
            public GameObject obj;
        }
    }
}
