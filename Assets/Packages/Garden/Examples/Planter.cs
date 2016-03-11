using UnityEngine;
using System.Collections;

namespace GardenSystem {

    public class Planter : MonoBehaviour {
        public Garden garden;
        public float searchRadius = 1f;

    	void Update () {
            if (Input.GetMouseButton (0)) {
                GameObject plant;
                garden.Add (GetUv(), out plant);
            }
            if (Input.GetMouseButton (1)) {
                Garden.PlantData plant = null;
                float sqrDist = float.MaxValue;
                var uv = GetUv ();
                var screenPos = GetScreenPos (uv);
                Debug.LogFormat("Sceen pos {0}", screenPos);
                foreach (var p in garden.Neighbors(uv, searchRadius)) {
                    var d = GetScreenPos (p.screenUv) - screenPos;
                    if (d.sqrMagnitude < sqrDist) {
                        sqrDist = d.sqrMagnitude;
                        plant = p;
                    }
                }

                if (plant != null) {
                    Debug.Log ("Remove nearest plant");
                    garden.Remove (plant.obj);
                }
            }
    	}

        Vector2 GetUv() {
            return garden.targetCamera.ScreenToViewportPoint (Input.mousePosition);
        }
        Vector2 GetScreenPos(Vector2 uv) {
            return garden.targetCamera.ViewportToScreenPoint (uv);
        }
    }
}