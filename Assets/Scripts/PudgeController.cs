using System;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

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
    public AudioClip landSound;
    public AudioClip jumpSound;

    public float groundDistance;

    private Rigidbody _rb;
    
    public float jumpForce;

    private bool _jumping;

    void Start()
    {
        userSeekingModule.OnUserFound += OnUserFound;
        userSeekingModule.OnUserLost += OnUserLost;
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);

        UpdateNavMeshTarget();
        UpdateGroundDistance();
    }

    private void Step()
    {
        PlayRandomChain();
        PlayRandomStep();
    }

    private void UpdateGroundDistance()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, 100f))
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
        if (_jumping) return;
        if (Jump())
        {
            _jumping = true;
            animator.SetTrigger("Jump");
        }
    }

    private void PlayUserFoundSfx()
    {
        Say(userFoundSounds[Random.Range(0, userFoundSounds.Count)]);
    }

    private void OnUserLost()
    {
        user.OnJump -= OnUserJump;
        user = null;
    }

    private bool Jump()
    {
        try
        {
            if (groundDistance <= 0.1f)
            {
                if (agent.enabled)
                {
                    // set the agents target to where you are before the jump
                    // this stops her before she jumps. Alternatively, you could
                    // cache this value, and set it again once the jump is complete
                    // to continue the original move
                    agent.SetDestination(transform.position);
                    // disable the agent
                    agent.updatePosition = false;
                    agent.updateRotation = false;
                    agent.isStopped = true;
                }

                Debug.Log("Jumping");
                // make the jump
                _rb.isKinematic = false;
                _rb.useGravity = true;
                _rb.AddRelativeForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
                Say(jumpSound);
                PlayRandomStep();
                PlayRandomChain();
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_jumping && collision.collider != null && collision.collider.CompareTag("Ground"))
        {
            Debug.Log("Grounded");
            if (agent.enabled)
            {
                agent.updatePosition = true;
                agent.updateRotation = true;
                agent.isStopped = false;
            }

            _rb.isKinematic = true;
            _rb.useGravity = false;
            _jumping = false;
            animator.SetTrigger("Grounded");
            
            Say(landSound);
            PlayRandomStep();
            PlayRandomChain();
        }
    }

    private void Say(AudioClip clip)
    {
        speechAudioSource.clip = clip;
        speechAudioSource.Play();
    }

    private void PlayRandomStep()
    {
        stepAudioSource.clip = stepSounds[Random.Range(0, stepSounds.Count)];
        stepAudioSource.Play();
    }

    private void PlayRandomChain()
    {
        chainAudioSource.clip = chainSounds[Random.Range(0, chainSounds.Count)];
        chainAudioSource.Play();
    }
}