using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnergyScript : MonoBehaviourPunCallbacks
{
    public float MoveSpeed = 8f;
    public float DestroyTime = .75f;
    public float Damage = 15f;

    [HideInInspector] public GameObject ParentObject;
    [HideInInspector] public Vector2 MovingDirection;

    private void Awake()
    {
        StartCoroutine("DestroyByTime");
        MovingDirection = new Vector2(1,1);
    }

    [PunRPC]
    public void SetDirection(Vector2 direction, int changeAngle) 
    {
        if (direction.y > .1)
        {
            if (changeAngle == 2) 
                direction.x += .2f;
            else if (changeAngle == 3) 
                direction.x -= .2f;
        }
        else
        {
            if (changeAngle == 2) 
                direction.y += .2f;
            else if (changeAngle == 3) 
                direction.y -= .2f;
        }

        var normal = new Vector2(1, 0);
        double angleInRadians = Math.Atan2(direction.y, direction.x) - Math.Atan2(normal.y, normal.x);
        float angle = (float) (angleInRadians * (180.0 / Math.PI));

        this.transform.localEulerAngles = new Vector3(0, 0, angle);


        var Mag = Math.Sqrt(direction.x * direction.x + direction.y * direction.y);
        MovingDirection = new Vector2((float)(direction.x / Mag), (float)(direction.y / Mag));
    }

    private void Update()
    {
        var move = new Vector3(MovingDirection.x, MovingDirection.y, 0);
        transform.position += move * MoveSpeed * Time.deltaTime;
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
            //if (!ParentObject.GetComponent<ZealotController>().inSpecialMode)
            //    ParentObject.GetComponent<ZealotController>().UpdateSpecialMeter(25f); 
        }
        else if (target != null && (!target.IsMine || target.IsSceneView))
        {
            if (collision.tag == "Player")
            {
                target.RPC("ReduceHealth", RpcTarget.AllBuffered, Damage);
                
                if (!ParentObject.GetComponent<ZealotController>().inSpecialMode)
                    ParentObject.GetComponent<ZealotController>().UpdateSpecialMeter(25f);

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

