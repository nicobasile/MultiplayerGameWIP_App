using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NinjaStarScript : MonoBehaviourPunCallbacks
{
    public float MoveSpeed = 12f;
    public float DestroyTime = 1f;
    public float Damage = 20f;

    [HideInInspector] public GameObject ParentObject;
    [HideInInspector] public Vector2 MovingDirection;
    [HideInInspector] private Vector3 rotation;

    private void Awake()
    {
        StartCoroutine("DestroyByTime");
        MovingDirection = new Vector2(1,1);
        rotation = new Vector3(0, 0, -3f);
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
    private void DestroyOBJ() 
    {
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine)
            return;
        
        //Debug.Log("Trigger: " + collision.tag);
        //Debug.Log("Ninja Star Collided: " + collision.ClosestPoint(this.gameObject.transform.position));
        PhotonView target = collision.gameObject.GetComponent<PhotonView>();

        if (target == null)
        {
            this.GetComponent<PhotonView>().RPC("DestroyOBJ", RpcTarget.AllBuffered); 
            //ParentObject.GetComponent<PhotonView>().RPC("ReduceHealth", RpcTarget.AllBuffered, Damage);
            //ParentObject.GetComponent<PlayerHealth>().YouEliminated("");   
            //ParentObject.GetComponent<ShinobiController>().UpdateSpecialMeter(25f);
        }
        else if (target != null && (!target.IsMine || target.IsSceneView))
        {
            if (collision.tag == "Player")
            {
                target.RPC("ReduceHealth", RpcTarget.AllBuffered, Damage);
                ParentObject.GetComponent<ShinobiController>().UpdateSpecialMeter(25f);

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
