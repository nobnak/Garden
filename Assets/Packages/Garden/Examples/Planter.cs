using UnityEngine;
using System.Collections;

namespace GardenSystem {

    public class Planter : MonoBehaviour {
        public Garden garden;

    	void Update () {
            if (Input.GetMouseButton (0)) {
                GameObject plant;
                garden.Add (garden.targetCamera.ScreenToViewportPoint (Input.mousePosition), out plant);
            }
    	}
    }
}