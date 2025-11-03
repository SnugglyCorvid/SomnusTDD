using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    //TODO: Expand AI with more behaviors and states as needed, attacking objects that block its path, for Rask, climbing from walls to ceiling, working in teams, etc.

    public enum AIState { Idle, Patrol, Chase, Search }
    public AIState currentState = AIState.Idle;

    [Header("References")]
    public NavMeshAgent agent;
    public Transform[] waypoints;
    public Transform player;

    [Header("Settings")]
    public float sightRange = 10f;
    public float fieldOfView = 90f;
    public float searchDuration = 5f;

    private int currentWaypointIndex = 0;
    private float searchTimer = 0f;

    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (waypoints.Length > 0) GoToNextWP();
    }

    void Update()
    {
        switch (currentState)
        {
            case AIState.Idle:
                Idle();
                break;
            case AIState.Patrol:
                Patrol();
                break;
            case AIState.Chase:
                Chase();
                break;
            case AIState.Search:
                Search();
                break;
        }
    }
    void Idle()
    {
        if (CanSeePlayer()) ChangeState(AIState.Chase);
    }

    void Patrol()
    {
        if (CanSeePlayer()) ChangeState(AIState.Chase);

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextWP();
    }

    void Chase()
    {
        if (player == null) return;
        agent.SetDestination(player.position);

        if (!CanSeePlayer())
        {
            searchTimer = 0f;
            ChangeState(AIState.Search);
        }
    }

    void Search()
    {
        searchTimer += Time.deltaTime;
        if (CanSeePlayer()) ChangeState(AIState.Chase);

        if (searchTimer >= searchDuration)
        {
            ChangeState(AIState.Patrol);
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (directionToPlayer.magnitude < sightRange && angle < fieldOfView / 2)
        {
            if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, out RaycastHit hit, sightRange))
            {
                if (hit.collider.CompareTag("Player"))
                    return true;
            }
        }
        return false;
    }

    void GoToNextWP()
    {
        if (waypoints.Length == 0) return;

        agent.destination = waypoints[currentWaypointIndex].position;
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    void ChangeState(AIState newState)
    {
        currentState = newState;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Vector3 left = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, fieldOfView / 2, 0) * transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + left * sightRange);
        Gizmos.DrawLine(transform.position, transform.position + right * sightRange);
    }

}