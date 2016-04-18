using UnityEngine;
using System.Collections;
using GardenSystem;

namespace NetworkGardenSystem {
	
	public class Server : MonoBehaviour {
		public Garden gardenfab;

		Garden _garden;

		void Start () {
			var mainCam = Camera.main;

			_garden = Instantiate (gardenfab);
			_garden.transform.SetParent (transform, false);
			_garden.SetCamera (mainCam);

		}
		
		// Update is called once per frame
		void Update () {
		
		}

		void OnGUI() {
			GUILayout.BeginVertical ();

			GUILayout.Label ("This is Server");

			GUILayout.EndVertical ();
		}
	}
}