using UnityEngine;
using System.Collections;

namespace Gist {
	public class GLFigure : System.IDisposable {
        public const string PROP_COLOR = "_Color";
        public const string PROP_SRC_BLEND = "_SrcBlend";
        public const string PROP_DST_BLEND = "_DstBlend";
        public const string PROP_ZWRITE = "_ZWrite";
        public const string PROP_ZTEST = "_ZTest";
        public const string PROP_CULL = "_Cull";
        public const string PROP_ZBIAS = "_ZBias";

		public const float TWO_PI_RAD = 2f * Mathf.PI;
		public const int SEGMENTS = 36;
		public const string LINE_SHADER = "Hidden/Internal-Colored";
        
        public enum ZTestEnum { NEVER = 1, LESS = 2, EQUAL = 3, LESSEQUAL = 4,
            GREATER = 5, NOTEQUAL = 6, GREATEREQUAL = 7, ALWAYS = 8 };

		Material _lineMat;

		public GLFigure() {
			var lineShader = Shader.Find (LINE_SHADER);
			if (lineShader == null)
				Debug.LogErrorFormat ("Line Shader not found : {0}", LINE_SHADER);
			_lineMat = new Material (lineShader);
		}

        public bool ZWriteMode {
            get { return _lineMat.GetInt (PROP_ZWRITE) == 1; }
            set { _lineMat.SetInt (PROP_ZWRITE, value ? 1 : 0); }
        }
        public ZTestEnum ZTestMode {
            get { return (ZTestEnum)_lineMat.GetInt (PROP_ZTEST); }
            set { _lineMat.SetInt (PROP_ZTEST, (int)value); }
        }

		public void DrawCircle(Vector3 center, Quaternion look, Vector2 size, Color color) {
			var scale = new Vector3 (size.x, size.y, 1f);
			var modelMat = Matrix4x4.TRS (center, look, scale);
			var cameraMat = Camera.current.worldToCameraMatrix;
			DrawCircle (cameraMat * modelMat, color);
		}
		public void FillCircle(Vector3 center, Quaternion look, Vector2 size, Color color) {
			var scale = new Vector3 (size.x, size.y, 1f);
			var modelMat = Matrix4x4.TRS (center, look, scale);
			var cameraMat = Camera.current.worldToCameraMatrix;
			FillCircle (cameraMat * modelMat, color);
		}

		public void DrawCircle (Matrix4x4 modelViewMat, Color color) {
			_lineMat.SetPass (0);
			GL.PushMatrix ();
			GL.LoadIdentity ();
			GL.MultMatrix (modelViewMat);
			var dr = TWO_PI_RAD / SEGMENTS;
			GL.Begin (GL.LINES);
			GL.Color (color);
			var v = new Vector3 (1f, 0f, 0f);
			for (var i = 0; i <= SEGMENTS; i++) {
				GL.Vertex (v);
				v.Set(Mathf.Cos (i * dr), Mathf.Sin (i * dr), 0f);
				GL.Vertex (v);
			}
			GL.End ();
			GL.PopMatrix ();
		}
		public void FillCircle(Matrix4x4 modelViewMat, Color color) {
			_lineMat.SetPass (0);
			GL.PushMatrix ();
			GL.LoadIdentity ();
			GL.MultMatrix (modelViewMat);
			var dr = TWO_PI_RAD / SEGMENTS;
			GL.Begin (GL.TRIANGLES);
			GL.Color (color);
			var v = new Vector3 (1f, 0f, 0f);
			for (var i = 0; i < SEGMENTS; i++) {
				GL.Vertex (v);
				GL.Vertex (Vector3.zero);
				v.Set (Mathf.Cos ((i + 1) * dr), Mathf.Sin ((i + 1) * dr), 0f);
				GL.Vertex (v);
			}
			GL.End ();
			GL.PopMatrix ();
		}

		#region IDisposable implementation
		public void Dispose () {
			GameObject.Destroy (_lineMat);
		}
		#endregion
	}
}