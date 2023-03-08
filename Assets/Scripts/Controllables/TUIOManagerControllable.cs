using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class TUIOManagerControllable : Controllable
{
    [Header("OUTPUT SETTINGS")]
    [OSCProperty(TargetList = "TUIOPresets")]
    public string preset;

    [OSCProperty]
    public int outputScene;
    [OSCProperty]
    public int TUIOPort;
    [OSCProperty]
    public string outputIP;

    [Header("TUIO SETTINGS")]
    [OSCProperty(TargetList = "TUIODimensions")]
    public string dimension;

    [OSCProperty(TargetList = "TUIODescriptors")]
    public string descriptor;

    private const String descriptorParameterName = "descriptor"; // if you change the variable name above do not forget to also change this name
    public List<string> TUIODescriptors = new List<string>();

    private const String presetParameterName = "preset"; // if you change the variable name above do not forget to also change this name
    public List<string> TUIOPresets = new List<string>();

    private const String dimensionParameterName = "dimension"; // if you change the variable name above do not forget to also change this name
    public List<string> TUIODimensions = new List<string>();

    private const String outputSceneParameterName = "outputScene";
    private const String TUIOPortParameterName = "TUIOPort";

    public override void Awake()
    {
        (TargetScript as TUIOManager).dimension = (TargetScript as TUIOManager).TUIODimension.ToString();
        (TargetScript as TUIOManager).descriptor = (TargetScript as TUIOManager).TUIODescriptor.ToString();
        InitialiseParameters();

        (TargetScript as TUIOManager).preset = (TargetScript as TUIOManager).TUIOPreset.ToString();
        TUIOPresets.Add(TUIOManager.AugmentaTUIOPreset.NONE.ToString());
        TUIOPresets.Add(TUIOManager.AugmentaTUIOPreset.NOTCH.ToString());
        TUIOPresets.Add(TUIOManager.AugmentaTUIOPreset.TOUCHDESIGNER.ToString());
        TUIOPresets.Add(TUIOManager.AugmentaTUIOPreset.MINIMAL.ToString());
        TUIOPresets.Add(TUIOManager.AugmentaTUIOPreset.BEST.ToString());

        base.Awake();
    }

    public override void OnUiValueChanged(string name)
    {
        if (name == presetParameterName)
        {
            SendEmptyMessageOnAllAddresses();
            (TargetScript as TUIOManager).TUIOPreset = (TUIOManager.AugmentaTUIOPreset)Enum.Parse(typeof(TUIOManager.AugmentaTUIOPreset), preset);
            ChangedParameterPreset();
        }

        if (name == dimensionParameterName || name == descriptorParameterName)
        {
            SendEmptyMessageOnAllAddresses();
            (TargetScript as TUIOManager).TUIODimension = (TUIOManager.AugmentaTUIODimension)Enum.Parse(typeof(TUIOManager.AugmentaTUIODimension), dimension);
            (TargetScript as TUIOManager).TUIODescriptor = (TUIOManager.AugmentaTUIODescriptor)Enum.Parse(typeof(TUIOManager.AugmentaTUIODescriptor), descriptor);
        }

        if (name == TUIOPortParameterName)
        {
            if ((TargetScript as TUIOManager).TUIOPreset == TUIOManager.AugmentaTUIOPreset.TOUCHDESIGNER)
            {
                (TargetScript as TUIOManager).outputScene = TUIOPort;
            }
        }

        if (name == outputSceneParameterName)
        {
            if ((TargetScript as TUIOManager).TUIOPreset == TUIOManager.AugmentaTUIOPreset.TOUCHDESIGNER)
            {
                (TargetScript as TUIOManager).TUIOPort = outputScene;
            }
        }

        base.OnUiValueChanged(name);
    }

    public override void OnScriptValueChanged(string name)
    {
        if (name == presetParameterName)
        {
            preset = (TargetScript as TUIOManager).TUIOPreset.ToString();
            ChangedParameterPreset();
        }

        if (name == dimensionParameterName || name == descriptorParameterName)
        {
            dimension = (TargetScript as TUIOManager).TUIODimension.ToString();
            descriptor = (TargetScript as TUIOManager).TUIODescriptor.ToString();
        }

        base.OnScriptValueChanged(name);
    }

    private void SendEmptyMessageOnAllAddresses()
    {
        foreach (int i in Enum.GetValues(typeof(TUIOManager.AugmentaTUIODimension)))//for (int i = 0; i < 3; i++)
        {
            foreach (int j in Enum.GetValues(typeof(TUIOManager.AugmentaTUIODescriptor)))//for (int j = 0; j < 3; j++)
            {
                string addr = (TargetScript as TUIOManager).GetAddressTUIO((TUIOManager.AugmentaTUIODescriptor)j, (TUIOManager.AugmentaTUIODimension)i);
                var msgAlive = new OSCMessage(addr);

                msgAlive.Append("alive");
                (TargetScript as TUIOManager).SendAugmentaMessage(msgAlive, "TUIO");
                var msgFseq = new OSCMessage(addr);

                msgFseq.Append("fseq");
                msgFseq.Append(Time.frameCount + 1);
                (TargetScript as TUIOManager).SendAugmentaMessage(msgFseq, "TUIO");
            }
        }
    }

    private void ChangedParameterPreset()
    {
        switch ((TargetScript as TUIOManager).TUIOPreset)
        {
            case TUIOManager.AugmentaTUIOPreset.NONE:
                
                InitialiseParameters();

               (TargetScript as TUIOManager).dimension = (TargetScript as TUIOManager).TUIODimension.ToString();
               (TargetScript as TUIOManager).descriptor = (TargetScript as TUIOManager).TUIODescriptor.ToString();

                break;

            case TUIOManager.AugmentaTUIOPreset.TOUCHDESIGNER:

                if ((TargetScript as TUIOManager).TUIOPort != (TargetScript as TUIOManager).outputScene)
                {
                    (TargetScript as TUIOManager).outputScene = (TargetScript as TUIOManager).TUIOPort;
                }
                
                InitialiseParameters();

                (TargetScript as TUIOManager).TUIODimension = TUIOManager.AugmentaTUIODimension.TUIO25D;
                (TargetScript as TUIOManager).TUIODescriptor = TUIOManager.AugmentaTUIODescriptor.BLOB;
                (TargetScript as TUIOManager).dimension = (TargetScript as TUIOManager).TUIODimension.ToString();
                (TargetScript as TUIOManager).descriptor = (TargetScript as TUIOManager).TUIODescriptor.ToString();

                break;

            case TUIOManager.AugmentaTUIOPreset.NOTCH:

                if ((TargetScript as TUIOManager).TUIOPort == (TargetScript as TUIOManager).outputScene)
                {
                    (TargetScript as TUIOManager).TUIOPort = 3333;
                    (TargetScript as TUIOManager).outputScene = 9001;
                }
                
                InitialiseParameters();
                TUIODimensions.Remove(TUIOManager.AugmentaTUIODimension.TUIO25D.ToString());
                TUIODimensions.Remove(TUIOManager.AugmentaTUIODimension.TUIO3D.ToString());

                (TargetScript as TUIOManager).TUIODimension = TUIOManager.AugmentaTUIODimension.TUIO2D;
                (TargetScript as TUIOManager).TUIODescriptor = TUIOManager.AugmentaTUIODescriptor.OBJECT;
                (TargetScript as TUIOManager).dimension = (TargetScript as TUIOManager).TUIODimension.ToString();
                (TargetScript as TUIOManager).descriptor = (TargetScript as TUIOManager).TUIODescriptor.ToString();

                break;

            case TUIOManager.AugmentaTUIOPreset.MINIMAL:

                InitialiseParameters();
                TUIODimensions.Remove(TUIOManager.AugmentaTUIODimension.TUIO25D.ToString());
                TUIODimensions.Remove(TUIOManager.AugmentaTUIODimension.TUIO3D.ToString());

                TUIODescriptors.Remove(TUIOManager.AugmentaTUIODescriptor.OBJECT.ToString());
                TUIODescriptors.Remove(TUIOManager.AugmentaTUIODescriptor.BLOB.ToString());

                (TargetScript as TUIOManager).TUIODimension = TUIOManager.AugmentaTUIODimension.TUIO2D;
                (TargetScript as TUIOManager).TUIODescriptor = TUIOManager.AugmentaTUIODescriptor.CURSOR;
                (TargetScript as TUIOManager).dimension = (TargetScript as TUIOManager).TUIODimension.ToString();
                (TargetScript as TUIOManager).descriptor = (TargetScript as TUIOManager).TUIODescriptor.ToString();

                break;

            case TUIOManager.AugmentaTUIOPreset.BEST:

                InitialiseParameters();
                TUIODimensions.Remove(TUIOManager.AugmentaTUIODimension.TUIO2D.ToString());

                TUIODescriptors.Remove(TUIOManager.AugmentaTUIODescriptor.OBJECT.ToString());
                TUIODescriptors.Remove(TUIOManager.AugmentaTUIODescriptor.CURSOR.ToString());

                (TargetScript as TUIOManager).TUIODimension = TUIOManager.AugmentaTUIODimension.TUIO25D;
                (TargetScript as TUIOManager).TUIODescriptor = TUIOManager.AugmentaTUIODescriptor.BLOB;
                (TargetScript as TUIOManager).dimension = (TargetScript as TUIOManager).TUIODimension.ToString();
                (TargetScript as TUIOManager).descriptor = (TargetScript as TUIOManager).TUIODescriptor.ToString();

                break;
        }
    }

    private void InitialiseParameters()
    {
        TUIODimensions.Clear();
        TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO2D.ToString());
        TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO25D.ToString());
        TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO3D.ToString());

        TUIODescriptors.Clear();
        TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.OBJECT.ToString());
        TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.CURSOR.ToString());
        TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.BLOB.ToString());
    }
}