using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallScript : MonoBehaviourPunCallbacks
{
    public float MoveSpeed = 8f;
    public float DestroyTime = 6f;
    public float Damage = 50f;

    [HideInInspector] public GameObject ParentObject;
    [HideInInspector] public Vector2 MovingDirection;
    [HideInInspector] private Vector3 rotation;


    private void Awake()
    {
        StartCoroutine("DestroyByTime");
        MovingDirection = new Vector2(1,1);
        rotation = new Vector3(0, 0, 1f);
    }

    [PunRPC]
    public void SetDirection(Vector2 direction) 
    {
        var Mag = Math.Sqrt(direction.x * direction.x + direction.y * direction.y);
        MovingDirection = new Vector2((float)(direction.x / Mag), (float)(direction.y / Mag));
    }

    private void Update()
    {
        var move = new Vector3(MovingDirection.x, MovingDirection.y, 0);
        transform.position += move * MoveSpeed * Time.deltaTime;
        this.transform.Rotate(rotation);
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
            //ParentObject.GetComponent<SluggerController>().UpdateSpecialMeter(Damage);
        }
        else if (target != null && (!target.IsMine || target.IsSceneView))
        {
            if (collision.tag == "Player")
            {
                target.RPC("ReduceHealth", RpcTarget.AllBuffered, Damage);
                ParentObject.GetComponent<SluggerController>().UpdateSpecialMeter(Damage);

                if (target.GetComponent<PlayerHealth>().CurrentHealth <= 0)
                    ParentObject.GetComponent<PlayerHealth>().YouEliminated("");
            }
            this.GetComponent<PhotonView>().RPC("DestroyOBJ", RpcTarget.AllBuffered);
        }
    }

    IEnumerator DestroyByTime()
    {
        yield return new WaitForSeconds(DestroyTime);
        this.GetComponent<PhotonView>().RPC("DestroyOBJ", RpcTarget.AllBuffered);
    }
}
