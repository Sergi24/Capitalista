using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailController : MonoBehaviour {

    public float velocity;

    private Vector3 destination;

    // Use this for initialization
    void Start () {
        InvokeRepeating("ChangeDestination", 0f, 4f);
	}
	
	// Update is called once per frame
	void Update () {
        transform.Translate((destination - transform.position) * velocity * Time.deltaTime * 0.5f);
	}

    private void ChangeDestination()
    {
        destination = new Vector3(Random.Range(-10, 10), Random.Range(-5, 5), 150);
    }
}
