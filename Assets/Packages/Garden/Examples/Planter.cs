using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gist;

namespace GardenSystem {

	public class Planter : MonoBehaviour {
		public const int TIME_STENCIL_BIRTH = 0;
		public const int TIME_STENCIL_DIE = 1;
        public const float ROUND_IN_DEG = 360f;

        public Garden garden;
        public TimeAnimator animator;
        public GameObject[] planttypes;
        public float perturbation = 1f;
        public float searchRadius = 1f;
        public float tiltPower = 1f;

        public float debugHoldTime = 1f;
        public Color debugColorAdd = Color.green;
        public Color debugColorRemove = Color.red;

        GLFigure _figure;
        List<DebugMarker> _markers;

        void Start() {
            _figure = new GLFigure ();
            _markers = new List<DebugMarker> ();

            garden.InitTypeCount (planttypes.Length);
        }
        void OnDestroy() {
            _figure.Dispose();
        }
    	void Update () {
            if (Input.GetMouseButton (0)) {
                var worldPos = WorldMousePos();
                _markers.Add (new DebugMarker (worldPos, searchRadius, debugColorAdd));

                var localPos = PerturbedLocalPos (worldPos);
                var typeId = garden.Sample (localPos, searchRadius);
                if (typeId >= 0) {
                    var p = Instantiate (planttypes [typeId]);
                    p.transform.localPosition = localPos;
                    AddPlant (typeId, p);
                }
            }
            if (Input.GetMouseButton (1)) {
                var worldPos = WorldMousePos();
                _markers.Add (new DebugMarker (worldPos, searchRadius, debugColorRemove));

                var localPos = PerturbedLocalPos (worldPos);
                foreach (var p in garden.Neighbors(localPos, searchRadius)) {
                    animator.SetStencil (p, TIME_STENCIL_DIE);
                }
            }

            foreach (var p in animator.DeadPlants())
                RemovePlant (p);
        }
		void OnRenderObject() {
            _figure.ZTestMode = GLFigure.ZTestEnum.ALWAYS;
            _figure.ZWriteMode = false;

            var timeLimit = Time.timeSinceLevelLoad - debugHoldTime;
            var rot = Camera.main.transform.rotation;
            for (var i = 0; i < _markers.Count;) {
                var m = _markers [i];
                _figure.FillCircle (m.pos, rot, m.size, m.color);
                if (m.time < timeLimit)
                    _markers.RemoveAt (i);
                else
                    i++;
            }
		}

        Vector3 WorldMousePos() {
            var garden2camInWorld = garden.transform.position - garden.targetCamera.transform.position;
            var mousePos = Input.mousePosition;
            mousePos.z = Vector3.Dot (garden2camInWorld, garden.targetCamera.transform.forward);
            var worldPlantPos = garden.targetCamera.ScreenToWorldPoint (mousePos);
            return worldPlantPos;
        }
        Vector3 PerturbedLocalPos(Vector3 worldPlantPos) {
            worldPlantPos += perturbation * Random.insideUnitSphere;
			var localPlantPos = garden.transform.InverseTransformPoint (worldPlantPos);
			localPlantPos.y = 0f;
			return localPlantPos;
		}

        void AddPlant (int typeId, GameObject p) {
            garden.Add (typeId, p.transform);
        }
        void RemovePlant (Transform plant) {
            garden.Remove (plant);
            Destroy (plant.gameObject);
        }

        public class PlantWelfare {
            public readonly Renderer Renderer;
            public readonly MaterialPropertyBlock Block;

            public PlantWelfare(GameObject parent) {
                this.Renderer = parent.GetComponentInChildren<Renderer>();

                Renderer.GetPropertyBlock(this.Block = new MaterialPropertyBlock());
            }
            public void Apply() {
                Renderer.SetPropertyBlock (Block);
            }
        }
        public class DebugMarker {
            public Vector3 pos;
            public Vector2 size;
            public Color color;
            public float time;

            public DebugMarker(Vector3 pos, float radius, Color color) {
                this.pos = pos;
                this.size = radius * Vector2.one;
                this.color = color;
                this.time = Time.timeSinceLevelLoad;
            }
        }
    }
}