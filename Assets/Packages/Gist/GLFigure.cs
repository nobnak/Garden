using UnityEngine;
using System.Collections;

namespace Gist {
	public class GLFigure : System.IDisposable {
		public const float TWO_PI_RAD = 2f * Mathf.PI;
		public const int SEGMENTS = 36;
		public const string LINE_SHADER = "Hidden/Internal-Colored";

		Material _lineMat;

		public GLFigure() {
			var lineShader = Shader.Find (LINE_SHADER);
			if (lineShader == null)
				Debug.LogErrorFormat ("Line Shader not found : {0}", LINE_SHADER);
			_lineMat = new Material (lineShader);
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