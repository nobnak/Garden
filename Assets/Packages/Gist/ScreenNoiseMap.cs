using UnityEngine;
using System.Collections;

namespace Gist {

public class ScreenNoiseMap : MonoBehaviour {
        public enum TextureTypeEnum { HeightNormal = 0, Height, Normal }
        public enum DebugModeEnum { Normal = 0, Show }
        public const float SEED_SIZE = 100f;

        public TextureTypeEnum textureType;
        public DebugModeEnum debugMode;
        public TextureEvent OnCreateTexture;
        public Camera targetCam;
        public int lod = 2;
        public float fieldSize = 1f;
        public float noiseFreq = 1f;
        public float timeScale = 1f;

        Texture2D _noiseTex;
        float[] _heightColors;
        Color[] _noiseColors;
        Vector3 _seeds;

        void Awake() {
            _seeds = SEED_SIZE * new Vector3 (Random.value, Random.value, Random.value);
        }
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
                OnCreateTexture.Invoke (_noiseTex);
            }

            UpdateNoiseMap ();
    	}
        void OnRenderObject() {
            if (debugMode == DebugModeEnum.Show) {
                GL.PushMatrix ();
                GL.LoadOrtho ();
                Graphics.DrawTexture (new Rect (0f, 0f, 1f, 1f), _noiseTex);
                GL.PopMatrix ();
            }
        }
        void OnDestroy() {
            ReleaseTex ();
        }

        void UpdateNoiseMap() {
            var width = _noiseTex.width;
            var height = _noiseTex.height;
            var px = (float)noiseFreq / height;
            var t = Time.timeSinceLevelLoad * timeScale + _seeds.z;
            Parallel.For (0, height + 1, (y) => {
                for (var x = 0; x <= width; x++) {
                    var i = x + y * (width + 1);
                    _heightColors [i] = (float)SimplexNoise.Noise (px * (x - 0.5f + _seeds.x), px * (y - 0.5f + _seeds.y), t);
                }
            });

            switch (textureType) {
            case TextureTypeEnum.Height:
                GenerateHeightMap (width, height);
                break;
            case TextureTypeEnum.Normal:
                GenerateNormalMap (width, height);
                break;
            default:
                GenerateHeightNormalMap (width, height);
                break;
            }

            _noiseTex.SetPixels (_noiseColors);
            _noiseTex.Apply (false);
        }

        void GenerateHeightNormalMap (int width, int height) {
            var idx = (float)height / fieldSize;
            Parallel.For (0, height, y =>  {
                for (var x = 0; x < width; x++) {
                    var i = x + y * width;
                    var j = x + y * (width + 1);
                    var h = _heightColors[j];
                    var dhdx = (_heightColors [j + 1] - h) * idx;
                    var dhdy = (_heightColors [j + (width + 1)] - h) * idx;
                    var n = new Vector3 (-dhdx, -dhdy, 1f).normalized;
                    _noiseColors [i] = new Color (0.5f * (n.x + 1f), 0.5f * (n.y + 1f), 0.5f * (n.z + 1f), h);
                }
            });
        }
        void GenerateNormalMap (int width, int height) {
            var idx = (float)height / fieldSize;
            Parallel.For (0, height, y =>  {
                for (var x = 0; x < width; x++) {
                    var i = x + y * width;
                    var j = x + y * (width + 1);
                    var h = _heightColors[j];
                    var dhdx = (_heightColors [j + 1] - h) * idx;
                    var dhdy = (_heightColors [j + (width + 1)] - h) * idx;
                    var n = new Vector3 (-dhdx, -dhdy, 1f).normalized;
                    _noiseColors [i] = new Color (0.5f * (n.x + 1f), 0.5f * (n.y + 1f), 0.5f * (n.z + 1f), 1f);
                }
            });
        }
        void GenerateHeightMap (int width, int height) {
            Parallel.For (0, height, y =>  {
                for (var x = 0; x < width; x++) {
                    var i = x + y * width;
                    var j = x + y * (width + 1);
                    var h = _heightColors[j];
                    _noiseColors [i] = new Color (h, h, h, 1f);
                }
            });
        }

        void ReleaseTex () {
            Destroy (_noiseTex);
        }

        [System.Serializable]
        public class TextureEvent : UnityEngine.Events.UnityEvent<Texture> {}
    }
}