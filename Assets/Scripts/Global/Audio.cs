using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    public enum SE
    {
        PickUp,
        DropOff,
        PowerUp,
        Tutorial,
        Walk1,
        Walk2,
    }

    public static readonly Dictionary<SE, AudioClip> seRef = new Dictionary<SE, AudioClip>();

    public static void CacheAll()
    {
        Audio.seRef.Add(SE.PickUp,   Resources.Load<AudioClip>("Audio/Se/se_pickup"));
        Audio.seRef.Add(SE.DropOff,  Resources.Load<AudioClip>("Audio/Se/se_dropoff"));
        Audio.seRef.Add(SE.PowerUp,  Resources.Load<AudioClip>("Audio/Se/se_powerup"));
        Audio.seRef.Add(SE.Tutorial, Resources.Load<AudioClip>("Audio/Se/se_tutorial"));
        Audio.seRef.Add(SE.Walk1,     Resources.Load<AudioClip>("Audio/Se/se_walk1"));
        Audio.seRef.Add(SE.Walk2, Resources.Load<AudioClip>("Audio/Se/se_walk2"));
    }

    public static AudioClip GetSE(SE se)
    {
        if (!Audio.seRef.ContainsKey(se))
            Audio.CacheAll();

        return Audio.seRef[se];
    }
}
