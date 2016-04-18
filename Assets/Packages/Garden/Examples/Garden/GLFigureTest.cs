using UnityEngine;
using System.Collections;
using Gist;

namespace GardenSystem {

	public class GLFigureTest : MonoBehaviour {
        public Data[] datas;

		GLFigure _figure;

		void Awake() {
			_figure = new GLFigure ();
		}
		void OnDestroy() {
			_figure.Dispose ();
		}
		void OnRenderObject() {
            foreach (var d in datas) {
                d.Draw (_figure);
            }
		}

        [System.Serializable]
        public class Data {
            [System.Flags]
            public enum RenderTypeEnum { None = 0, Line = (1 << 0), Fill = (1 << 1), Whole = Line | Fill }
            public enum ZModeEnum { LEqual = 0, Overlay }
            public enum FigureTypeEnum { Circle = 0, Quad }

            public RenderTypeEnum renderType;
            public ZModeEnum ztest;
            public FigureTypeEnum figureType;

            public Transform target;
            public Vector2 size;
            public Color bodyColor;
            public Color lineColor;

            public void ApplyZMode(GLFigure f) {
                switch (ztest) {
                case ZModeEnum.LEqual:
                    f.ZTestMode = GLFigure.ZTestEnum.LESSEQUAL;
                    f.ZWriteMode = true;
                    break;
                case ZModeEnum.Overlay:
                    f.ZTestMode = GLFigure.ZTestEnum.ALWAYS;
                    f.ZWriteMode = false;
                    break;
                }
            }
            public void Draw(GLFigure f) {
                ApplyZMode (f);

                switch (figureType) {
                case FigureTypeEnum.Circle:
                    DrawCircle (f);
                    break;
                case FigureTypeEnum.Quad:
                    DrawQuad (f);
                    break;
                }
            }
            public void DrawCircle(GLFigure f) {
                if ((renderType & Data.RenderTypeEnum.Fill) != 0) {
                    f.FillCircle (target.position, Camera.main.transform.rotation, size, bodyColor);
                }
                if ((renderType & Data.RenderTypeEnum.Line) != 0) {
                    f.DrawCircle (target.position, Camera.main.transform.rotation, size, lineColor);
                }                
            }public void DrawQuad(GLFigure f) {
                if ((renderType & Data.RenderTypeEnum.Fill) != 0) {
                    f.FillQuad(target.position, Camera.main.transform.rotation, size, bodyColor);
                }
                if ((renderType & Data.RenderTypeEnum.Line) != 0) {
                    f.DrawQuad(target.position, Camera.main.transform.rotation, size, lineColor);
                }                

            }

        }
	}
}