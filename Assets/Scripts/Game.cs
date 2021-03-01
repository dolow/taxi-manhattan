using System;
using System.Collections.Generic;

public class Game
{
    public enum AddressItem
    {
        None,
        Passenger,
        Destination,
        MoveSpeed,
        CameraSpeed,
        IncreaseSpawn,
        Unavailable,
    };

    public delegate int CalcPathDistance(int origin, int dest);
    public delegate void SpawnedItem(AddressItem item, int address);
    public delegate void ConsumedItem(AddressItem item, int address, int amount);

    public CalcPathDistance OnCalcPathDistanceRequested = null;
    public SpawnedItem OnSpawnedItem = null;
    public ConsumedItem OnConsumedItem = null;

    private int minTravelDistance = 6;

    private readonly Dictionary<AddressItem, int> inventry = new Dictionary<AddressItem, int>() {
        { AddressItem.Passenger, 0 },
        { AddressItem.Destination, 0 },
        { AddressItem.MoveSpeed, 0 },
        { AddressItem.CameraSpeed, 0 },
        { AddressItem.IncreaseSpawn, 1 },
    };

    private List<AddressItem> addressItems = new List<AddressItem>();

    public int GetScore()
    {
        return this.GetInventry(AddressItem.Destination);
    }

    public int GetInventry(AddressItem item)
    {
        return this.inventry[item];
    }

    public void SetMinTravelDistance(int distance)
    {
        this.minTravelDistance = distance;
    }

    public void SetMap(Map map, int floor)
    {
        int[] terrain = map.terrains[floor];
        
        for (int i = 0; i < terrain.Length; i++)
        {
            this.addressItems.Add((terrain[i] == 1) ? AddressItem.None : AddressItem.Unavailable);
        }
    }

    public bool SpawnItem(AddressItem item)
    {
        int address = this.RandomAvailableAddress();
        if (address == -1)
            return false;
        
        this.addressItems[address] = item;
        this.OnSpawnedItem?.Invoke(item, address);
        return true;
    }
    public bool SpawnItem(AddressItem item, int address)
    {
        if (this.addressItems[address] != AddressItem.None)
            return false;
        
        this.addressItems[address] = item;
        this.OnSpawnedItem?.Invoke(item, address);
        return true;
    }

    public void ConsumeItem(int address)
    {
        AddressItem item = this.addressItems[address];
        
        switch (item)
        {
            case AddressItem.Passenger:
                {
                    this.addressItems[address] = AddressItem.None;
                    this.inventry[AddressItem.Passenger]++;

                    int dest = -1;
                    if (this.OnCalcPathDistanceRequested == null)
                    {
                        dest = this.RandomAvailableAddress();
                    }
                    else
                    {
                        int retry = 5;
                        int distance = -1;
                        while (distance < this.minTravelDistance)
                        {
                            int lastDistance = distance;
                            int newDest  = this.RandomAvailableAddress();
                            distance = this.OnCalcPathDistanceRequested(address, dest);
                            if (distance > lastDistance)
                                dest = newDest;
                            // give up
                            if (--retry <= 0)
                                break;
                        }
                    }

                    if (!this.SpawnItem(AddressItem.Destination, dest))
                    {
                        // parallel things may went bad
                    }

                    break;
                }
            case AddressItem.Destination:
                {
                    if (this.inventry[AddressItem.Passenger] > 0)
                    {
                        this.inventry[AddressItem.Passenger]--;
                        this.inventry[AddressItem.Destination]++;
                        this.SpawnItem(this.RandomEnhanceItem());
                    }

                    int placedPassengers = 0;
                    for (int i = 0; i < this.addressItems.Count; i++)
                    {
                        if (this.addressItems[i] == AddressItem.Passenger)
                            placedPassengers++;
                    }

                    for (int i = placedPassengers; i < this.inventry[AddressItem.IncreaseSpawn]; i++)
                    {
                        this.SpawnItem(AddressItem.Passenger);
                    }

                    this.addressItems[address] = AddressItem.None;

                    break;
                }
            case AddressItem.MoveSpeed:
            case AddressItem.CameraSpeed:
            case AddressItem.IncreaseSpawn:
                {
                    this.addressItems[address] = AddressItem.None;
                    this.inventry[item]++;
                    break;
                }
            case AddressItem.None:        // noop
            case AddressItem.Unavailable: // unexpected
            default: return;
        }

        this.OnConsumedItem?.Invoke(item, address, this.inventry[item]);
    }

    private int RandomAvailableAddress()
    {
        List<int> availabaleAddresses = new List<int>();
        for (int i = 0; i < this.addressItems.Count; i++)
        {
            AddressItem item = this.addressItems[i];
            if (item == AddressItem.None)
                availabaleAddresses.Add(i);
        }

        if (availabaleAddresses.Count == 0)
            return -1;

        int index = Rand.Next(availabaleAddresses.Count - 1);
        int address = availabaleAddresses[index];
        return address;
    }
    private AddressItem RandomEnhanceItem()
    {
        switch (Rand.Next(3))
        {
            case 0:  return AddressItem.MoveSpeed;
            case 1:  return AddressItem.CameraSpeed;
            default: return AddressItem.IncreaseSpawn;
        }
    }
}
