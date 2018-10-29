using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonkeyController : MonoBehaviour {

    public Sprite[] sprites;
    public float moveVelocity;

    private SpriteRenderer spriteRenderer;
    private float yInitial;
    private int direction;

    // Use this for initialization
    void Start()
    {
        yInitial = transform.position.y;
        direction = -1;
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        StartCoroutine(SetImageVisible());
        StartCoroutine(ChangeImage());
        Invoke("DestroyObject", 4f);
        Invoke("EmojiSound", 1.4f);
    }

    private IEnumerator ChangeImage()
    {
        for (int i=0; i<sprites.Length; i++)
        {
            yield return new WaitForSeconds(0.7f);
            spriteRenderer.sprite = sprites[i];
        }
    }

    private IEnumerator SetImageVisible()
    {
        while (spriteRenderer.color.a < 1)
        {
            spriteRenderer.color = new Color(1, 1, 1, spriteRenderer.color.a + 0.05f);
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void EmojiSound()
    {
        gameObject.GetComponent<AudioSource>().Play();
        StartCoroutine(MoveUpDown());
    }

    private IEnumerator MoveUpDown()
    {
        for (; ; )
        { 
            if (transform.position.y > yInitial + 0.05f) direction = -1;
            else if (transform.position.y < yInitial - 0.05f) direction = 1;

            transform.Translate(Vector3.up * moveVelocity * direction);

            yield return new WaitForSeconds(0.01f);
        }
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
