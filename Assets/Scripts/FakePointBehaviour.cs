using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakePointBehaviour : MonoBehaviour {

    public FakePointManager manager;
    public float Speed;
    public int pid;
    public Vector3 direction;
	
	void Start () {
        direction = Random.onUnitSphere;
        var rndVelocity = direction * Speed;
        rndVelocity.z = 0.0f;

        GetComponent<Rigidbody2D>().velocity = rndVelocity;
        transform.GetChild(0).GetComponent<TextMesh>().text = "ID : " + pid;
    }

    public void UpdateSpeed()
    {
        GetComponent<Rigidbody2D>().velocity = direction * Speed;
    }

    private void Update()
    {
        var actualPos = Camera.main.WorldToViewportPoint(transform.position);
        var newPos = transform.position;

        if (actualPos.x < 0.00f || actualPos.x > 1f || actualPos.y < 0.0f || actualPos.y > 1f)
            newPos = Vector3.zero;

        transform.position = newPos;
    }

    private void OnMouseDown()
    {
        manager.CanMoveCursorPoint = false;
    }

    private void OnMouseDrag()
    {
        var newPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        newPos.z = 0;
        newPos.x = Mathf.Clamp(newPos.x, 0.05f, 0.95f);
        newPos.y = Mathf.Clamp(newPos.y, 0.05f, 0.95f);
        newPos = Camera.main.ViewportToWorldPoint(newPos);
        newPos.z = 0;
        transform.position = newPos;
    }

    private void OnMouseUp()
    {
        manager.CanMoveCursorPoint = true;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.name == "Left" || collision.name == "Right")
        {
            direction = new Vector2(-direction.x , direction.y);
        }
        if(collision.name == "Top" || collision.name == "Bot")
        {
            direction = new Vector2(direction.x, -direction.y);
        }

        GetComponent<Rigidbody2D>().velocity = direction * Speed;
    }
}
