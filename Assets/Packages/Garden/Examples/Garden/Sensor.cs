using UnityEngine;
using System.Collections;
using OSC;
using System.Collections.Generic;
using Gist;

namespace GardenSystem {
        
    public class Sensor : MonoBehaviour {
        public const string OSC_PATH = "/point";

        public Camera targetCamera;
        public Planter planter;
        public Dartboard dartboard;

        public Data data;

        GLFigure _fig;

        void Update() {
            var mouseButtonFlags = (Input.GetMouseButton (0) ? 1 : 0) 
                | (Input.GetMouseButton (1) ? 2 : 0);
            
            if (mouseButtonFlags != 0) {
                var uv = targetCamera.ScreenToViewportPoint (Input.mousePosition);
                Vector3 worldPos;
                if (Contact(uv, out worldPos)) {
                    if ((mouseButtonFlags & 1) != 0)
                        planter.AddCreationMarker (worldPos);
                    else
                        planter.AddDestructionMarker (worldPos);
                }
            }
        }

        public void OnReceive(OscPort.Capsule c) {
            if (c.message.path != OSC_PATH)
                return;

            var p = SensorInput.Parse (c.message);
            Vector3 worldPos;
            if (Contact (p.center, out worldPos)) {
                if (p.center.z > 0.5f)
                    planter.AddCreationMarker (worldPos);
                else
                    planter.AddDestructionMarker (worldPos);
            }
        }
        public void OnError(System.Exception e) {
            Debug.LogFormat ("Exception {0}", e);
        }


        bool Contact(Vector3 uv, out Vector3 worldPos) {
            var ray = targetCamera.ViewportPointToRay(uv);
            return dartboard.World (ray, out worldPos);
        }

        public struct SensorInput {
            public Vector3 center;
            public Vector2 size;

            public SensorInput(Vector3 center, Vector2 size) {
                this.center = center;
                this.size = size;
            }

            public static SensorInput Parse(Osc.Message m) {
                var x = (float)m.data [0];
                var y = (float)m.data [1];
                var z = (float)m.data [2];
                var w = (float)m.data [3];
                var h = (float)m.data [4];
                return new SensorInput (new Vector3 (x, y, z), new Vector2 (w, h));
            }
        }
        [System.Serializable]
        public class Data {
            public int limitInput = 10;
        }
    }
}
