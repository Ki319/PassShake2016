using UnityEngine;
using System.Collections;

public class CheckmarkSprite : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    SpriteRenderer.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void show(){
        SpriteRenderer.enabled = true;
    }
    public void hide(){
        SpriteRenderer.enabled = false;
    }
}
