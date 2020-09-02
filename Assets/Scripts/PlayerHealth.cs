using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerHealth : MonoBehaviourPunCallbacks
{
    public float HealthAmount = 100;
    public Image BarImage;

    public Player plMove;
    public Rigidbody2D Rigid; 
    public BoxCollider2D Collider;
    public SpriteRenderer Sprite;
    public GameObject PlayerCanvas;

    void Awake()
    {
        if (photonView.IsMine)
        {
            GameManager.Instance.localPlayer = this.gameObject;
            BarImage.color = Color.green;
        }
        else
        {
            BarImage.color = Color.red;
        }
    }

    [PunRPC]
    public void ReduceHealth(float amount)
    {
        ModifyHealth(amount);
    }

    public void CheckHealth()
    {
        BarImage.fillAmount = HealthAmount / 100f;

        if (photonView.IsMine && HealthAmount <= 0)
        {
            GameManager.Instance.EnableRespawn();
            plMove.DisableInput = true;
            this.GetComponent<PhotonView>().RPC("Death", RpcTarget.AllBuffered);
        }
    }

    private void ModifyHealth(float amount)
    {
        if (photonView.IsMine)
        {
            HealthAmount -= amount;
        }
        else
        {
            HealthAmount -= amount;
        }

        CheckHealth();
    }

    [PunRPC]
    private void Death()
    {
        Rigid.gravityScale = 0;
        Collider.enabled = false;
        Sprite.enabled = false;
        PlayerCanvas.SetActive(false);
    }

    [PunRPC]
    private void Revive()
    {
        HealthAmount = 100;
        CheckHealth();
        Rigid.gravityScale = 1;
        Collider.enabled = true;
        Sprite.enabled = true;
        PlayerCanvas.SetActive(true);
    }

    public void EnableInput()
    {
        plMove.DisableInput = false;
    }
}
