using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using GardenSystem;

namespace NetworkGardenSystem {
	public class Client : MonoBehaviour {
        public InstanceEvent OnInstantiateGarden;

        public Garden gardenfab;

		void Start () {
            var targetCam = Camera.main;
            var garden = Instantiate (gardenfab.gameObject).GetComponent<Garden>();
            garden.SetCamera (targetCam);
            garden.transform.SetParent (transform, false);
            OnInstantiateGarden.Invoke (garden.gameObject);
		}		
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

    [System.Serializable]
    public class InstanceEvent : UnityEngine.Events.UnityEvent<GameObject> {}
}