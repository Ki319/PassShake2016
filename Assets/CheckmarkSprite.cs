using UnityEngine;
using System.Collections;

public class CheckmarkSprite : MonoBehaviour {
    bool pendingHide = false;
    float timeOn;
	// Use this for initialization
	void Start () {
        timeOn = Time.time * 1000;
	    GameObject[] gameObjects = gameObject.scene.GetRootGameObjects();
        int i = 0;


        for (i = 0; i < gameObjects.Length && !gameObjects[i].ToString().StartsWith("Canvas"); i++) ;
        //gameObjects[i].GetComponent<CanvasRenderer>().GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        //foreach (Component c in gameObjects[i].GetComponents()) Debug.Log(c.ToString());
	}
	
	// Update is called once per frame
	void Update () {
	    if(pendingHide && (timeOn - (Time.time*1000) > 500 )){
            this.hide();
        }
	}

    public void show(){
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
        pendingHide = true;
        timeOn = Time.time*1000;
        
    }
    public void hide(){
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }
}
