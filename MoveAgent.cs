using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MoveAgent : MonoBehaviour
{
    public enum State { PATROL, TRACE, ATTACK, DIE } // PATROL 순찰

    [SerializeField]
    private State state = State.PATROL;
    public bool isDie = false;
    public AudioClip walkSfx;
    public AudioClip punchSfx;
    public AudioClip hitSfx;
    public AudioClip dieSfx;

    private List<GameObject> buildings;

    private readonly string player = "PLAYER";
    private SphereCollider sphereCollider;
    private NavMeshAgent agent;
    private Transform playerTr;
    private Transform enemyTr;
    private float damping = 1.0f;
    private bool tracingPlayer = false;
    private int enemyHP = 10;
    [SerializeField]
    private GameObject target;
    private BuildingController targetController;

    private WaitForSeconds ws;

    private Animator animator;
    private EnemyPunch enemyPunch;
    private AudioSource audio;
    private bool isPlaying = false;
    private bool isHited = false;
    private readonly int hashOnHit = Animator.StringToHash("onHit");
    private readonly int hashMoving = Animator.StringToHash("isMoving");
    private readonly int hashDie = Animator.StringToHash("die");
    private readonly int hashDieIdx = Animator.StringToHash("dieIdx");
    private readonly int hashPlayerDie = Animator.StringToHash("playerDie");
    private readonly float attackDist = 1f;
    private readonly float traceDist = 5f;
    private readonly Vector3 floorVector = new Vector3(1f, 0f, 1f);

    private Vector3 enemyFloorPos;
    private Vector3 targetFloorPos;

    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();

        var player = GameObject.FindGameObjectWithTag("PLAYER");
        if (player != null) playerTr = player.GetComponent<Transform>();
        enemyTr = GetComponent<Transform>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemyPunch = GetComponent<EnemyPunch>();
        audio = GetComponent<AudioSource>();
        //agent.autoBraking = false;
        agent.updateRotation = false;
        //agent.speed = patrolSpeed;

        buildings = GameObject.Find("GameManager").GetComponent<GameManager>().buildings;

        //MoveWayPoints();

        SetNext();

        StartCoroutine(CheckState());
        StartCoroutine(Action());
    }

    IEnumerator CheckState()
    {
        while (!isDie)
        {
            if (state == State.DIE) yield break;

            enemyFloorPos = Vector3.Scale(enemyTr.position, floorVector);
            targetFloorPos = Vector3.Scale(target.transform.position, floorVector);
            float dist = Vector3.Distance(enemyFloorPos, targetFloorPos);

            if (dist <= attackDist) state = State.ATTACK;
            else if (tracingPlayer && dist <= traceDist) state = State.TRACE;
            else state = State.PATROL;

            yield return ws;
        }
    }
    IEnumerator Action()
    {
        while (!isDie)
        {
            yield return ws;

            switch (state)
            {
                case State.PATROL:
                    SetPunchFalse();
                    tracingPlayer = false;
                    animator.SetBool(hashMoving, true);
                    PlayWalk();
                    break;
                case State.TRACE:
                    SetPunchFalse();
                    //moveAgent.traceTarget = playerTr.position;
                    target = playerTr.gameObject;
                    TraceTarget(playerTr.position);
                    animator.SetBool(hashMoving, true);
                    PlayWalk();
                    break;
                case State.ATTACK:
                    //StopAudio();
                    Stop();
                    if (!isPlaying) 
                    {
                        PlayOneShot(punchSfx, 2.5f);
                    } 
                    animator.SetBool(hashMoving, false);
                    if (enemyPunch.isPunch == false) enemyPunch.isPunch = true;
                    break;
                case State.DIE:
                    isDie = true;
                    SetPunchFalse();
                    //Damage.OnPlayerDie -= this.OnPlayerDie;
                    //StopAudio();
                    Stop();
                    PlayOneShot(dieSfx, 7f);
                    animator.SetInteger(hashDieIdx, Random.Range(0, 3));
                    animator.SetTrigger(hashDie);
                    GetComponent<CapsuleCollider>().enabled = false;
                    break;
            }
        }
    }

    IEnumerator PlayingAudio(float playTime)
    {
        isPlaying = true;
        yield return new WaitForSeconds(playTime);
        isPlaying = false;
    }

    void TraceTarget(Vector3 pos)
    {
        if (agent.isPathStale) return;
        agent.destination = pos;
        agent.isStopped = false;
    }

    public void Stop()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    void Update()
    {
        if (enemyHP <= 0) state = State.DIE;

        if(!isDie)
        {
            if (agent.velocity.magnitude >= 0.05f) sphereCollider.enabled = true;
            else sphereCollider.enabled = false;

            if (agent.isStopped == false)
            {
                Quaternion rot = Quaternion.LookRotation(agent.desiredVelocity);
                enemyTr.rotation = Quaternion.Slerp(enemyTr.rotation, rot, Time.deltaTime * damping); // Slerp time
            }
            if (tracingPlayer) return;
            if (targetController.isDestroy || !target.activeSelf)
            {
                SetPunchFalse();
                buildings.Remove(target);
                SetNext();
            }
        }
    }

    void SetNext()
    {
        if (agent.isPathStale) return;

        float min = Vector3.Distance(transform.position, buildings[0].transform.position);
        int minIndex = 0;
        float current;
        for(int i = 1; i < buildings.Count; i++)
        {
            current = Vector3.Distance(transform.position, buildings[i].transform.position);
            if (current < min)
            {
                min = current;
                minIndex = i;
            }
        }
        target = buildings[minIndex];
        targetController = target.GetComponent<BuildingController>();
        enemyPunch.SetTarget(target);
        agent.destination = target.transform.position;
        agent.isStopped = false;
    }

    void SetPunchFalse()
    {
        enemyPunch.isPunch = false;
        enemyPunch.boxColls[0].enabled = false;
        enemyPunch.boxColls[1].enabled = false;
    }
    void PlayWalk()
    {
        audio.clip = walkSfx;
        audio.loop = true;
        if (audio.isPlaying) return;
        audio.Play();
    }
    void PlayOneShot(AudioClip clip, float playTime)
    {
        audio.clip = clip;
        audio.loop = false;
        audio.Play();
        StartCoroutine(PlayingAudio(playTime));
    }

    public void OnHit()
    {
        if(!isHited)
        {
            enemyHP--;
            animator.SetTrigger(hashOnHit);
            if (enemyHP <= 0)
            {
                state = State.DIE;
                return;
            }

            if (!tracingPlayer)
            {
                tracingPlayer = true;
                SetPunchFalse();
            }

            PlayOneShot(hitSfx, 1f);
            StartCoroutine(IsHited());
        }
        
    }

    IEnumerator IsHited()
    {
        isHited = true;
        yield return new WaitForSeconds(0.5f);
        isHited = false;
    }

    public void OnPlayerDie() // 이벤트 함수
    {
        Stop();
        enemyPunch.isPunch = false;
        StopAllCoroutines();
        animator.SetTrigger(hashPlayerDie);
    }
}
