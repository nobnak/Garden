using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gist;
using GardenSystem;
using UnityEngine.Networking;

namespace NetworkGardenSystem {
    
    public class NetworkPlanter : NetworkBehaviour {
        [System.Flags]
        public enum MarkerTypeEnum { Nil = 0, Creation = 1 << 0, Destruction = 1 << 1 }

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

            var mouseFlags = (Input.GetMouseButton (0) ? MarkerTypeEnum.Creation : MarkerTypeEnum.Nil)
                             | (Input.GetMouseButton (1) ? MarkerTypeEnum.Destruction : MarkerTypeEnum.Nil);
            if (dartboard != null && mouseFlags != 0) {
                var uv = _targetCam.ScreenToViewportPoint (Input.mousePosition);
                MarkInNormalziedCoord (uv, mouseFlags);
            }
        }

        public void MarkInNormalziedCoord(Vector2 uv, MarkerTypeEnum markerType) {
            var viewport = _targetCam.rect;
            var posInViewport = new Vector2 (uv.x * viewport.width + viewport.x, uv.y * viewport.height + viewport.y);
            var ray = _targetCam.ViewportPointToRay (posInViewport);
            Vector3 worldPosition;
            if (dartboard.World (ray, out worldPosition)) {
                if ((markerType & MarkerTypeEnum.Creation) != 0)
                    RpcAddCreationMarker (worldPosition);
                else
                    RpcAddDestructionMarker (worldPosition);
            }
        }

        [ClientRpc]
        void RpcAddCreationMarker(Vector3 worldPoint) {
            if (Planter.Instance != null)
                Planter.Instance.AddCreationMarker (worldPoint);
        }
        [ClientRpc]
        void RpcAddDestructionMarker (Vector3 worldPoint) {
            if (Planter.Instance != null)
                Planter.Instance.AddDestructionMarker (worldPoint);
        }
    }
}