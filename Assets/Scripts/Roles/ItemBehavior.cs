using UnityEngine;

public class ItemBehavior : MonoBehaviour
{
    [SerializeField]
    private AtlasMaterial surface = null;

    public void SetLookAtCamera(Camera camera)
    {
        Billboard billboeard = this.surface.GetComponent<Billboard>();
        if (billboeard != null)
        {
            billboeard.targetCamera = camera;
        }
    }

    public void SetSurface(Game.AddressItem item)
    {
        if (this.surface == null)
            return;

        int index = -1;

        switch (item)
        {
            case Game.AddressItem.Passenger: index = 0; break;
            case Game.AddressItem.Destination: index = 1; break;
            case Game.AddressItem.MoveSpeed: index = 2; break;
            case Game.AddressItem.CameraSpeed: index = 3; break;
            case Game.AddressItem.IncreaseSpawn: index = 4; break;
            case Game.AddressItem.None:
            case Game.AddressItem.Unavailable:
            default: break;
        }

        if (index == -1)
            return;

        this.surface.initialIndex = index;
        this.surface.maxIndex = index;
    }
}
