using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BatScript : MonoBehaviourPunCallbacks
{
    public float DestroyTime = .1f;
    public float Damage = 50f;

    [HideInInspector] public GameObject ParentObject;

    void Awake()
    {
        StartCoroutine("DestroyByTime");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine)
            return;
        
        //Debug.Log("Trigger: " + collision.tag);
        //Debug.Log("Bat Collided: " + collision.ClosestPoint(this.gameObject.transform.position));
        PhotonView target = collision.gameObject.GetComponent<PhotonView>();

        if (target == null)
        {
            ParentObject.GetComponent<SluggerController>().UpdateSpecialMeter(Damage);
        }
        else if (target != null && (!target.IsMine || target.IsSceneView))
        {
            if (collision.tag == "Player")
            {
                target.RPC("ReduceHealth", RpcTarget.AllBuffered, Damage);
                ParentObject.GetComponent<SluggerController>().UpdateSpecialMeter(Damage);
            }
        }
    }

    [PunRPC]
    public void SetDirection(Vector2 direction) 
    {
        var normal = new Vector2(1, 0);
        double angleInRadians = Math.Atan2(direction.y, direction.x) - Math.Atan2(normal.y, normal.x);
        float angle = (float) (angleInRadians * (180.0 / Math.PI));
        this.transform.localEulerAngles = new Vector3(0, 0, angle);
    }
    
    [PunRPC]
    private void DestroyOBJ() 
    {
        Destroy(this.gameObject);
    }

    IEnumerator DestroyByTime()
    {
        yield return new WaitForSeconds(DestroyTime);
        this.GetComponent<PhotonView>().RPC("DestroyOBJ", RpcTarget.AllBuffered);
    }
}
