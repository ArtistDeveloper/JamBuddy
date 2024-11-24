using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    private Transform _target;
    private NavMeshAgent _agent;

    private void Start()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        _agent.SetDestination(_target.position);
    }
}
