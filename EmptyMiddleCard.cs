using UnityEngine;

public class EmptyMiddleCard : MonoBehaviour {

	
	// Update is called once per frame
	void Update () {
        if (transform.childCount == 0)
        {
            GetComponent<Collider2D>().enabled = true;
        }
        else {
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
