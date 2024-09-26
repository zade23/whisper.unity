using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParamCube : MonoBehaviour {

    public int band;
    public float startScale, scaleMultiplier;

    Material material;
	void Start () {
        material = GetComponent<MeshRenderer>().materials[0];
	}
	
	// Update is called once per frame
	void Update () {
    
            float newYScale = (AudioVisable._audioBandBuffer[band]) * scaleMultiplier + startScale;
            if (!float.IsNaN(newYScale) && !float.IsInfinity(newYScale))
            {
                transform.localScale = new Vector3(transform.localScale.x, newYScale, transform.localScale.z);
            }
            Color color = new Color(AudioVisable._audioBandBuffer[band], AudioVisable._audioBandBuffer[band], AudioVisable._audioBandBuffer[band]);
            material.SetColor("_EmissionColor",color);      
    }
}

