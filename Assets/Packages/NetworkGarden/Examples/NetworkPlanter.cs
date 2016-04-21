using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gist;
using GardenSystem;
using UnityEngine.Networking;

namespace NetworkGardenSystem {
    
    public class NetworkPlanter : NetworkBehaviour {
        public string nameTargetCam = "Main Camera";
        public Collider ground;

        Camera _targetCam;

        void Update() {
            if (isServer)
                UpdateServer();
        }

        void UpdateServer() {
            if (_targetCam == null)
                _targetCam = GameObject.Find (nameTargetCam).GetComponent<Camera>();

            if (ground != null) {
                if (Input.GetMouseButton (0)) {
                    Ray ray = _targetCam.ScreenPointToRay (Input.mousePosition);
                    RaycastHit hit;
                    if (ground.Raycast (ray, out hit, float.MaxValue)) {
                        RpcAddCreationMarker (hit.point);
                    }
                } else if (Input.GetMouseButton (1)) {
                    Ray ray = _targetCam.ScreenPointToRay (Input.mousePosition);
                    RaycastHit hit;
                    if (ground.Raycast (ray, out hit, float.MaxValue))
                        RpcAddDestructionMarker (hit.point);
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