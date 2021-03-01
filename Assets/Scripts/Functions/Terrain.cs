using UnityEngine;

[RequireComponent(typeof(FieldTile))]
public class Terrain : MonoBehaviour
{
    public float speed = 0.01f;

    private float velocity = 0.00f;
    private float opacity = 1.0f;
    private MeshRenderer meshRenderer = null;

    private void Start()
    {
        this.meshRenderer = this.GetComponent<MeshRenderer>();
        this.opacity = meshRenderer.sharedMaterial.color.a;
        // TODO: underground
        //this.meshRenderer.sharedMaterial.renderQueue = 3100;
    }
    /*
    private void Update()
    {
        this.opacity += this.velocity;
        if (this.opacity > 1.0f)
            this.opacity = 1.0f;
        if (this.opacity < 0.0f)
            this.opacity = 0.0f;

        Color baseColor = this.meshRenderer.sharedMaterial.color;
        this.meshRenderer.sharedMaterial.color = new Color(baseColor.r, baseColor.g, baseColor.b, this.opacity);
    }
    */

    public void Show()
    {
        this.velocity = this.speed;
    }

    public void Hide()
    {
        this.velocity = -this.speed;
    }

    public void Stop()
    {
        this.velocity = 0.0f;
    }
}
