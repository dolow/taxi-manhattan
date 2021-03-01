using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;
using System;

[Serializable]
public struct Walker
{
    public static Walker Empty = new Walker(-1, -1, -1, false);

    public int type;
    public int direction;
    public int address;
    public bool cameraFollow;

    public Walker(int type, int direction, int address, bool cameraFollow)
    {
        this.type = type;
        this.direction = direction;
        this.address = address;
        this.cameraFollow = cameraFollow;
    }
}

[Serializable]
public struct RawMap
{
    public string name;
    public int width;
    public int height;
    public int floors;
    public int[] fields0;
    public int[] fields1;
    public int[] fields2;
    public string[] verticalFields0;
    public string[] verticalFields1;
    public string[] verticalFields2;
    public int[] terrains0;
    public int[] terrains1;
    public int[] terrains2;

    public Walker walker;
}

[Serializable]
public struct Map
{
    public string name;
    public int width;
    public int height;
    public int[][] fields;
    public int[][][] verticalFields;
    public int[][] terrains;

    public Walker[] walkers;

    public Vector3 addressToPos(int address)
    {
        return new Vector3(address % this.width, 0.0f, -(int)Mathf.Floor(address / this.width));
    }
}

public class MapLoader
{
    public const int EmptyField = 0;
    public static string MapJsonDirectory = "Maps";
    public static string MapFieldMaterialDirectory = "Materials/Field";

    public static Vector3 FieldUnit = new Vector3(1.0f, 1.0f, 1.0f);
    public static Quaternion SurfaceUp = Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f));
    public static Dictionary<DirectionUtil.Direction, Quaternion> SurfaceDirection = new Dictionary<DirectionUtil.Direction, Quaternion>() {
        { DirectionUtil.Direction.South, Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f)) },
        { DirectionUtil.Direction.North, Quaternion.Euler(new Vector3(0.0f, 180.0f, 0.0f)) },
        { DirectionUtil.Direction.East,  Quaternion.Euler(new Vector3(0.0f, -90.0f, 0.0f)) },
        { DirectionUtil.Direction.West,  Quaternion.Euler(new Vector3(0.0f, 90.0f, 0.0f)) },
    };
    public static Dictionary<DirectionUtil.Direction, Vector3> SurfaceOffset = new Dictionary<DirectionUtil.Direction, Vector3>() {
        { DirectionUtil.Direction.South, new Vector3(0.0f,  0.0f, -0.5f) },
        { DirectionUtil.Direction.North, new Vector3(0.0f,  0.0f, 0.5f)  },
        { DirectionUtil.Direction.East,  new Vector3(0.5f,  0.0f, 0.0f)  },
        { DirectionUtil.Direction.West,  new Vector3(-0.5f, 0.0f, 0.0f)  },
    };

    private static Dictionary<string, Material> materialCache = new Dictionary<string, Material>();
    private static Map currentMap;

    public static Map CurrentMap()
    {
        return MapLoader.currentMap;
    }

    public static Map Load(
        string name,
        GameObject mapRoot,
        MeshFilter tilePool,
        FieldTile fieldPrototype,
        FieldTile verticalFieldPrototype,
        FieldCollider fieldColliderPrototype,
        WalkerBehavior walkerPrototype,
        Action<Map, MeshFilter, int, int> onEachTile
    )
    {
        string path = Path.Combine(MapLoader.MapJsonDirectory, name);
        TextAsset asset = Resources.Load<TextAsset>(path);
        
        if (asset == null)
        {
            Debug.Log("could not load map file: " + path);
            return new Map();
        }

        // JsonUtility does not support nested array
        
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Map));
        RawMap raw = JsonUtility.FromJson<RawMap>(asset.text);
        MapLoader.RawMapToMap(raw, out MapLoader.currentMap);
        

        MapLoader.currentMap.height = (int)Mathf.Floor(MapLoader.currentMap.terrains[0].Length / MapLoader.currentMap.width);

        MapLoader.InstantiateFields(mapRoot, tilePool, fieldPrototype, verticalFieldPrototype, fieldColliderPrototype, onEachTile);
        MapLoader.InstantiateWalkerBehaviors(mapRoot, walkerPrototype);

        return MapLoader.currentMap;
    }

    // TODO: I really want DataContractJsonSerializer for WebGL
    private static void RawMapToMap(RawMap raw, out Map map)
    {
        map = new Map();
        map.name = raw.name;
        map.width = raw.width;
        map.height = raw.height;
        map.walkers = new Walker[] { raw.walker };

        map.fields = new int[raw.floors][];

        // TODO: reflection
        {
            int[] floor = new int[raw.fields0.Length];
            for (int i = 0; i < raw.fields0.Length; i++)
                floor[i] = raw.fields0[i];
            map.fields[0] = floor;
        }
        {
            int[] floor = new int[raw.fields1.Length];
            for (int i = 0; i < raw.fields1.Length; i++)
                floor[i] = raw.fields1[i];
            map.fields[1] = floor;
        }
        {
            int[] floor = new int[raw.fields2.Length];
            for (int i = 0; i < raw.fields2.Length; i++)
                floor[i] = raw.fields2[i];
            map.fields[2] = floor;
        }

        map.terrains = new int[raw.floors][];
        {
            int[] floor = new int[raw.terrains0.Length];
            for (int i = 0; i < raw.terrains0.Length; i++)
                floor[i] = raw.terrains0[i];
            map.terrains[0] = floor;
        }
        {
            int[] floor = new int[raw.terrains1.Length];
            for (int i = 0; i < raw.terrains1.Length; i++)
                floor[i] = raw.terrains1[i];
            map.terrains[1] = floor;
        }
        {
            int[] floor = new int[raw.terrains2.Length];
            for (int i = 0; i < raw.terrains2.Length; i++)
                floor[i] = raw.terrains2[i];
            map.terrains[2] = floor;
        }

        map.verticalFields = new int[raw.floors][][];
        {
            int[][] floor = new int[raw.verticalFields0.Length][];
            for (int i = 0; i < raw.verticalFields0.Length; i++)
            {
                string square = raw.verticalFields0[i];
                string[] aspectStrs = square.Split(',');
                
                int[] aspects = new int[aspectStrs.Length];
                for (int j = 0; j < aspectStrs.Length; j++)
                {
                    if (aspectStrs[j] == "")
                        break;
                    aspects[j] = Int32.Parse(aspectStrs[j]);
                }
                floor[i] = aspects;
            }
            map.verticalFields[0] = floor;
        }
        {
            int[][] floor = new int[raw.verticalFields1.Length][];
            for (int i = 0; i < raw.verticalFields1.Length; i++)
            {
                string square = raw.verticalFields1[i];
                string[] aspectStrs = square.Split(',');

                int[] aspects = new int[aspectStrs.Length];
                for (int j = 0; j < aspectStrs.Length; j++)
                {
                    if (aspectStrs[j] == "")
                        break;
                    aspects[j] = Int32.Parse(aspectStrs[j]);
                }
                floor[i] = aspects;
            }
            map.verticalFields[1] = floor;
        }
        {
            int[][] floor = new int[raw.verticalFields2.Length][];
            for (int i = 0; i < raw.verticalFields2.Length; i++)
            {
                string square = raw.verticalFields2[i];
                string[] aspectStrs = square.Split(',');

                int[] aspects = new int[aspectStrs.Length];
                for (int j = 0; j < aspectStrs.Length; j++)
                {
                    if (aspectStrs[j] == "")
                        break;
                    aspects[j] = Int32.Parse(aspectStrs[j]);
                }
                floor[i] = aspects;
            }
            map.verticalFields[2] = floor;
        }
    }

    private static GameObject InstantiateFields(
        GameObject mapRoot,
        MeshFilter tilePool,
        FieldTile fieldPrototype,
        FieldTile verticalPrototype,
        FieldCollider fieldColliderPrototype,
        Action<Map, MeshFilter, int, int> onEachTile
    )
    {
        Map map = MapLoader.currentMap;

        mapRoot.isStatic = true;

        int fieldAspects = 4;
        int width = map.width;

        int floors = (map.fields.Length > map.verticalFields.Length)
            ? map.fields.Length
            : map.verticalFields.Length;

        Dictionary<int, List<MeshFilter>> meshFilterDictionary = new Dictionary<int, List<MeshFilter>>();
        Dictionary<int, List<MeshFilter>> verticalMeshFilterDictionary = new Dictionary<int, List<MeshFilter>>();

        // TODO: reduce draw call, RT? Mesh Combine?
        for (int floor = 0; floor < floors; floor++)
        {
            int[] floorField = (map.fields.Length > floor)
                ? map.fields[floor]
                : new int[] { };

            int[][] floorVerticalField = (map.verticalFields.Length > floor)
                ? map.verticalFields[floor]
                : new int[][]{ };

            int indexes = (floorField.Length >= floorVerticalField.Length)
                ? floorField.Length
                : floorVerticalField.Length;

            for (int i = 0; i < indexes; i++)
            {
                int col = i % width;
                int row = (int)Mathf.Floor(i / width);

                do // collect MeshFilters of horizontal fields by same appearance
                {
                    if (floorField.Length <= i)
                        break;

                    int fieldId = floorField[i];
                    if (fieldId == MapLoader.EmptyField)
                        break;

                    MeshFilter filter = GameObject.Instantiate<MeshFilter>(tilePool);
                    filter.transform.parent = mapRoot.transform.transform;
                    filter.transform.rotation = MapLoader.SurfaceUp;
                    filter.transform.position = new Vector3(col * FieldUnit.x + FieldUnit.x * 0.5f, (float)floor * FieldUnit.y, -row * FieldUnit.z - FieldUnit.z * 0.5f);

                    if (!meshFilterDictionary.ContainsKey(fieldId))
                        meshFilterDictionary.Add(fieldId, new List<MeshFilter> { });

                    List<MeshFilter> filters = meshFilterDictionary[fieldId];
                    filters.Add(filter);
                    onEachTile?.Invoke(MapLoader.currentMap, filter, col, row);
                } while (false);

                do // collect MeshFilters of vertical fields by same appearance
                {
                    if (floorVerticalField.Length <= i)
                        break;

                    int[] verticalFieldDirections = floorVerticalField[i];
                    if (verticalFieldDirections.Length != fieldAspects)
                        break;

                    for (int directionNumber = 0; directionNumber < verticalFieldDirections.Length; directionNumber++)
                    {
                        int verticalFieldId = verticalFieldDirections[directionNumber];
                        if (verticalFieldId == MapLoader.EmptyField)
                            continue;

                        DirectionUtil.Direction direction = (DirectionUtil.Direction)directionNumber;

                        MeshFilter verticalFilter = GameObject.Instantiate<MeshFilter>(tilePool);
                        verticalFilter.transform.parent = mapRoot.transform.transform;
                        verticalFilter.transform.rotation = MapLoader.SurfaceDirection[direction];
                        verticalFilter.transform.position = new Vector3(col * FieldUnit.x + 0.5f, ((float)floor + 0.5f) * FieldUnit.y, -row * FieldUnit.z - 0.5f) + MapLoader.SurfaceOffset[direction];

                        if (!verticalMeshFilterDictionary.ContainsKey(verticalFieldId))
                            verticalMeshFilterDictionary.Add(verticalFieldId, new List<MeshFilter> { });

                        List<MeshFilter> verticalFilters = verticalMeshFilterDictionary[verticalFieldId];
                        verticalFilters.Add(verticalFilter);
                    }
                } while (false);
            }

            FieldCollider fieldCollider = GameObject.Instantiate<FieldCollider>(fieldColliderPrototype);
            float height = Mathf.Floor(map.fields[0].Length / width);
            fieldCollider.Init(width, height, floor);
            fieldCollider.transform.parent = mapRoot.transform;
            // TODO:
            fieldCollider.gameObject.SetActive(floor == 0);
        }

        // combine meshes
        {
            foreach (KeyValuePair<int, List<MeshFilter>> kv in meshFilterDictionary)
            {
                List<MeshFilter> filters = kv.Value;
                CombineInstance[] combine = new CombineInstance[filters.Count];
                for (int i = 0; i < filters.Count; i++)
                {
                    combine[i].mesh = filters[i].sharedMesh;
                    combine[i].transform = filters[i].transform.localToWorldMatrix;
                    combine[i].subMeshIndex = 0;
                    GameObject.Destroy(filters[i].gameObject);
                }

                FieldTile tile = GameObject.Instantiate<FieldTile>(fieldPrototype);
                tile.InitCombinedFloor(combine, kv.Key - 1);
                tile.transform.parent = mapRoot.transform;
            }

            foreach (KeyValuePair<int, List<MeshFilter>> kv in verticalMeshFilterDictionary)
            {
                List<MeshFilter> filters = kv.Value;
                CombineInstance[] combine = new CombineInstance[filters.Count];
                for (int i = 0; i < filters.Count; i++)
                {
                    combine[i].mesh = filters[i].sharedMesh;
                    combine[i].transform = filters[i].transform.localToWorldMatrix;
                    combine[i].subMeshIndex = 0;
                    GameObject.Destroy(filters[i].gameObject);
                }

                FieldTile tile = GameObject.Instantiate<FieldTile>(verticalPrototype);
                tile.InitCombinedFloor(combine, kv.Key - 1);
                tile.gameObject.AddComponent<Terrain>();
                tile.transform.parent = mapRoot.transform;

                // skip this if scene allows penetrate path finding raycast
                MeshCollider collider = tile.GetComponent<MeshCollider>();
                collider.sharedMesh = tile.GetComponent<MeshFilter>().sharedMesh;
                /*
                CombineInstance[] rcombine = new CombineInstance[filters.Count];
                for (int i = 0; i < filters.Count; i++)
                {
                    Vector3 euler = rcombine[i].transform.rotation.eulerAngles;
                    rcombine[i].mesh = filters[i].sharedMesh;
                    filters[i].transform.rotation = Quaternion.Euler(new Vector3(euler.x, euler.y + 180.0f, euler.z));
                    rcombine[i].transform = filters[i].transform.localToWorldMatrix;
                    rcombine[i].subMeshIndex = 0;
                    GameObject.Destroy(filters[i].gameObject);
                }

                FieldTile rtile = GameObject.Instantiate<FieldTile>(verticalPrototype);
                rtile.InitCombinedFloor(rcombine, kv.Key - 1);
                rtile.gameObject.AddComponent<Terrain>();
                rtile.transform.parent = mapRoot.transform;

                // skip this if scene allows penetrate path finding raycast
                MeshCollider rcollider = rtile.GetComponent<MeshCollider>();
                rcollider.sharedMesh = rtile.GetComponent<MeshFilter>().sharedMesh;
                */
            }
        }

        return mapRoot;
    }

    private static GameObject InstantiateWalkerBehaviors(GameObject mapRoot, WalkerBehavior prototype)
    {
        Map map = MapLoader.currentMap;

        for (int i = 0; i < map.walkers.Length; i++)
        {
            Walker data = map.walkers[i];
            GameObject go = GameObject.Instantiate<GameObject>(prototype.gameObject);
            WalkerBehavior walker = go.GetComponent<WalkerBehavior>();
            walker.data = data;
            walker.transform.parent = mapRoot.transform;
            walker.transform.position = new Vector3(data.address % map.width + 0.5f, 0.5f, -Mathf.Floor(data.address / map.width) - 0.5f);

            go.SetActive(true);
        }

        return mapRoot;
    }
}
