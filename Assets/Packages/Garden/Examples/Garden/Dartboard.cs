using UnityEngine;
using System.Collections;
using Gist;
using System.Collections.Generic;

namespace GardenSystem {

    [ExecuteInEditMode]
    public class Dartboard : MonoBehaviour {
        public enum DebugMarkerVisualEnum { Hide = 0, Draw }
        public enum BoundaryModeEnum { Clamped = 0, Free }

        public const float EPSILON = 1e-3f;

        public DebugMarkerVisualEnum debugMarkerVisual;

        public float debugMarkerLifetime = 1f;
        public Color debugMarkerColor = Color.green;
        public Color debugBoardColor = Color.red;

        GLFigure _fig;
        List<DebugMarker> _debugMarkers;

        void OnEnable() {
            _fig = new GLFigure ();
            _debugMarkers = new List<DebugMarker> ();
        }
        void OnDisable() {
            if (_fig != null) {
                _fig.Dispose ();
                _fig = null;
            }
        }
        void OnDrawGizmosSelected() {
            if (_fig == null)
                return;
            
            _fig.ZTestMode = GLFigure.ZTestEnum.LESSEQUAL;
            _fig.ZWriteMode = false;

            var scale = transform.localScale;
            _fig.FillQuad (transform.position, transform.rotation, new Vector2 (scale.x, scale.y), debugBoardColor);
        }
        void OnRenderObject() {
            var visibleLayer = (Camera.current.cullingMask & (1 << gameObject.layer)) != 0;
            if (debugMarkerVisual == DebugMarkerVisualEnum.Hide || !visibleLayer)
                return;

            var rot = transform.rotation;
            for (var i = 0; i < _debugMarkers.Count;) {
                var m = _debugMarkers [i];
                _fig.FillCircle (m.pos, rot, m.size, debugMarkerColor);
                if ((Time.timeSinceLevelLoad - m.time) > debugMarkerLifetime)
                    _debugMarkers.RemoveAt (i);
                else
                    i++;
            }
        }

        public bool World(Ray ray, out Vector3 worldPosition) {
            var localOrigin = transform.InverseTransformPoint (ray.origin);
            var localDirection = transform.InverseTransformVector (ray.direction);

            if (-EPSILON < localDirection.z && localDirection.z < EPSILON) {
                worldPosition = default(Vector3);
                return false;
            }

            var t = -localOrigin.z / localDirection.z;
            worldPosition = ray.GetPoint (t);
            _debugMarkers.Add (new DebugMarker (worldPosition, 2f * Vector2.one));
            return true;
        }

        public class DebugMarker {
            public Vector3 pos;
            public Vector2 size;
            public float time;

            public DebugMarker(Vector3 pos, Vector2 size) {
                this.pos = pos;
                this.size = size;
                this.time = Time.timeSinceLevelLoad;
            }
        }
    }
}
