using UnityEngine;
using System.Collections;
using Gist;

public class ScreenNoiseMap : MonoBehaviour {
    public Camera targetCam;
    public int lod = 2;
    public float fieldSize = 1f;
    public float noiseFreq = 1f;
    public float timeScale = 1f;

    Texture2D _noiseTex;
    float[] _heightColors;
    Color[] _noiseColors;

	void Update () {
        var width = targetCam.pixelWidth >> lod;
        var height = targetCam.pixelHeight >> lod;
        if (_noiseTex == null || _noiseTex.width != width || _noiseTex.height != height) {
            ReleaseTex();
            _noiseTex = new Texture2D (width, height, TextureFormat.ARGB32, false);
            _noiseTex.wrapMode = TextureWrapMode.Clamp;
            _noiseTex.filterMode = FilterMode.Bilinear;
            _noiseColors = _noiseTex.GetPixels ();
            _heightColors = new float[(width + 1) * (height + 1)];
        }

        UpdateNoiseMap ();
	}
    void OnRenderObject() {
        GL.PushMatrix ();
        GL.LoadOrtho ();
        Graphics.DrawTexture (new Rect (0f, 0f, 1f, 1f), _noiseTex);
        GL.PopMatrix ();
    }
    void OnDestroy() {
        ReleaseTex ();
    }

    void UpdateNoiseMap() {
        var width = _noiseTex.width;
        var height = _noiseTex.height;
        var px = (float)noiseFreq / height;
        var t = Time.timeSinceLevelLoad * timeScale;
        Parallel.For (0, height + 1, (y) => {
            for (var x = 0; x <= width; x++) {
                var i = x + y * (width + 1);
                _heightColors [i] = (float)SimplexNoise.Noise (px * (x - 0.5f), px * (y - 0.5f), t);
            }
        });

        var idx = (float)height / fieldSize;
        Parallel.For (0, height, (y) => {
            for (var x = 0; x < width; x++) {
                var i = x + y * width;
                var j = x + y * (width + 1);
                var dhdx = (_heightColors [j + 1] - _heightColors [j]) * idx;
                var dhdy = (_heightColors [j + (width + 1)] - _heightColors [j]) * idx;
                var n = new Vector3 (-dhdx, -dhdy, 1f).normalized;
                _noiseColors [i] = new Color (0.5f * (n.x + 1f), 0.5f * (n.y + 1f), 0.5f * (n.z + 1f), 1f);
            }
        });

        _noiseTex.SetPixels (_noiseColors);
        _noiseTex.Apply (false);
    }
    void ReleaseTex () {
        Destroy (_noiseTex);
    }
}
