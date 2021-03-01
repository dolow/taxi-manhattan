﻿using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainCamera : MonoBehaviour
{
    public enum Mode
    {
        Disabled,
        Orbit,
        Follow,
        Static,
    }

    private Orbit  orbit = null;
    private Follow follow = null;
    private Static statik = null;

    private Transform target = null;

    public float orbitSpeed {
        get
        {
            return this.orbit.speed;
        }
        set
        {
            this.orbit.speed = value;
        }
    }

    public Mode CameraMode
    {
        get
        {
            if (this.follow?.enabled == true) return Mode.Follow;
            if (this.orbit?.enabled  == true) return Mode.Orbit;
            if (this.statik?.enabled == true) return Mode.Static;
            return Mode.Disabled;
        }
        set
        {
            if (this.follow != null) this.follow.enabled = false;
            if (this.orbit != null)  this.orbit.enabled  = false;
            if (this.statik != null) this.statik.enabled = false;

            switch (value)
            {
                case Mode.Follow:
                    {
                        if (this.orbit != null)  this.orbit.enabled = false;
                        if (this.statik != null) this.statik.enabled = false;

                        if (this.follow == null)
                            this.follow = this.gameObject.AddComponent<Follow>();

                        this.follow.enabled = true;
                        break;
                    }
                case Mode.Orbit:
                    {
                        if (this.follow != null) this.follow.enabled = false;
                        if (this.statik != null) this.statik.enabled = false;

                        if (this.orbit == null)
                            this.orbit = this.gameObject.AddComponent<Orbit>();

                        this.orbit.enabled = true;
                        break;
                    }
                case Mode.Static:
                    {
                        if (this.follow != null) this.follow.enabled = false;
                        if (this.orbit  != null) this.orbit.enabled = false;

                        if (this.statik == null)
                            this.statik = this.gameObject.AddComponent<Static>();

                        this.statik.enabled = true;
                        break;
                    }
                case Mode.Disabled:
                    {
                        if (this.follow != null) this.follow.enabled = false;
                        if (this.orbit != null)  this.orbit.enabled  = false;
                        if (this.statik != null) this.statik.enabled = false;
                        break;
                    }
            }
        }
    }

    public float TargetDistance {
        get {
            return Vector3.Distance(this.transform.position, this.target.position);
        }
        set { }
    }

    private void Awake()
    {
        this.orbit  = this.GetComponent<Orbit>();
        this.follow = this.GetComponent<Follow>();
        this.statik = this.GetComponent<Static>();

        if (this.statik != null)
            this.CameraMode = Mode.Static;
        else if (this.follow != null)
            this.CameraMode = Mode.Follow;
        else if (this.orbit != null)
            this.CameraMode = Mode.Orbit;
        else
            this.CameraMode = Mode.Disabled;

        if (this.orbit != null)
            this.orbit.OnRotateFinished = this.OnRotateFinished;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
        this.orbit?.SetTarget(this.target);
        this.follow?.SetTarget(this.target);
    }

    public void RotateRight()
    {
        this.CameraMode = Mode.Orbit;
        this.orbit?.RotateRight();
    }
    public void RotateLeft()
    {
        this.CameraMode = Mode.Orbit;
        this.orbit?.RotateLeft();
    }

    private void OnRotateFinished()
    {
        this.CameraMode = Mode.Follow;
        this.follow?.RefreshDistance();
    }
}
