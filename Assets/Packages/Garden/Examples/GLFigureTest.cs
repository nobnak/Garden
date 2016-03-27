using UnityEngine;
using System.Collections;
using Gist;

namespace Garden {

	public class GLFigureTest : MonoBehaviour {
		public Transform target;
		public Vector2 size;
		public Color bodyColor = Color.green;
		public Color lineColor = Color.white;

		GLFigure _figure;

		void Awake() {
			_figure = new GLFigure ();
		}
		void OnDestroy() {
			_figure.Dispose ();
		}
		void OnRenderObject() {
			_figure.FillCircle (target.position, Camera.main.transform.rotation, size, bodyColor);
			_figure.DrawCircle (target.position, Camera.main.transform.rotation, size, lineColor);
		}
	}
}