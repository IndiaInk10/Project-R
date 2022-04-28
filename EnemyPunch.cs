using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPunch : MonoBehaviour
{
    private Animator animator;
    private Transform enemyTr;
    private readonly string player = "PLAYER";
    private readonly int hashPunch = Animator.StringToHash("punch");
    private readonly int hashPunchIdx = Animator.StringToHash("punchIdx");
    private readonly int hashLeftRight = Animator.StringToHash("leftRight");
    private readonly WaitForSeconds waitSec = new WaitForSeconds(4f);
    private float nextPunch = 0.0f;
    private readonly float punchRate = 3f;
    private readonly float damping = 10.0f;
    private Transform targetTr;
    private BuildingController targetController;

    public bool isPunch = false;
    public bool isForced = false;
    public BoxCollider[] boxColls;
    //public AudioClip reloadSfx;
    //public GameObject bullet;
    //public Transform firePos;
    //public MeshRenderer muzzleFlash;
    void Start()
    {
        enemyTr = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        //muzzleFlash.enabled = false;
    }

    void Update()
    {
        if (isPunch)
        {
            if (Time.time >= nextPunch)
            {
                Punch();
                nextPunch = Time.time + punchRate + Random.Range(0.0f, 0.3f);
            }
            Quaternion rot = Quaternion.LookRotation(targetTr.position - enemyTr.position);
            enemyTr.rotation = Quaternion.Slerp(enemyTr.rotation, rot, Time.deltaTime * damping);
        }
    }

    public void SetTarget(GameObject target)
    {
        targetTr = target.transform;
        if (target.CompareTag(player)) targetController = null;
        else targetController = target.GetComponent<BuildingController>();
    }
    void Punch()
    {
        int random = Random.Range(0, 2) == 1 ? 1 : -1;
        animator.SetInteger(hashLeftRight, random);

        if (targetTr.gameObject.CompareTag(player)) random = Random.Range(0, 2) == 1 ? 1 : -1;
        else random = -1;
        animator.SetInteger(hashPunchIdx, random);

        animator.SetTrigger(hashPunch);

        if(targetController != null)
        {
            if (!isForced)
            {
                isForced = true;
                StartCoroutine(ForceDestroy(targetController));
            }
        }

        StartCoroutine(WaitPunchAnim());
    }

    IEnumerator WaitPunchAnim()
    {
        yield return new WaitForSeconds(1f);
        boxColls[0].enabled = true;
        boxColls[1].enabled = true;
    }

    IEnumerator ForceDestroy(BuildingController buildingController)
    {
        yield return new WaitForSeconds(5f);
        buildingController.DoDestroyAnimation();
        isForced = false;
    }
}
