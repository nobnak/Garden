using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gist;
using GardenSystem;
using UnityEngine.Networking;

namespace NetworkGardenSystem {
    
    public class NetworkPlanter : NetworkBehaviour {
        public string nameTargetCam = "Main Camera";
        public Dartboard dartboard;

        Camera _targetCam;

        void Update() {
            if (isServer)
                UpdateServer();
        }

        void UpdateServer() {
            if (_targetCam == null)
                _targetCam = GameObject.Find (nameTargetCam).GetComponent<Camera>();

            var mouseFlags = (Input.GetMouseButton (0) ? 1 : 0)
                | (Input.GetMouseButton (1) ? 2 : 0);
            if (dartboard != null && mouseFlags != 0) {
                Ray ray = _targetCam.ScreenPointToRay (Input.mousePosition);
                Vector3 worldPosition;
                if (dartboard.World (ray, out worldPosition)) {
                    if ((mouseFlags & 1) != 0)
                        RpcAddCreationMarker (worldPosition);
                    else
                        RpcAddDestructionMarker (worldPosition);
                }
            }
        }

        [ClientRpc]
        void RpcAddCreationMarker(Vector3 worldPoint) {
            Planter.Instance.AddCreationMarker (worldPoint);
        }
        [ClientRpc]
        void RpcAddDestructionMarker (Vector3 worldPoint) {
            Planter.Instance.AddDestructionMarker (worldPoint);
        }
    }
}