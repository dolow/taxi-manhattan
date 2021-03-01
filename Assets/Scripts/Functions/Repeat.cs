using UnityEngine;

public class Repeat : MonoBehaviour
{
    public Vector3 from = Vector3.zero;
    public Vector3 to = Vector3.zero;
    public float speed = 0.05f;

    private Vector3 diff = Vector3.zero;
    private Vector3 lastPosition = Vector3.zero;

    private void Start()
    {
        this.diff = this.to - this.from;
        this.lastPosition = this.transform.position;
    }

    private void Update()
    {
        this.transform.position = this.transform.position + this.diff * this.speed;
        if (Vector3.Distance(this.transform.position, this.to) >= Vector3.Distance(this.lastPosition, this.to))
        {
            this.transform.position = this.from;
        }
        this.lastPosition = this.transform.position;
    }
}
