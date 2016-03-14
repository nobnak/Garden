using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GardenSystem {
    
    public class HashGrid<T> : System.IDisposable {
        List<T> _points;
        List<int> _cellIds;
        System.Func<T, Vector3> _GetPosition;

        public HashGrid(System.Func<T, Vector3> GetPosition) {
            _points = new List<T> ();
            _cellIds = new List<int> ();
            _GetPosition = GetPosition;
        }

        public void Add(T point) {
            _points.Add (point);
        }
        public void Remove(T point) {
            var index = _points.IndexOf (point);
            _points.RemoveAt (index);
        }
        public T Find(System.Predicate<T> Predicate) {
            return _points.Find (Predicate);
        }
        public IEnumerable<T> Neighbors(Vector3 center, float distance) {
            var r2 = distance * distance;
            foreach (var p in _points) {
                var d2 = (_GetPosition(p) - center).sqrMagnitude;
                if (d2 < r2)
                    yield return p;
            }
        }
		public void Build(float cellSize, int x, int y, int z) {
			
		}

        #region IDisposable implementation
        public void Dispose () {
            _points.Clear();
            _cellIds.Clear();
        }
        #endregion

		public class Hash {
			public readonly Vector3 gridSize;
			public readonly float cellSize;
			public readonly int nx, ny, nz;

			public Hash(float cellSize, int nx, int ny, int nz) {
				this.cellSize = cellSize;
				this.nx = nx;
				this.ny = ny;
				this.nz = nz;
				this.gridSize = new Vector3(nx * cellSize, ny * cellSize, nz * cellSize);
			}
			public IEnumerable<int> CellIds(Vector3 position, float radius) {
				var minx = CellX (position.x - radius);
				var miny = CellY (position.y - radius);
				var minz = CellZ (position.z - radius);
				var maxx = CellX (position.x + radius);
				var maxy = CellY (position.y + radius);
				var maxz = CellZ (position.z + radius);
				if (maxx < minx)
					maxx += nx;
				if (maxy < miny)
					maxy += ny;
				if (maxz < minz)
					maxz += nz;
				maxx = Mathf.Min (maxx, minx + nx - 1);
				maxy = Mathf.Min (maxy, miny + ny - 1);
				maxz = Mathf.Min (maxz, minz + nz - 1);
				for (var z = minz; z <= maxz; z++)
					for (var y = miny; y <= maxy; y++)
						for (var x = minx; x <= maxx; x++)
							yield return CellId (x, y, z);
			}
			public int CellId(Vector3 position) {
				return CellId (CellX (position.x), CellY (position.y), CellZ (position.z));
			}
			public int CellId(int x, int y, int z) {
				x = x < 0 ? 0 : (x >= nx ? nx - 1 : x);
				y = y < 0 ? 0 : (y >= ny ? ny - 1 : y);
				z = z < 0 ? 0 : (z >= nz ? nz - 1 : z);
				return x + (y + z * ny) * nx;
			}
			public int CellX(float posX) {
				posX -= gridSize.x * Mathf.CeilToInt (posX / gridSize.x);
				return (int)(posX / cellSize);
			}
			public int CellY(float posY) {
				posY -= gridSize.y * Mathf.CeilToInt (posY / gridSize.y);
				return (int)(posY / cellSize);
			}
			public int CellZ(float posZ) {
				posZ -= gridSize.z * Mathf.CeilToInt (posZ / gridSize.z);
				return (int)(posZ / cellSize);
			}
		}
    }
}