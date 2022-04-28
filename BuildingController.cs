using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingController : MonoBehaviour
{
    private readonly string enemy = "ENEMY";
    private readonly string enemyHand = "ENEMYHAND";
    private readonly string player = "PLAYER";
    private readonly WaitForSeconds waitSec = new WaitForSeconds(1f);
    private readonly int power = 1000;

    private Rigidbody rb;
    private MeshCollider meshCollider;
    private GameManager gameManager;
    private AudioSource audio;
    private bool isJumped = false;
    private Vector3 defaultPos;
  
    private int buildingHP = 2;

    public bool isDestroy = false;
    public bool isLong = false;
    public AudioClip destroySfx;

    // Start is called before the first frame update
    void Start()
    {
        defaultPos = transform.localPosition;
        rb = GetComponent<Rigidbody>();
        meshCollider = GetComponent<MeshCollider>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        audio = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!isDestroy && transform.localPosition.y >= defaultPos.y + 4f) transform.localPosition = defaultPos;
        if (isDestroy)  transform.localRotation = Quaternion.Euler(Random.Range(0f, 2f), 0, Random.Range(0f, 2f));
    }

    public void DoDestroyAnimation()
    {
        Destroy(meshCollider);
        audio.PlayOneShot(destroySfx);
        isDestroy = true;
        StartCoroutine(BuildingDestroy());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag(enemyHand))
        {
            buildingHP--;
            
            if (buildingHP > 0) return;
            DoDestroyAnimation();
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag(enemyHand))
        {
            buildingHP--;

            if (buildingHP > 0) return;
            DoDestroyAnimation();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.CompareTag(enemy) || other.gameObject.CompareTag(player))
        {
            if (isJumped) return;
            StartCoroutine(BuildingJump());
        }
    }

    IEnumerator BuildingJump()
    {
        isJumped = true;
        rb.AddForce(Vector3.up * Random.Range(1, 4) * power);
        yield return waitSec;
        isJumped = false;
    }
    IEnumerator BuildingDestroy()
    {
        if(isLong) yield return new WaitForSeconds(12f);
        else  yield return new WaitForSeconds(6f);
        StartCoroutine(gameManager.OnBuildingDestroy());
        gameObject.SetActive(false);
    }
}
