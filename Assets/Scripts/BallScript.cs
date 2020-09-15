using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallScript : MonoBehaviourPunCallbacks
{
    public float MoveSpeed = 6f;
    public float Distance = 1f;
    public float DestroyTime = 3f;
    public float Damage = 25f;

    [HideInInspector] public GameObject ParentObject;
    [HideInInspector] public Vector2 MovingDirection;

    private void Awake()
    {
        StartCoroutine("DestroyByTime");
        MovingDirection = new Vector2(1,1);
    }

    [PunRPC]
    public void SetDirection(Vector2 direction) 
    {
        var Mag = Math.Sqrt(direction.x * direction.x + direction.y * direction.y);
        MovingDirection = new Vector2((float)(direction.x * Distance / Mag), (float)(direction.y * Distance / Mag));
    }

    private void Update()
    {
        var move = new Vector3(MovingDirection.x, MovingDirection.y, 0);
        transform.position += move * MoveSpeed * Time.deltaTime;
        //transform.position += move * runSpeed * Time.deltaTime;
        //transform.Translate(MovingDirection * MoveSpeed * Time.deltaTime);
    }

    [PunRPC]
    private void Bounce(Vector2 closest) 
    {
        //Debug.Log("Boing!");
        //Debug.Log(this.gameObject.transform.position + "    " + closest);
        if (Mathf.Abs(closest.x - this.gameObject.transform.position.x) > 
            Mathf.Abs(closest.y - this.gameObject.transform.position.y))
            SetDirection(new Vector2(-MovingDirection.x, MovingDirection.y));
        else
            SetDirection(new Vector2(MovingDirection.x, -MovingDirection.y));
    }

    [PunRPC]
    private void DestroyOBJ() 
    {
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine)
            return;
        
        //Debug.Log("Trigger: " + collision.tag);
        //Debug.Log("Ball Collided: " + collision.ClosestPoint(this.gameObject.transform.position));
        PhotonView target = collision.gameObject.GetComponent<PhotonView>();

        if (target == null)
        {
            this.GetComponent<PhotonView>().RPC("Bounce", RpcTarget.AllBuffered, collision.ClosestPoint(this.gameObject.transform.position));
            ParentObject.GetComponent<Player>().UpdateSpecialMeter(Damage);
        }
        else if (target != null && (!target.IsMine || target.IsSceneView))
        {
            if (collision.tag == "Player")
            {
                target.RPC("ReduceHealth", RpcTarget.AllBuffered, Damage);
                //ParentObject.GetComponent<Player>().UpdateSpecialMeter(Damage);
            }
            this.GetComponent<PhotonView>().RPC("DestroyOBJ", RpcTarget.AllBuffered);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision: " + collision.gameObject.tag); 
    }

    IEnumerator DestroyByTime()
    {
        yield return new WaitForSeconds(DestroyTime);
        this.GetComponent<PhotonView>().RPC("DestroyOBJ", RpcTarget.AllBuffered);
    }

}
