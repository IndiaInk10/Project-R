using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShpereController : MonoBehaviour
{
    private readonly float moveDistance = 0.5f;
    private readonly WaitForSeconds waitSec = new WaitForSeconds(1f);

    private bool isCheck = false;
    private Transform previousTransform;
    private SphereCollider sphereCollider;

    // Start is called before the first frame update
    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCheck) StartCoroutine(PreviousTransform());
    }

    IEnumerator PreviousTransform()
    {
        isCheck = true;

        previousTransform = transform;
        yield return waitSec;
        if((previousTransform.position - transform.position).magnitude >= moveDistance)
            sphereCollider.enabled = true;
        else
            sphereCollider.enabled = false;

        isCheck = false;
    }
}
