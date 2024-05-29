using System.Collections.Generic;
using Physics_2p5D;
using UnityEngine;
using UnityEngine.AI;
using Rigidbody = Physics_2p5D.Rigidbody;

public enum MovementSource
{
    Jump,
    Dash,
}


public class CharacterController : MonoBehaviour
{
    [SerializeField] private RigidbodyParams rigidbodyParams;
    [SerializeField] private new CapsuleCollider collider;
    [SerializeField] private bool interpolate;
    [SerializeField] private float gravity;
    [SerializeField] private float maxWaterDepth;
    [SerializeField] private int frameInterval = 1;
    private int myFrame = 0;

    private Rigidbody rb;
    private Vector3 camVelocity;
    private Vector3 previousPosition;
    private Vector3? displacement;
    public bool previousGrounded;

    public CapsuleCollider Collider => collider;
    public Vector3? Displacement => displacement;

    public Rigidbody Rb => rb;
    
    public bool IsJumping => rb.HasForceOverTime((int)MovementSource.Jump);
    private HashSet<MovementSource> forceOverTimeHash;
    private Queue<ForceOverTime> forceOverTimeQueue;

    void Awake()
    {
        rb = new Rigidbody(transform, collider, rigidbodyParams);
        forceOverTimeHash = new HashSet<MovementSource>();
        forceOverTimeQueue = new Queue<ForceOverTime>();
    }

    public void SetFrameInterval(int frameInterval)
    {
        this.frameInterval = frameInterval;
        myFrame = Random.Range(0, frameInterval);
    }
    
    void Update()
    {
        if (interpolate)
            InterpolatePosition();
    }

    private void InterpolatePosition()
    {
        transform.position = Vector3.Lerp(transform.position, rb.Position, Time.deltaTime * 20);
    }

    private bool IsStunned()
    {
        return false;
    }

    public void QueueForceOverTime(ForceOverTime forceOverTime, MovementSource source, bool singleInstance)
    {
        if (singleInstance && !forceOverTimeHash.Contains(source))
        {
            forceOverTimeQueue.Enqueue(forceOverTime);
        }
        else
        {
            forceOverTimeQueue.Enqueue(forceOverTime);
        }
    }
    
    public void Move(Vector3 displacement)
    {
        if (this.displacement == null)
        {
            this.displacement = displacement;
            
        }
        else
        {
            this.displacement += displacement;   
        }
    }
    
    private void FixedUpdate()
    {
        if (frameInterval > 0 && Time.frameCount % frameInterval == myFrame)
        {
            return;
        }
        
        previousGrounded = rb.Grounded;
        if (!rb.Grounded)
            rb.HasDrag = true;
        else
            rb.HasDrag = false;

        foreach (var f in forceOverTimeQueue)
        {
            if (f.source == (int)MovementSource.Dash)
            {
                rb.RestartGravity();
            }

            rb.AddForceOverTime(f);
        }
        forceOverTimeQueue.Clear();
        forceOverTimeHash.Clear();

        var dt = frameInterval > 0 ? frameInterval * Time.fixedDeltaTime : Time.fixedDeltaTime;
        if (displacement.HasValue && !IsStunned() && !rb.HasForceOverTime((int)MovementSource.Dash))
        {
            if (!rb.HasDrag)
                displacement /= dt;

            rb.AddForce(displacement.Value);
            displacement = null;
        }
        
        if (rb.HasForceOverTime((int)MovementSource.Jump))
            rb.RestartGravity();

        rb.AddForce(Vector3.down * gravity);
        rb.Step(dt);
        
        if (NavMesh.SamplePosition(rb.Position, out var hit, .2f, NavMesh.AllAreas))
        {
            rb.RestPosition(hit.position, false);
        }
        
        if (!interpolate)
            transform.position = rb.Position;
        previousPosition = transform.position;
    }
}