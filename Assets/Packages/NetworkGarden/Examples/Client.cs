using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace NetworkGardenSystem {
	public class Client : MonoBehaviour {

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}
		void OnGUI() {
			if (NetworkServer.active)
				return;

			GUILayout.BeginVertical ();

			GUILayout.Label ("This is Client");

			GUILayout.EndVertical ();
		}
	}
}