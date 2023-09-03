using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class PudgeController : MonoBehaviour
{
    public Animator animator;
    public UserSeekingModule userSeekingModule;
    public NavMeshAgent agent;
    public FirstPersonController user;
    public float stopDistance = 1f;

    public AudioSource stepAudioSource;
    public AudioSource chainAudioSource;
    public AudioSource speechAudioSource;

    public List<AudioClip> stepSounds;
    public List<AudioClip> chainSounds;
    public List<AudioClip> userFoundSounds;

    public float groundDistance;

    private Rigidbody _rb;

    void Start()
    {
        userSeekingModule.OnUserFound += OnUserFound;
        userSeekingModule.OnUserLost += OnUserLost;
    }

    private void Update()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);

        UpdateNavMeshTarget();
        UpdateGroundDistance();
    }

    private void Step()
    {
        stepAudioSource.clip = stepSounds[Random.Range(0, stepSounds.Count)];
        chainAudioSource.clip = chainSounds[Random.Range(0, stepSounds.Count)];
        stepAudioSource.Play();
        chainAudioSource.Play();
    }

    private void UpdateGroundDistance()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f))
        {
            groundDistance = hit.distance;
        }

        animator.SetFloat("GroundDistance", groundDistance);
    }

    private void UpdateNavMeshTarget()
    {
        if (user)
        {
            var targetPosition = (transform.position + user.transform.position) * 0.5f;
            if (Vector3.Distance(transform.position, targetPosition) > stopDistance)
            {
                agent.SetDestination(targetPosition);
                Debug.DrawLine(transform.position, targetPosition, Color.red);
            }
            else
            {
                agent.SetDestination(transform.position);
                Debug.DrawLine(transform.position, targetPosition, Color.green);
            }
        }
    }

    private void OnUserFound(GameObject foundUser)
    {
        animator.SetTrigger("Waving");
        user = foundUser.GetComponent<FirstPersonController>();
        user.OnJump += OnUserJump;
        PlayUserFoundSfx();
    }

    private void OnUserJump()
    {
        animator.SetTrigger("Jump");
        Jump();
    }

    private void PlayUserFoundSfx()
    {
        speechAudioSource.clip = userFoundSounds[Random.Range(0, userFoundSounds.Count)];
        speechAudioSource.Play();
    }

    private void OnUserLost()
    {
        user.OnJump -= OnUserJump;
        user = null;
    }

    private void Jump()
    {
        if (groundDistance <= 0.1f)
        {
            _rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
        }
    }
}