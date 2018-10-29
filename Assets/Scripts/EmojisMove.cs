using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojisMove : MonoBehaviour {

    public float rotationVelocity;

    private bool orientation, enableRotation;
    private SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Start () {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        orientation = true;
        enableRotation = false;
        StartCoroutine(SetImageVisible());
        Invoke("DestroyObject", 3f);
        Invoke("EmojiSound", 0.8f);
    }
	
	// Update is called once per frame
	void Update () {
        if (enableRotation)
        {
            if (transform.rotation.z < -0.25) orientation = true;
            else if (transform.rotation.z > 0.25) orientation = false;

            if (orientation) transform.Rotate(transform.rotation.x, transform.rotation.y, transform.rotation.z + rotationVelocity);
            else transform.Rotate(transform.rotation.x, transform.rotation.y, transform.rotation.z - rotationVelocity);
        }
    }

    private IEnumerator SetImageVisible()
    {
        while (spriteRenderer.color.a < 1)
        {
            spriteRenderer.color = new Color(1, 1, 1, spriteRenderer.color.a + 0.05f);
            yield return new WaitForSeconds(0.08f);
        }
    }

    private void EmojiSound()
    {
        enableRotation = true;
        gameObject.GetComponent<AudioSource>().Play();
    }

    private IEnumerator SetImageInvisible()
    {
        while (spriteRenderer.color.a > 0)
        {
            spriteRenderer.color = new Color(1, 1, 1, spriteRenderer.color.a - 0.05f);
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void DestroyObject()
    {
        StartCoroutine(SetImageInvisible());
        Destroy(gameObject, 2f);
    }
}
