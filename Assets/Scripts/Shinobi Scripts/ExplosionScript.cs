using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ExplosionScript : MonoBehaviourPunCallbacks
{
    public float DestroyTime = .15f;
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
        
        PhotonView target = collision.gameObject.GetComponent<PhotonView>();
        if (target != null && (!target.IsMine || target.IsSceneView))
        {
            if (collision.tag == "Player")
            {
                target.RPC("ReduceHealth", RpcTarget.AllBuffered, Damage);
                ParentObject.GetComponent<ShinobiController>().UpdateSpecialMeter(25f);

                if (target.GetComponent<PlayerHealth>().CurrentHealth <= 0)
                    ParentObject.GetComponent<PlayerHealth>().YouEliminated("");
            }
        }
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