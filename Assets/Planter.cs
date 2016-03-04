using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;

public class Planter : MonoBehaviour {
    public FabData[] plantfabs;
	public Collider groundCollider;
	public float plantRange = 1f;
    public float neighborRange = 2f;
	public float frequency = 10f;
    public Vector2 interference = new Vector2 (-0.2f, 1.6f);

	float _time;
	float _lastHitTime;
	Vector3 _lastHitPoint;
    List<InstanceData> _plants;
    List<InstanceData> _tmpNeighbors;
    int[] _tmpCounters;
    float[] _tmpWeights;
    float _weightMax;

	void OnEnable() {
		_time = 0f;
		_lastHitTime = float.MinValue;
        _plants = new List<InstanceData> ();
        _tmpNeighbors = new List<InstanceData> ();
        _tmpCounters = new int[plantfabs.Length];
        _tmpWeights = new float[plantfabs.Length];
        _weightMax = FindMaxWeight ();
	}
	void Update () {
		if (Input.GetMouseButton (0)) {
			RaycastHit hit;
			var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (groundCollider.Raycast (ray, out hit, float.MaxValue)) {
				_lastHitTime = Time.timeSinceLevelLoad;
				_lastHitPoint = hit.point;

                FindNeighbors (_tmpNeighbors, hit.point, neighborRange);
                UpdateWeights (_tmpNeighbors);
                _weightMax = Sampling.RouletteWheelSelection.MaxWeight (_tmpWeights);
                Debug.Log(StatFrequency (_tmpNeighbors));

                foreach (var p in PlantSequence()) {
                    var typeId = Sampling.RouletteWheelSelection.Sample (_weightMax, _tmpWeights);
                    var fabdata = plantfabs [typeId];
                    var plant = Instantiate (fabdata.fab);
                    var posOffset = plantRange * Random.insideUnitSphere;

                    posOffset -= Vector3.Dot(posOffset, hit.normal) * hit.normal;
					plant.transform.SetParent (transform, false);
                    plant.transform.position = hit.point + posOffset;
                    _plants.Add (new InstanceData (){ typeId = typeId, obj = plant });
				}
			}
		}
	}
	void OnDrawGizmos() {
		if (!Application.isPlaying)
			return; 
		
		if ((Time.timeSinceLevelLoad - _lastHitTime) < 3f) {
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere (_lastHitPoint, plantRange);
		}
	}

    IEnumerable PlantSequence() {
        if (frequency <= 0f)
            yield break;

        _time += Time.deltaTime;

        var interval = 1f / frequency;
        var plantsCount = (int)(_time / interval);
        _time = Mathf.Max (_time - plantsCount * interval, 0f);
        for(var i = 0; i < plantsCount; i++)
            yield return null;
    }
    void FindNeighbors(List<InstanceData> neighbors, Vector3 center, float radius) {
        neighbors.Clear ();
        foreach (var p in _plants) {
            var d = p.obj.transform.position - center;
            if (d.sqrMagnitude < (radius * radius))
                neighbors.Add (p);
        }
    }
    string StatFrequency(List<InstanceData> neighbors) {
        var buf = new StringBuilder ();
        for (var i = 0; i < _tmpWeights.Length; i++)
            buf.AppendFormat ("w={0:f2} type={1}, ", _tmpWeights [i], i);
        return buf.ToString ();
    }
    void UpdateWeights(List<InstanceData> neighbors) {
        System.Array.Clear (_tmpCounters, 0, _tmpCounters.Length);
        foreach (var n in neighbors)
            _tmpCounters [n.typeId]++;
        
        var sum = neighbors.Count;
        for (var i = 0; i < _tmpCounters.Length; i++)
            _tmpWeights[i] = Interfered(sum - _tmpCounters [i],  sum);
    }
    float FindMaxWeight() {
        return Sampling.RouletteWheelSelection.MaxWeight (plantfabs, FabData.GetValue);
    }
    float Interfered(int aliens, int total) {
        if (aliens <= 0 || total <= 0)
            return 1f;
        var rate = aliens / (float)total;
        var t = (rate - interference.x) / (interference.y - interference.x);
        return Mathf.SmoothStep (1f, 0f, t);
    }

    [System.Serializable]
    public class FabData {
        public float weight;
        public GameObject fab;

        public static float GetValue(FabData self) { return self.weight; }
    }
    public class InstanceData {
        public int typeId;
        public GameObject obj;
    }
}
