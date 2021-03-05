using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Article
{
    [Serializable]
    public struct Map
    {
        public int width;
        public int height;
        public int[] fields;
        public int[] terrain;
    }

    public class ArticleScene : MonoBehaviour
    {
        [SerializeField]
        private Article.Tile tilePool = null;
        [SerializeField]
        private Article.Walker walker = null;

        private Map map;

        private void Start()
        {
            TextAsset mapAsset = Resources.Load<TextAsset>("Article/ArticleField");
            this.map = JsonUtility.FromJson<Map>(mapAsset.text);
            this.map.height = (int)Mathf.Floor(this.map.fields.Length / this.map.width) + 1;

            Dictionary<int, List<MeshFilter>> filters = new Dictionary<int, List<MeshFilter>>();
            for (int i = 0; i < this.map.fields.Length; i++)
            {
                int tileId = map.fields[i];
                Vector3 tilePos = new Vector3(i % map.width, 0.0f, -Mathf.Floor(i / map.width));
                Tile tile = GameObject.Instantiate<Tile>(this.tilePool);

                if (!filters.ContainsKey(tileId))
                {
                    filters.Add(tileId, new List<MeshFilter>());
                }

                MeshFilter meshFilter = tile.GetComponent<MeshFilter>();
                meshFilter.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
                meshFilter.transform.position = tilePos;
                filters[tileId].Add(tile.GetComponent<MeshFilter>());
            }

            foreach (KeyValuePair<int, List<MeshFilter>> kv in filters)
            {
                List<MeshFilter> filterList = kv.Value;
                CombineInstance[] combine = new CombineInstance[filterList.Count];
                for (int i = 0; i < filterList.Count; i++)
                {
                    combine[i].mesh = filterList[i].sharedMesh;
                    combine[i].transform = filterList[i].transform.localToWorldMatrix;
                    combine[i].subMeshIndex = 0;
                    GameObject.Destroy(filterList[i].gameObject);
                }

                Article.Tile combinedTile = GameObject.Instantiate<Article.Tile>(this.tilePool);
                MeshFilter filter = combinedTile.GetComponent<MeshFilter>();
                filter.mesh = new Mesh();
                filter.mesh.CombineMeshes(combine);
                combinedTile.SetTilePos(kv.Key);

                MeshCollider collider = combinedTile.GetComponent<MeshCollider>();
                collider.sharedMesh = filter.sharedMesh;

                combinedTile.gameObject.SetActive(true);

                combinedTile.InitCollider(this.OnPhysicsRaycasterHit);
            }

            this.walker.index = 0;
            Vector3 tileScale = this.tilePool.transform.localScale;
            int x = (int)Mathf.Floor(tileScale.x * 0.5f);
            int y = (int)-Mathf.Floor(tileScale.y * 0.5f);
        }

        private void OnPhysicsRaycasterHit(Tile tile, BaseEventData data)
        {
            Vector3 tileScale = tile.transform.localScale;
            Vector3 hitPos = ((PointerEventData)data).pointerCurrentRaycast.worldPosition;
            int x = (int)Mathf.Floor(hitPos.x + tileScale.x * 0.5f);
            int y = (int)-Mathf.Floor(hitPos.z + tileScale.y * 0.5f);

            this.TryWalk(x, y);
        }

        private void TryWalk(int x, int y)
        {
            if (this.walker.IsWalking())
            {
                return;
            }

            int destIndex = x + y * this.map.width;

            List<int> route = Astar.Exec(this.walker.index, destIndex, this.map.terrain, this.map.width);
            List<Vector3> directions = this.AddressesToDirections(route, this.map.width, this.map.height);
            this.walker.AppendWalkDirections(directions);

            this.walker.index = destIndex;
        }

        public List<Vector3> AddressesToDirections(List<int> addresses, int width, int height)
        {
            List<Vector3> directions = new List<Vector3>();

            for (int i = 0; i < addresses.Count - 1; i++)
            {
                int address = addresses[i];
                int nextAddress = addresses[i + 1];
                Vector3 direction;
                if (nextAddress == address + width)
                    direction = new Vector3(0.0f, 0.0f, -1.0f);
                else if (nextAddress == address - width)
                    direction = new Vector3(0.0f, 0.0f, 1.0f);
                else if (nextAddress == address + 1)
                    direction = new Vector3(1.0f, 0.0f, 0.0f);
                else if (nextAddress == address - 1)
                    direction = new Vector3(-1.0f, 0.0f, 0.0f);
                else
                    continue;
                directions.Add(direction);
            }

            return directions;
        }
    }
}