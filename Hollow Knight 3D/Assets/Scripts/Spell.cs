using UnityEngine;

public class Spell : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 22f;
    [SerializeField] private float maxTime = 2f;

    private float buffer = 0f;
    private bool once = true;

    private void Awake()
    {
        buffer = maxTime;
    }

    void Update()
    {
        buffer -= Time.deltaTime;

        transform.Translate(moveSpeed * Time.deltaTime * -transform.forward, Space.World);

        if (buffer < 0f)
            Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if(other.CompareTag("Enemy") && once)
        {
            once = false;
            HornetStats hs = other.gameObject.GetComponent<HornetStats>();
            hs.DecreaseHealth(true);
        }
    }
}
