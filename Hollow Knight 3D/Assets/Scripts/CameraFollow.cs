using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform followPosition;

    private void Awake()
    {
        transform.rotation = followPosition.rotation;
    }

    void Update()
    {
        transform.position = followPosition.position;
    }
}
