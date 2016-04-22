using UnityEngine;
using System.Collections;

namespace Gist {

    public class MaterialPropertyBlockChanied {
    	public readonly Renderer Renderer;
    	public readonly MaterialPropertyBlock Block;

        bool _session;

    	public MaterialPropertyBlockChanied(Renderer rend) : this(rend, new MaterialPropertyBlock()) {}
    	public MaterialPropertyBlockChanied(Renderer rend, MaterialPropertyBlock block) {
    		this.Renderer = rend;
    		this.Block = block;
            this._session = false;
    	}

    	public MaterialPropertyBlockChanied Apply() {
            _session = false;
    		Renderer.SetPropertyBlock (Block);
    		return this;
    	}

        public MaterialPropertyBlockChanied SetColor(string name, Color value) {
            CheckLoad ();
            Block.SetColor (name, value);
            return this;
        }
        public MaterialPropertyBlockChanied SetFloat(string name, float value) {
            CheckLoad ();
            Block.SetFloat (name, value);
            return this;
        }
        public MaterialPropertyBlockChanied SetMatrix(string name, Matrix4x4 value) {
            CheckLoad ();
            Block.SetMatrix (name, value);
            return this;
        }
        public MaterialPropertyBlockChanied SetTexture(string name, Texture value) {
            CheckLoad ();
            Block.SetTexture (name, value);
            return this;
        }
        public MaterialPropertyBlockChanied SetVector(string name, Vector4 value) {
            CheckLoad ();
            Block.SetVector (name, value);
            return this;
        }

        public Color GetColor(string name) {
            CheckLoad ();
            return (Color)Block.GetVector (name);
        }
        public float GetFloat(string name) {
            CheckLoad ();
            return Block.GetFloat (name);
        }
        public Matrix4x4 GetMatrix(string name) {
            CheckLoad ();
            return Block.GetMatrix (name);
        }
        public Texture GetTexture(string name) {
            CheckLoad ();
            return Block.GetTexture (name);
        }
        public Vector4 GetVector(string name) {
            CheckLoad ();
            return Block.GetVector (name);
        }

        public Color GetDefaultColor(string name) {
            CheckLoad ();
            return Renderer.sharedMaterial.GetColor (name);
        }
        public float GetDefaultFloat(string name) {
            CheckLoad ();
            return Renderer.sharedMaterial.GetFloat (name);
        }
        public Matrix4x4 GetDefaultMatrix(string name) {
            CheckLoad ();
            return Renderer.sharedMaterial.GetMatrix (name);
        }
        public Texture GetDefaultTexture(string name) {
            CheckLoad ();
            return Renderer.sharedMaterial.GetTexture (name);
        }
        public Vector4 GetDefaultVector(string name) {
            CheckLoad ();
            return Renderer.sharedMaterial.GetVector (name);
        }

        void CheckLoad() {
            if (!_session) {
                _session = true;
                Renderer.GetPropertyBlock (Block);
            }
        }
    }
}
