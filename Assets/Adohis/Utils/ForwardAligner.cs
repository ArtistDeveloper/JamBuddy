using UnityEngine;

public class ForwardAligner : MonoBehaviour
{
    public Transform target;
    // Update is called once per frame
    void Update()
    {
        transform.forward = target.transform.forward; 
    }
}
