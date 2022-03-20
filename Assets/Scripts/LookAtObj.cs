using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LookAtObj : MonoBehaviour
{
    private Rig rig;
    private float targetWight;
    [SerializeField] public Transform rightArmIKTarget;

    private void Awake()
    {
        rig = GetComponent<Rig>();
    }

    private void Update()
    {
        rig.weight = Mathf.Lerp(rig.weight, targetWight, Time.deltaTime * 10f);

        if (Input.GetKeyDown(KeyCode.T))
        {
            targetWight = 1f;
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            targetWight = 0f;
        }
        
    }
}
