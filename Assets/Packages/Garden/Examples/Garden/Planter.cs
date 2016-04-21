using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gist;

namespace GardenSystem {

	public class Planter : MonoBehaviour {
        public enum DebugInputModeEnum { None = 0, Mouse }
        public enum DebugVisualModeEnum { None = 0, Marker }

        public const string TAG_MAIN_CAMERA = "MainCamera";
		public const int TIME_STENCIL_BIRTH = 0;
		public const int TIME_STENCIL_DIE = 1;
        public const float ROUND_IN_DEG = 360f;

        public Garden garden;
        public TimeAnimator animator;
		public Plant[] plantfabs;
        public float perturbation = 1f;
        public float searchRadius = 1f;
        public float tiltPower = 1f;

        public DebugInputModeEnum debugInputMode;
        public DebugVisualModeEnum debugVisualMode;
        public float debugHoldTime = 1f;
        public Color debugColorAdd = Color.green;
        public Color debugColorRemove = Color.red;
        public float debugRadiusAveragedPoint = 2f;
        public Color debugColorAveragedPoint = Color.yellow;

        GLFigure _figure;
        List<DebugMarker> _markers;

        static Planter _instance;

        public static Planter Instance { get { return _instance; } }

        void Start() {
            _instance = this;
            _figure = new GLFigure ();
            _markers = new List<DebugMarker> ();

			garden.InitTypeCount (plantfabs.Length);
        }
        void OnDestroy() {
            _figure.Dispose();
        }
    	void Update () {
            if (debugInputMode != DebugInputModeEnum.None) {
                if (Input.GetMouseButton (0)) {
                    var worldPos = WorldMousePos ();
                    AddCreationMarker (worldPos);

                }
                if (Input.GetMouseButton (1)) {
                    var worldPos = WorldMousePos ();
                    AddDestructionMarker (worldPos);
                }
            }

            foreach (var p in animator.DeadPlants())
                RemovePlant (p);
        }
		void OnRenderObject() {
            switch (debugVisualMode) {
            default:
                _markers.Clear ();
                return;
            case DebugVisualModeEnum.Marker:
                break;
            }

            if (((1 << gameObject.layer) & Camera.current.cullingMask) == 0)
                return;
            
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
        void OnDrawGizmos() {
            if (!Application.isPlaying)
                return;
            
            var worldPos = WorldMousePos ();
            var localPos = World2LocalPos (worldPos);
            Vector3 averaged;
            var total = garden.Average (out averaged, localPos, debugRadiusAveragedPoint);
            if (total > 0) {
                Gizmos.color = debugColorAveragedPoint;
                Gizmos.DrawLine (worldPos, Local2WorldPos (averaged));
            }
        }

        public void AddCreationMarker (Vector3 worldPos) {
            _markers.Add (new DebugMarker (worldPos, searchRadius, debugColorAdd));
            var localPos = PerturbedWorld2LocalPos (worldPos);
            var typeId = garden.Sample (localPos, searchRadius);
            if (typeId >= 0) {
                var p = Instantiate (typeId);
                p.transform.localPosition = localPos;
                AddPlant (p.GetComponent<Plant> ());
            }
        }
        public void AddDestructionMarker (Vector3 worldPos) {
            _markers.Add (new DebugMarker (worldPos, searchRadius, debugColorRemove));
            var localPos = PerturbedWorld2LocalPos (worldPos);
            foreach (var p in garden.Neighbors (localPos, searchRadius)) {
                animator.SetStencil (p, TIME_STENCIL_DIE);
            }
        }

        public virtual GameObject Instantiate(int typeId) {
            return Instantiate (plantfabs [typeId].gameObject);
        }


        Vector3 WorldMousePos() {
            var garden2camInWorld = garden.transform.position - garden.targetCamera.transform.position;
            var mousePos = Input.mousePosition;
            mousePos.z = Vector3.Dot (garden2camInWorld, garden.targetCamera.transform.forward);
            return garden.targetCamera.ScreenToWorldPoint (mousePos);
        }
        Vector3 World2LocalPos(Vector3 worldPlantPos) {
            var localPlantPos = garden.transform.InverseTransformPoint (worldPlantPos);
            localPlantPos.y = 0f;
            return localPlantPos;            
        }
        Vector3 Local2WorldPos(Vector3 localPlantPos) { return garden.transform.TransformPoint (localPlantPos); }
        Vector3 PerturbedWorld2LocalPos(Vector3 worldPlantPos) {
            worldPlantPos += perturbation * Random.insideUnitSphere;
            return World2LocalPos (worldPlantPos);
        }

        void AddPlant (Plant p) {
            garden.Add (p);
        }
        void RemovePlant (Plant plant) {
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
                this.size = 2f * radius * Vector2.one;
                this.color = color;
                this.time = Time.timeSinceLevelLoad;
            }
        }
    }
}