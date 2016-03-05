using UnityEngine;
using System.Collections;

public class PlantAnimation : MonoBehaviour {
    public const string PROP_ANIM_TIME = "_AnimTex_T";

    Renderer _rnd;
    MaterialPropertyBlock _block;
    float _time;

	void OnEnable() {
        _rnd = GetComponent<Renderer> ();
        _rnd.GetPropertyBlock (_block = new MaterialPropertyBlock ());

        _block.SetFloat (PROP_ANIM_TIME, _time = 0f);
        _rnd.SetPropertyBlock (_block);
	}
	
	void Update () {
        var timeScale = 1f;
        if (_time < 10f)
            timeScale = 5f;
        else
            timeScale = 0f;
        _time += Time.deltaTime * timeScale;

        _block.SetFloat (PROP_ANIM_TIME, _time);
        _rnd.SetPropertyBlock (_block);
	}
}
