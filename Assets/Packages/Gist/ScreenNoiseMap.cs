using UnityEngine;
using System.Collections;

namespace Gist {
    [ExecuteInEditMode]
    public class ScreenNoiseMap : MonoBehaviour {
        public enum DebugModeEnum { None = 0, ShowNormal, ShowHeight }
        public const string KW_OUTPUT_NORMAL = "OUTPUT_NORMAL";
        public const string KW_OUTPUT_HEIGHT = "OUTPUT_HEIGHT";
        public const float SEED_SIZE = 100f;

        public DebugModeEnum debugMode;
        public Material debugMat;
        public TextureEvent OnCreateTexture;
        public Camera targetCam;
        public int lod = 2;
        public float fieldSize = 1f;
        public float noiseFreq = 1f;
        public float timeScale = 1f;

        Texture2D _noiseTex;
        float[] _heightValues;
        Vector3[] _normalValues;
        Color[] _noiseColors;
        Vector3 _seeds;
        int _width, _height;
        Vector2 _texelSize;

        void OnEnable() {
            _seeds = SEED_SIZE * new Vector3 (Random.value, Random.value, Random.value);
        }
    	void Update () {
            _width = targetCam.pixelWidth >> lod;
            _height = targetCam.pixelHeight >> lod;
            _texelSize.Set(1f / _width, 1f / _height);

            if (_noiseTex == null || _noiseTex.width != _width || _noiseTex.height != _height) {
                ReleaseTex();
                _noiseTex = new Texture2D (_width, _height, TextureFormat.ARGB32, false);
                _noiseTex.wrapMode = TextureWrapMode.Clamp;
                _noiseTex.filterMode = FilterMode.Bilinear;
                _noiseColors = _noiseTex.GetPixels ();
                _normalValues = new Vector3[_noiseColors.Length];
                _heightValues = new float[(_width + 1) * (_height + 1)];
                OnCreateTexture.Invoke (_noiseTex);
            }

            UpdateNoiseMap ();
    	}
        void OnRenderObject() {
            if (debugMode != DebugModeEnum.None) {
                if (debugMat != null) {
                    debugMat.shaderKeywords = null;
                    debugMat.EnableKeyword (debugMode == DebugModeEnum.ShowHeight ? KW_OUTPUT_HEIGHT : KW_OUTPUT_NORMAL);
                }
                GL.PushMatrix ();
                GL.LoadOrtho ();
                Graphics.DrawTexture (new Rect (0f, 0f, 1f, 1f), _noiseTex, debugMat);
                GL.PopMatrix ();
            }
        }
        void OnDisable() {
            ReleaseTex ();
        }

        public Vector3 GetNormalZ(float u, float v) {
            var c = _noiseTex.GetPixelBilinear (u, v);
            var n = new Vector3 (2f * c.r - 1f, 2f * c.g - 1f, 2f * c.b - 1f);
            return n.normalized;
        }
        public Vector3 GetNormalY(float u, float v) {
            var c = _noiseTex.GetPixelBilinear (u, v);
            var n = new Vector3 (2f * c.r - 1f, 2f * c.b - 1f, 2f * c.g - 1f);
            return n.normalized;
        }
        public float GetHeight(float u, float v) {
            var c = _noiseTex.GetPixelBilinear (u, v);
            return c.a;
        }

        void UpdateNoiseMap() {
            UpdateHeightMap ();
            UpdateNormalMap ();
            UpdateTexture ();

            _noiseTex.SetPixels (_noiseColors);
            _noiseTex.Apply (false);
        }

        void UpdateHeightMap () {
            var px = (float)noiseFreq / _height;
            var t = Time.timeSinceLevelLoad * timeScale + _seeds.z;
            Parallel.For (0, _height + 1, y =>  {
                for (var x = 0; x <= _width; x++) {
                    var i = x + y * (_width + 1);
                    _heightValues [i] = (float)SimplexNoise.Noise (px * (x - 0.5f + _seeds.x), px * (y - 0.5f + _seeds.y), t);
                }
            });
        }
        void UpdateNormalMap () {
            var idx = (float)_height / fieldSize;
            Parallel.For (0, _height, y =>  {
                for (var x = 0; x < _width; x++) {
                    var i = x + y * _width;
                    var j = x + y * (_width + 1);
                    var h = _heightValues[j];
                    var dhdx = (_heightValues [j + 1] - h) * idx;
                    var dhdy = (_heightValues [j + (_width + 1)] - h) * idx;
                    var n = new Vector3 (-dhdx, -dhdy, 1f).normalized;
                    _normalValues[i] = n;
                }
            });
        }
        void UpdateTexture() {
            Parallel.For (0, _height, y =>  {
                for (var x = 0; x < _width; x++) {
                    var i = x + y * _width;
                    var j = x + y * (_width + 1);
                    var h = _heightValues[j];
                    var n = _normalValues[i];
                    _noiseColors[i] = new Color(0.5f * (n.x + 1f), 0.5f * (n.y + 1f), 0.5f * (n.z + 1f), h);
                }
            });
        }

        void ReleaseTex () {
            DestroyImmediate(_noiseTex);
        }

        [System.Serializable]
        public class TextureEvent : UnityEngine.Events.UnityEvent<Texture> {}
    }
}