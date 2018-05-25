using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakePointManagerControllable : Controllable {

    public FakePointManager manager;

    [Header("Network settings")]
    [OSCProperty]
    public string TargetIP;
    [OSCProperty]
    public int TargetPort;

    [Header("Area settings")]
    [OSCProperty]
    public int Width;
    [OSCProperty]
    public int Height;

    [Header("Points settings")]
    [OSCProperty]
    public int NbPoints;
    [OSCProperty]
    public float PointSizeX;
    [OSCProperty]
    public float PointSizeY;

    [OSCProperty]
    [Range(0.0f,10.0f)]
    public float Speed;

    [OSCMethod]
    public void Clear()
    {
        NbPoints = 0; //TODO : remove this line, shouldn't be required
        manager.Clear();
    }
	public override void Update () {
        //base.Update();
        manager.TargetIP = TargetIP;
        manager.TargetPort = TargetPort;


        manager.NbPoints = NbPoints;
        manager.Speed = Speed;
        manager.PointSize = new Vector2(PointSizeX / Width, PointSizeY / Height);
        if (manager.Height != Height || manager.Width != Width)
        {
            Screen.SetResolution(Width, Height, false);
            float safeWidth = Screen.safeArea.width;
            float safeHeight = Screen.safeArea.height;
            if (Height > Width)
                Screen.SetResolution((int)safeWidth, (int)(safeWidth * ((float)Height / (float)Width)), false);
            else
                Screen.SetResolution((int)(safeHeight * ((float)Width / (float)Height)), (int)safeHeight, false);
        }
        manager.Width = Width;
        manager.Height = Height;
    }
}
