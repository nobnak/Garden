using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gist;

namespace GardenSystem {

	public class Planter : MonoBehaviour {
        public const string TAG_MAIN_CAMERA = "MainCamera";
		public const int TIME_STENCIL_BIRTH = 0;
		public const int TIME_STENCIL_DIE = 1;
        public const float ROUND_IN_DEG = 360f;

        public Garden garden;
        public TimeAnimator animator;
		public Plant[] plantfabs;
        public Data data;
        public DebugData debugData;

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
            if (debugData.debugInputMode != DebugData.DebugInputModeEnum.None) {
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
            switch (debugData.debugVisualMode) {
            default:
                _markers.Clear ();
                return;
            case DebugData.DebugVisualModeEnum.Marker:
                break;
            }

            if (((1 << gameObject.layer) & Camera.current.cullingMask) == 0)
                return;
            
            _figure.ZTestMode = GLFigure.ZTestEnum.ALWAYS;
            _figure.ZWriteMode = false;

            var timeLimit = Time.timeSinceLevelLoad - debugData.debugHoldTime;
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
            var total = garden.Average (out averaged, localPos, debugData.debugRadiusAveragedPoint);
            if (total > 0) {
                Gizmos.color = debugData.debugColorAveragedPoint;
                Gizmos.DrawLine (worldPos, Local2WorldPos (averaged));
            }
        }

        public void AddCreationMarker (Vector3 worldPos) {
            _markers.Add (new DebugMarker (worldPos, data.searchRadius, debugData.debugColorAdd));
            var localPos = PerturbedWorld2LocalPos (worldPos);
            int neighborCount;
            var typeId = garden.Sample (localPos, data.searchRadius, out neighborCount);
            if (typeId >= 0 && neighborCount < data.limitNeighborCount) {
                var p = Instantiate (typeId);
                p.transform.localPosition = localPos;
                AddPlant (p.GetComponent<Plant> ());
            }
        }
        public void AddDestructionMarker (Vector3 worldPos) {
            _markers.Add (new DebugMarker (worldPos, data.searchRadius, debugData.debugColorRemove));
            var localPos = PerturbedWorld2LocalPos (worldPos);
            foreach (var p in garden.Neighbors (localPos, data.searchRadius)) {
                animator.SetStencil (p, TIME_STENCIL_DIE);
            }
        }
        public virtual GameObject Instantiate(int typeId) {
            return Instantiate (plantfabs [typeId].gameObject);
        }
        public bool PositionVisibility(Vector3 worldPos) {
            var viewportPos = garden.targetCamera.WorldToViewportPoint (worldPos);
            var viewport = garden.targetCamera.rect;
            return viewport.xMin < viewportPos.x && viewportPos.x < viewport.xMax &&
                viewport.yMin < viewportPos.y && viewportPos.y < viewport.yMax;
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
            worldPlantPos += data.perturbation * data.searchRadius * Random.insideUnitSphere;
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

        [System.Serializable]
        public class Data {
            public float perturbation = 1.5f;
            public float searchRadius = 1f;
            public int limitNeighborCount = 40;
        }

        [System.Serializable]
        public class DebugData {
            public enum DebugInputModeEnum { None = 0, Mouse }
            public enum DebugVisualModeEnum { None = 0, Marker }

            public DebugInputModeEnum debugInputMode;
            public DebugVisualModeEnum debugVisualMode;
            public float debugHoldTime = 0.1f;
            public Color debugColorAdd = new Color (0f, 1f, 0f, 0.5f);
            public Color debugColorRemove = new Color(1f, 0f, 0f, 0.5f);
            public float debugRadiusAveragedPoint = 2f;
            public Color debugColorAveragedPoint = Color.yellow;
        }
    }
}