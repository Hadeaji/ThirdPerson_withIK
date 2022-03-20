using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.Animations.Rigging;

public class ThirdPersonConfig : MonoBehaviour
{
    [SerializeField] private Rig aimRig;
    [SerializeField] private CinemachineVirtualCamera aimVirtualCam;
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] private Transform shootPointTransform;
    [SerializeField] private Transform pfBulletProjectile;
    [SerializeField] private Transform spawnBulletPosition;

    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;
    private Transform rightArmIKTarget;
    private Vector3 mouseWorldPosition;

    private float aimRigWieght = 1f;

    // debug serialization
    [SerializeField] private float shotDamage = 30f;
    [SerializeField] private float initialChargeMultiplier = 1f;
    private float chargeMultiplier = 1f;
    [SerializeField] private float chargeRate = 1f;
    [SerializeField] private float maxChargeMultiplier = 3f;
    private float timeDelay = 1f;
    private float timer = 0f;

    [SerializeField] private float reloadTimer = 0.2f;
    //

    // animation IDs
    private int _animIDCharge;

    //private Vector3 chargeTargetOriginal;
    //private Vector3 chargeTargetMax = new Vector3(-0.154f, -0.853f, 0.298f);

    private void Awake()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        animator = GetComponent<Animator>();
        rightArmIKTarget = aimRig.GetComponent<LookAtObj>().rightArmIKTarget;
        //chargeTargetOriginal = rightArmIKTarget.position;
        // initial mouse position
        mouseWorldPosition = Vector3.zero;
    }

    private void Update()
    {
        _animIDCharge = Animator.StringToHash("IsCharging");
        HandleWorldMousePositoion();
        HandleAim();
        HandleShooting();

        // updating aim rig stat all time
        aimRig.weight = Mathf.Lerp(aimRig.weight, aimRigWieght, Time.deltaTime * 20f);
    }

    private void OnAimStopped()
    {
        // deactivate the aim camera and return to mouse normal Sensitivity
        aimVirtualCam.gameObject.SetActive(false);
        thirdPersonController.SetSensitivity(normalSensitivity);
        thirdPersonController.SetRotateOnMove(true);
        aimRigWieght = 0f;
    }
    private void OnAimStarted()
    {
        // activate the aim camera after trigerring the aim
        aimVirtualCam.gameObject.SetActive(true);
        // Mouse Aim Sensitivity
        thirdPersonController.SetSensitivity(aimSensitivity);
        thirdPersonController.SetRotateOnMove(false);
        aimRigWieght = 1f;
    }
    private void HandleWorldMousePositoion()
    {
        // using ray cast with the default layer to find the point you are aiming at 
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            // change shoot transfor position
            shootPointTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
        }

    }

    private void HandleAim()
    {
        if (starterAssetsInputs.aim)
        {
            OnAimStarted();
            AimAnimations();
        }
        else
        {
            OnAimStopped();
            // Aim Layer exit
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
        }
    }

    private void AimAnimations()
    {
        // Aim Layer Animations 1 is the layer index 
        animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));

        Vector3 worldAimTarget = mouseWorldPosition;
        worldAimTarget.y = transform.position.y;
        Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

        // rotate the player object to aim point
        transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
    }

    private void HandleShooting()
    {
        if (!starterAssetsInputs.aim)
        {
            ChargeAnimation(false);
            return;
        }
        if (starterAssetsInputs.charge)
        {
            // initial shoot damage * charge damage multiplier ex: 0.5
            // increase charge damage multiplier with time * charge rate
            // cap the charge damage multiplier
            Debug.Log("Charge Multiplier:" + chargeMultiplier);
            Debug.Log("timer:" + timer);
            // charge animation
            ChargeAnimation(true);
            timer += Time.deltaTime;
            if (timer >= timeDelay)
            {
                timer = 0f;
                if (chargeMultiplier < maxChargeMultiplier)
                {
                    chargeMultiplier += chargeRate;
                    // charge animation effect
                } else
                {
                    chargeMultiplier = maxChargeMultiplier;
                    // max charge animation
                }
            }
        }
        if (starterAssetsInputs.shoot)
        {
            Shoot();
            // shoot animation

            ChargeAnimation(false);
            // reset Multiplier
            chargeMultiplier = initialChargeMultiplier;
        }
    }

    private void Shoot()
    {
        Debug.Log("Shot Damage:" + shotDamage * chargeMultiplier);
        Vector3 aimDirection = (mouseWorldPosition - spawnBulletPosition.position).normalized;
        Instantiate(pfBulletProjectile, spawnBulletPosition.position, Quaternion.LookRotation(aimDirection, Vector3.up));
        starterAssetsInputs.shoot = false;
    }
    private void ChargeAnimation(bool state)
    {
        animator.SetBool(_animIDCharge, state);
        //if (state)
        //{
        //    rightArmIKTarget.position = chargeTargetMax;
        //} else
        //{
        //    rightArmIKTarget.position = chargeTargetOriginal;
        //}
    }
}
