using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gist;
using GardenSystem;
using UnityEngine.Networking;

namespace NetworkGardenSystem {
    public class NetworkPlanter : NetworkBehaviour {
        public Camera targetCam;

        Collider _collider;
        Planter _planter;

        void Update() {
            if (isServer)
                UpdateServer();
        }

        void UpdateServer() {
            if (_collider == null)
                _collider = GetComponent<Collider> ();
            if (_planter == null)
                _planter = FindObjectOfType<Planter> ();

            if (_collider != null && _planter != null) {
                if (Input.GetMouseButton (0)) {
                    Ray ray = targetCam.ScreenPointToRay (Input.mousePosition);
                    RaycastHit hit;
                    if (_collider.Raycast(ray, out hit, float.MaxValue)) {
                        RpcAddCreationMarker (hit.point);
                    }
                }
            }
        }

        [ClientRpc]
        void RpcAddCreationMarker(Vector3 worldPoint) {
            Debug.LogFormat ("Add Creation Marker");
            _planter.AddCreationMarker (worldPoint);
        }
        [ClientRpc]
        void RpcAddDestructionMarker (Vector3 worldPoint) {
            Debug.LogFormat ("Add Destruction Marker");
            _planter.AddDestructionMarker (worldPoint);
        }
    }
}