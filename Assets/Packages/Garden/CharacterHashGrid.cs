using UnityEngine;
using System.Collections;
using Gist;

namespace GardenSystem {

    public class CharacterHashGrid : MonoBehaviour {
        public enum UpdateModeEnum { Update = 0, Rebuild }

        public UpdateModeEnum updateMode;
        public float cellSize = 1f;
        public int nx = 20;
        public int ny = 20;
        public int nz = 20;
        public Color gizmoColor = Color.white;

        public static HashGrid<MonoBehaviour> World;

        HashGrid<MonoBehaviour> _world;

        void Awake() {
            _world = new HashGrid<MonoBehaviour> (GetPosition, cellSize, nx, ny, nz);
            World = _world;
        }
        void LateUpdate() {
            switch (updateMode) {
            default:
                _world.Update ();
                break;
            case UpdateModeEnum.Rebuild:
                _world.Rebuild (cellSize, nx, ny, nz);
                break;
            }
        }
        void OnDrawGizmosSelected() {
            if (_world == null)
                return;
            
            var center = new Vector3 (0.5f * nx * cellSize, 0.5f * ny * cellSize, 0.5f * nz * cellSize);
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube (center, 2f * center);

            var size = cellSize * Vector3.one;
            var stat = _world.Stat ();
            for (var z = 0; z < stat.GetLength (2); z++) {
                for (var y = 0; y < stat.GetLength (1); y++) {
                    for (var x = 0; x < stat.GetLength (0); x++) {
                        var count = stat [x, y, z];
                        if (count >= 1) {
                            var pos = new Vector3 ((x + 0.5f) * cellSize, (y + 0.5f) * cellSize, (z + 0.5f) * cellSize);
                            var h = (float)count / 100;
                            h = Mathf.Clamp01(0.6f * (-h + 1));
                            Gizmos.color = Color.HSVToRGB (h, 1f, 1f);
                            Gizmos.DrawWireCube (pos, size);
                        }

                    }
                }
            }                        
        }

        Vector3 GetPosition(MonoBehaviour m) {
            return m.transform.position;
        }
    }
}