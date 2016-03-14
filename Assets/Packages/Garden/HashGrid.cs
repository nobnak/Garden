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

        #region IDisposable implementation
        public void Dispose () {
            _points.Clear();
            _cellIds.Clear();
        }
        #endregion
    }
}