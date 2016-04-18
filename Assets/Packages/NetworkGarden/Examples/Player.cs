using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace NetworkGardenSystem {

	public class Player : NetworkBehaviour {
		public GameObject clientfab;
		public GameObject serverfab;

		public override void OnStartServer () {
			var server = Instantiate (serverfab);
			server.transform.SetParent (transform, false);
		}
		public override void OnStartLocalPlayer () {
			var client = Instantiate (clientfab);
			client.transform.SetParent (transform, false);
		}
	}
}