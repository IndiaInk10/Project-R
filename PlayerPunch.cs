using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPunch : MonoBehaviour
{
    private readonly string enemy = "ENEMY";
    private readonly string buildings = "BUILDING";
    private OVRInput.Controller controller;

    public bool isLeft = false;

    private void Start()
    {
        controller = isLeft ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag(enemy) && !other.isTrigger)
        {
            other.gameObject.GetComponent<MoveAgent>().OnHit();
            StartCoroutine(PlayHaptic(controller, 0.5f));
        }
        else if(other.gameObject.CompareTag(buildings))
        {
            StartCoroutine(PlayHaptic(controller, 0.5f));
        }
    }

    IEnumerator PlayHaptic(OVRInput.Controller _controller, float playTime)
    {
        OVRInput.SetControllerVibration(1f, 1f, _controller);
        yield return new WaitForSeconds(playTime);
        OVRInput.SetControllerVibration(0f, 0f, _controller);
    }
}
