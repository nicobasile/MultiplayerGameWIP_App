using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BombScript : MonoBehaviourPunCallbacks
{
    public float MoveSpeed = 8f;
    public float DestroyTime = 3f;
    public float Damage = 50f;

    public GameObject Explosion_Prefab;

    [HideInInspector] public GameObject ParentObject;
    [HideInInspector] public Vector2 MovingDirection;
    [HideInInspector] private Vector3 rotation;
    

    private void Awake()
    {
        StartCoroutine("DestroyByTime");
        MovingDirection = new Vector2(1,1);
        rotation = new Vector3(0, 0, 2f);
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
        GameObject obj = PhotonNetwork.Instantiate(Explosion_Prefab.name, new Vector2(this.transform.position.x, this.transform.position.y), Quaternion.identity, 0);
        obj.GetComponent<ExplosionScript>().ParentObject = this.ParentObject;
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine)
            return;
        
        PhotonView target = collision.gameObject.GetComponent<PhotonView>();

        if (target == null)
        {
            this.GetComponent<PhotonView>().RPC("DestroyOBJ", RpcTarget.AllBuffered);
        }
        else if (target != null && (!target.IsMine || target.IsSceneView))
        {
            this.GetComponent<PhotonView>().RPC("DestroyOBJ", RpcTarget.AllBuffered);
        }
    }

    IEnumerator DestroyByTime()
    {
        yield return new WaitForSeconds(DestroyTime);
        this.GetComponent<PhotonView>().RPC("DestroyOBJ", RpcTarget.AllBuffered);
    }
}