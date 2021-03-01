using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SampleScene : MonoBehaviour
{
    public GameObject mapRoot = null;
    public FieldTile fieldPool = null;
    public MeshFilter tilePool = null;
    public FieldTile verticalFieldPool = null;
    public FieldCollider fieldCollierPool = null;
    public WalkerBehavior walkerPool = null;
    public ItemBehavior itemPool = null;
    public UIField uiField = null;
    public MainCamera mainCamera = null;

    public Text scoreText = null;
    public Text timeText = null;
    public GameObject resultPanel = null;
    public Text resultText = null;

    public float timeLimit = 180.0f;

    // TODO: be struct
    // game parameters with visible effect
    [SerializeField]
    private float moveSpeedItemValue = 2.0f;
    [SerializeField]
    private float cameraSpeedItemValue = 100.0f;

    private Map map = new Map();
    private WalkerBehavior mainWalker = null;

    private Game game = new Game();

    private Dictionary<int, ItemBehavior> placedItems = new Dictionary<int, ItemBehavior>();

    private float passedTime = 0.0f;
    private bool gameFinished = false;

    private void Start()
    {
        this.map = MapLoader.Load("Field2", this.mapRoot, this.tilePool, this.fieldPool, this.verticalFieldPool, this.fieldCollierPool, this.walkerPool, null);
        if (this.map.fields.Length == 0)
        {
            Debug.Log("could not load map");
            return;
        }

        this.uiField.OnRequestRotateRight = this.TryRotateCameraRight;
        this.uiField.OnRequestRotateLeft = TryRotateCameraLeft;

        FieldCollider[] colliders = this.mapRoot.GetComponentsInChildren<FieldCollider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            // TODO: overhead
            colliders[i].SetPhysicsRaycastHitCallback(this.TryWalkerWalk);
        }

        WalkerBehavior[] walkers = this.mapRoot.GetComponentsInChildren<WalkerBehavior>();
        for (int i = 0; i < walkers.Length; i++)
        {
            WalkerBehavior walker = walkers[i];
            if (walker.data.cameraFollow == true)
            {
                this.mainWalker = walker;
                this.mainCamera.SetTarget(this.mainWalker.transform);
            }
            walker.SetLookAtCamera(this.mainCamera.GetComponent<Camera>());
            walker.OnStopped = this.RetrieveTile;
        }

        this.game.SetMap(this.map, this.CurrentFloor());

        this.game.OnCalcPathDistanceRequested = this.GetDistance;
        this.game.OnConsumedItem = this.GetItem;
        this.game.OnSpawnedItem = this.PlaceItem;

        this.game.SpawnItem(Game.AddressItem.Passenger);
    }

    private void Update()
    {
        this.passedTime += Time.deltaTime;
        float leftTime = this.timeLimit - this.passedTime;
        if (this.gameFinished)
        {
            if (leftTime < -5.0f)
            {
                if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
                {
                    SceneManager.LoadScene("TitleScene");
                    return;
                }
            }
        }
        if (leftTime < 0.0f)
        {
            this.gameFinished = true;
            this.resultPanel.SetActive(true);
            this.resultText.text = "" + this.game.GetScore();
            return;
        }

        string min = "" + (int)Mathf.Floor(leftTime / 60.0f);
        string sec = "" + (int)Mathf.Floor(leftTime) % 60;
        
        this.timeText.text = min.PadLeft(2, '0') + ":" + sec.PadLeft(2, '0');
    }

    // TODO:
    private int CurrentFloor()
    {
        return 0;
    }

    private void RetrieveTile(int address)
    {
        this.game.ConsumeItem(address);
    }

    private void PlaceItem(Game.AddressItem item, int address)
    {
        ItemBehavior behavior = GameObject.Instantiate<ItemBehavior>(this.itemPool);
        Vector3 pos = this.map.addressToPos(address);
        behavior.transform.position = new Vector3(
            pos.x * MapLoader.FieldUnit.x + MapLoader.FieldUnit.x * 0.5f,
            pos.y * this.CurrentFloor() * MapLoader.FieldUnit.y + MapLoader.FieldUnit.y * 0.5f,
            pos.z * MapLoader.FieldUnit.z - MapLoader.FieldUnit.z * 0.5f
        );
        behavior.transform.parent = this.mapRoot.transform;
        behavior.SetSurface(item);
        behavior.SetLookAtCamera(this.mainCamera.GetComponent<Camera>());
        behavior.gameObject.SetActive(true);
        this.placedItems.Add(address, behavior);
    }

    private void GetItem(Game.AddressItem item, int address, int amount)
    {
        switch (item)
        {
            // logical things does nothing
            case Game.AddressItem.Passenger:
                {
                    this.mainWalker.PlaySe(Audio.GetSE(Audio.SE.PickUp));
                    break;
                }
            case Game.AddressItem.IncreaseSpawn:
                {
                    this.mainWalker.PlaySe(Audio.GetSE(Audio.SE.PowerUp));
                    break;
                }
            case Game.AddressItem.Destination:
                {
                    int score = this.game.GetScore();
                    this.scoreText.text = "Score:" + score;
                    this.mainWalker.PlaySe(Audio.GetSE(Audio.SE.DropOff));
                    break;
                }
            // scene has reponsibility for visible things
            case Game.AddressItem.MoveSpeed:
                {
                    this.mainWalker.walkSpeed += this.moveSpeedItemValue;
                    this.mainWalker.PlaySe(Audio.GetSE(Audio.SE.PowerUp));
                    break;
                }
            case Game.AddressItem.CameraSpeed:
                {
                    this.mainCamera.orbitSpeed += this.cameraSpeedItemValue;
                    this.mainWalker.PlaySe(Audio.GetSE(Audio.SE.PowerUp));
                    break;
                }
            case Game.AddressItem.None:        // noop
            case Game.AddressItem.Unavailable: // unexpected
            default: return;
        }

        if (this.placedItems.ContainsKey(address))
        {
            ItemBehavior behavior = this.placedItems[address];
            GameObject.Destroy(behavior.gameObject);
            this.placedItems.Remove(address);
        }
    }

    private int GetDistance(int origin, int dest)
    {
        int[] currentFloorTerrain = this.map.terrains[this.CurrentFloor()];
        List<int> route = Astar.Exec(origin, dest, currentFloorTerrain, this.map.width);
        return route.Count;
    }

    private void TryWalkerWalk(int x, int y, BaseEventData data)
    {
        if (this.gameFinished)
            return;
        
        // TODO: ignore collision between camera and walker
        if (this.mainWalker.IsWalking())
            return;

        int destAddress = y * this.map.width + x;
            
        int[] currentFloorTerrain = this.map.terrains[this.CurrentFloor()];
        List<int> route = Astar.Exec(this.mainWalker.data.address, destAddress, currentFloorTerrain, this.map.width);
        List<DirectionUtil.Direction> directions = DirectionUtil.AddressesToDirections(route, this.map.width, this.map.height);
        this.mainWalker.AppendWalkDirections(directions);

        switch (Rand.Next(10))
        {
            case 0: this.mainWalker.PlaySe(Audio.GetSE(Audio.SE.Walk1)); break;
            case 1: this.mainWalker.PlaySe(Audio.GetSE(Audio.SE.Walk2)); break;
            default: break;
        }
    }

    private void TryRotateCameraRight()
    {
        if (this.mainCamera.CameraMode == MainCamera.Mode.Follow)
            this.mainCamera.RotateRight();
    }
    private void TryRotateCameraLeft()
    {
        if (this.mainCamera.CameraMode == MainCamera.Mode.Follow)
            this.mainCamera.RotateLeft();
    }
}
