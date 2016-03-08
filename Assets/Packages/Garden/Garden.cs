using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Gist;

namespace GardenSystem {

    public class Garden : MonoBehaviour {
        public const float ROUND_IN_DEG = 360f;

        public Camera targetCamera;
        public ScreenNoiseMap noiseMap;

        public GameObject[] plantfabs;
        public float plantRange = 1f;
        public float interference = 1.2f;

        public float initRotScale = 1f;
        public Vector3 rotationSpeed = new Vector3 (10f, 0.1f, 0.1f);

        public readonly List<PlantData> Plants = new List<PlantData>();

        float _lastReproduceTime;
        Vector3 _lastReproduceLocalPos;
        int _totalCount;

        int[] _tmpTypeCounts;

    	void OnEnable() {
    		_lastReproduceTime = float.MinValue;
            Plants.Clear ();
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
            var loop = Plants.Count;
            var z2y = Quaternion.Euler (-90f, 0f, 0f);
            var y2z = Quaternion.Inverse (z2y);
            for (var i = 0; i < loop; i++) {
                var p = Plants [i];
                var tr = p.obj.transform;
                var n = noiseMap.GetNormal (p.screenUv.x, p.screenUv.y);
                tr.localRotation = z2y * Quaternion.LookRotation (n) * y2z * p.initRotation;
            }
        }

        public bool Add(Vector2 viewportPos, out GameObject plant) {
            var localPos = transform.InverseTransformPoint (targetCamera.ViewportToWorldPoint (viewportPos));
            localPos += plantRange * Random.insideUnitSphere;
            localPos.y = 0f;

            var totalCount = CountNeighbors (_tmpTypeCounts, localPos, plantRange);

            int typeId;
            var w = WeightFunc (_tmpTypeCounts, totalCount);
            if (!RouletteWheelSelection.Sample (out typeId, int.MaxValue, 1f, w, plantfabs.Length)) {
                plant = null;
                return false;
            }

            plant = Instantiate (plantfabs [typeId]);
            plant.transform.SetParent (transform, false);
            plant.transform.localPosition = localPos;
            var gaussian = BoxMuller.Gaussian ();
            var rot = Quaternion.Euler (initRotScale * gaussian.x, Random.Range(0f, ROUND_IN_DEG), initRotScale * gaussian.y);
            Plants.Add (new PlantData (){ typeId = typeId, obj = plant, screenUv = viewportPos, initRotation = rot });
            _totalCount++;

            _lastReproduceTime = Time.timeSinceLevelLoad;
            _lastReproduceLocalPos = localPos;
            return true;
        }
        public bool Remove(GameObject plant) {
            return Plants.RemoveAll ((p) => p.obj == plant) > 0;
        }
        public IEnumerable<PlantData> Neighbors(Vector2 centerUv, float radius) {
            var center = targetCamera.ViewportToWorldPoint (centerUv);
            var r2 = radius * radius;
            foreach (var p in Plants) {
                var d2 = (p.obj.transform.localPosition - center).sqrMagnitude;
                if (d2 < r2)
                    yield return p;
            }
        }

        int CountNeighbors(int[] typeCounters, Vector3 center, float radius) {
            System.Array.Clear (typeCounters, 0, typeCounters.Length);

            var r2 = radius * radius;
            var total = 0;
            foreach (var p in Plants) {
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
            public Vector2 screenUv;
            public Quaternion initRotation;
            public GameObject obj;
        }
    }
}
