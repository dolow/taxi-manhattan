using UnityEngine;

public class Raycaster : MonoBehaviour
{
    public delegate void RaycastHitChanged(Collider prev, Collider current);

    public RaycastHitChanged OnRaycastHitChanged = null;

    public int raycastFrameDuration = 6;

    private int frames = 0;
    private Transform target = null;
    private Collider currentCollider = null;

    private void Update()
    {
        this.frames++;
        if (this.frames % this.raycastFrameDuration != 0)
        {
            return;
        }

        this.frames = 0;

        RaycastHit hit;
        if (Physics.Raycast(this.transform.position, this.target.position - this.transform.position, out hit, 50))
        {
            if (this.currentCollider == null || this.currentCollider.gameObject.GetInstanceID() != hit.collider.gameObject.GetInstanceID())
            {
                Collider prevCollider = this.currentCollider;
                this.currentCollider = hit.collider;
                this.OnRaycastHitChanged?.Invoke(prevCollider, this.currentCollider);
            }
        }
        else
        {
            if (this.currentCollider != null)
            {
                Collider prevCollider = this.currentCollider;
                this.currentCollider = null;
                this.OnRaycastHitChanged?.Invoke(prevCollider, this.currentCollider);
            }
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}
