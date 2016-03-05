using UnityEngine;
using System.Collections;

namespace GardenSystem {

    public class Planter : MonoBehaviour {
        public Garden garden;

    	void Update () {
            if (Input.GetMouseButton (0))
                garden.Reproduce (garden.targetCamera.ScreenToViewportPoint (Input.mousePosition));
    	}
    }
}