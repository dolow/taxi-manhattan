using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Article
{
    [Serializable]
    public struct Map
    {
        public int width;
        public int height;
        public int[] fields;
    }

    public class ArticleScene : MonoBehaviour
    {
        [SerializeField]
        private Tile tilePool;

        private Map map;

        private void Start()
        {
            TextAsset mapAsset = Resources.Load<TextAsset>("Article/ArticleField");
            this.map = JsonUtility.FromJson<Map>(mapAsset.text);

            for (int i = 0; i < this.map.fields.Length; i++)
            {
                int tileId = map.fields[i];
                Vector3 tilePos = new Vector3(i % map.width, 0.0f, -Mathf.Floor(i / map.width));
                Tile tile = GameObject.Instantiate<Tile>(this.tilePool);
                tile.SetTilePos(tileId);
                // tile.UpdateUV();
                tile.transform.position = tilePos;
                tile.gameObject.SetActive(true);
            }
        }
    }
}