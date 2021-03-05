using UnityEngine;

namespace Article
{
    public class Tile : MonoBehaviour
    {
        [SerializeField]
        private Vector2 tileCount = Vector2.zero;
        [SerializeField]
        private Vector2 tilePos = Vector2.zero;

        private void Start()
        {
            this.UpdateUV();
        }

        public void SetTilePos(int index)
        {
            Vector2 pos = new Vector2(index % this.tileCount.x, Mathf.Floor(index / this.tileCount.x));
            this.tilePos = pos;
        }

        public void UpdateUV()
        {
            Renderer renderer = this.GetComponent<Renderer>();

            Vector2 tileScale = new Vector2(1.0f / this.tileCount.x, 1.0f / this.tileCount.y);
            Vector2 offset = new Vector2(this.tilePos.x * tileScale.x, 1.0f - tileScale.y - this.tilePos.y * tileScale.y);
            renderer.material.SetTextureScale("_MainTex", tileScale);
            renderer.material.SetTextureOffset("_MainTex", offset);
        }
    }
}