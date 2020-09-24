using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerHealth : MonoBehaviourPunCallbacks
{
    public float MaxHealth = 150f;
    [HideInInspector] public float CurrentHealth;
    public Text CurrentHealthText;
    public Image BarImage;

    //public Player plMove;
    public Rigidbody2D Rigid; 
    public BoxCollider2D Collider;
    public SpriteRenderer Sprite;
    public GameObject PlayerCanvas;
    [HideInInspector] public int Eliminations = 0;

    void Awake()
    {
        CurrentHealth = MaxHealth;
        CurrentHealthText.text = CurrentHealth.ToString();
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
        Debug.Log("OUCH: " + amount);
        ModifyHealth(amount);
    }

    public void CheckHealth()
    {
        CurrentHealthText.text = CurrentHealth.ToString();
        BarImage.fillAmount = CurrentHealth / MaxHealth;

        if (photonView.IsMine && CurrentHealth <= 0)
        {
            GameManager.Instance.EnableRespawn();
            //plMove.DisableInput = true;
            this.GetComponent<PhotonView>().RPC("Death", RpcTarget.AllBuffered);
        }
    }

    private void ModifyHealth(float amount)
    {
        if (photonView.IsMine)
        {
            CurrentHealth -= amount;
        }
        else
        {
            CurrentHealth -= amount;
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

    public void YouEliminated(string name)
    {
        Eliminations++;
    }

    [PunRPC]
    private void Revive()
    {
        CurrentHealth = MaxHealth;
        CheckHealth();
        Rigid.gravityScale = 1;
        Collider.enabled = true;
        Sprite.enabled = true;
        PlayerCanvas.SetActive(true);
    }

    public void EnableInput()
    {
        //plMove.DisableInput = false;
    }
}
