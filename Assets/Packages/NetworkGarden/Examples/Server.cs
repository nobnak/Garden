using UnityEngine;
using System.Collections;
using GardenSystem;

namespace NetworkGardenSystem {
	
	public class Server : MonoBehaviour {

		void Start () {

		}
		
		void Update () {
		
		}

		void OnGUI() {
			GUILayout.BeginVertical ();

			GUILayout.Label ("This is Server");

			GUILayout.EndVertical ();
		}
	}
}