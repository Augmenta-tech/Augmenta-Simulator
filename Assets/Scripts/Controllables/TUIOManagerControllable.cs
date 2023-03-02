using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

//public class TUIOManagerControllable : Controllable
//{
//    [Header("OUTPUT SETTINGS")]
//    [OSCProperty(enumName = "AugmentaTUIOPreset")]
//    public AugmentaTUIOPreset TUIOPreset;
//    private const String presetParameterName = "TUIOPreset"; // if you change the variable name above do not forget to also change this name
//    //[OSCProperty(TargetList = "TUIOPresets")]
//    //public string preset;
//    //public List<string> TUIOPresets;

//    [OSCProperty]
//    public int outputPort;
//    [OSCProperty]
//    public string outputIP;

//    [Header("TUIO SETTINGS")]
//    [OSCProperty(enumName = "AugmentaTUIODimension")]
//    public AugmentaTUIODimension TUIODimension;
//    private const String dimensionParameterName = "TUIODimension"; // if you change the variable name above do not forget to also change this name
//    //[OSCProperty(TargetList = "TUIODimensions")]
//    //public string dimension;

//    //public List<string> TUIODimensions;


//    //[OSCProperty(TargetList = "TUIODescriptors")]
//    //public string descriptor;
//    [OSCProperty(enumName = "AugmentaTUIODescriptor")]
//    public AugmentaTUIODescriptor TUIODescriptor;
//    private const String descriptorParameterName = "TUIODescriptor"; // if you change the variable name above do not forget to also change this name


//    //public List<string> TUIODescriptors;

//    //[OSCProperty(enumName = "CustomEnum")]
//    //public CustomEnum customEnum;


//    public override void Awake()
//    {
//        //dimension = TUIOManager.TUIODimension.ToString();
//        //TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO2D.ToString());
//        //TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO25D.ToString());
//        //TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO3D.ToString());

//        //descriptor = TUIOManager.TUIODescriptor.ToString();
//        //TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.OBJECT.ToString());
//        //TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.CURSOR.ToString());
//        //TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.BLOB.ToString());

//        //preset = TUIOManager.TUIOPreset.ToString();
//        //TUIOPresets.Add(TUIOManager.AugmentaTUIOPreset.NONE.ToString());
//        //TUIOPresets.Add(TUIOManager.AugmentaTUIOPreset.NOTCH.ToString());
//        //TUIOPresets.Add(TUIOManager.AugmentaTUIOPreset.TOUCHDESIGNER.ToString());
//        //TUIOPresets.Add(TUIOManager.AugmentaTUIOPreset.MINIMAL.ToString());
//        //TUIOPresets.Add(TUIOManager.AugmentaTUIOPreset.BEST.ToString());

//        base.Awake();
//    }
//    //public override void Update()
//    //{
//    //    base.Update();
//    //}
//    public override void OnUiValueChanged(string name)
//    {
//        base.OnUiValueChanged(name);

//        if (name == dimensionParameterName || name == descriptorParameterName)
//        {
//            //ChangedParameterInFonctionOfPreset();
//            RemoveAllTUIODescriptors();
//            //TUIOManager.TUIODimension = (AugmentaTUIODimension)Enum.Parse(typeof(AugmentaTUIODimension), dimension);
//            //TUIOManager.TUIODescriptor = (AugmentaTUIODescriptor)Enum.Parse(typeof(AugmentaTUIODescriptor), descriptor);
//        }

//        if (name == presetParameterName)
//        {
//            //TUIOManager.TUIOPreset = (AugmentaTUIOPreset)Enum.Parse(typeof(AugmentaTUIOPreset), preset);
//            ChangedParameterPreset();
//        }
//    }

//    public override void OnScriptValueChanged(string name)
//    {
//        base.OnScriptValueChanged(name);

//        /*
//        if (name == dimensionParameterName || name == descriptorParameterName)
//        {
//            //removeAllTUIODescriptors();
//            //dimension = TUIOManager.TUIODimension.ToString();
//            //descriptor = TUIOManager.TUIODescriptor.ToString();
//        }
//        */

//        if (name == presetParameterName)
//        {
//            //preset = TUIOManager.TUIOPreset.ToString();
//            ChangedParameterPreset();
//        }
//    }

//    private void RemoveAllTUIODescriptors()
//    {
//        try
//        {
//            foreach (int i in Enum.GetValues(typeof(AugmentaTUIODimension)))//for (int i = 0; i < 3; i++)
//            {
//                foreach (int j in Enum.GetValues(typeof(AugmentaTUIODescriptor)))//for (int j = 0; j < 3; j++)
//                {
//                    string addr = TUIOManager.activeManager.GetAddressTUIO((AugmentaTUIODescriptor)j, (AugmentaTUIODimension)i);
//                    var msgAlive = new OSCMessage(addr);

//                    msgAlive.Append("alive");
//                    TUIOManager.activeManager.SendAugmentaMessage(msgAlive);
//                    var msgFseq = new OSCMessage(addr);

//                    msgFseq.Append("fseq");
//                    msgFseq.Append(Time.frameCount + 1);
//                    TUIOManager.activeManager.SendAugmentaMessage(msgFseq);
//                }
//            }
//        }
//        catch (NullReferenceException e)
//        {
//            Debug.Log("Null Reference " + e.ToString());
//            //return;
//        }

//    }

//    private void ChangedParameterPreset()
//    {
//        switch (TUIOPreset)
//        {
//            case AugmentaTUIOPreset.NONE:
//                /*
//                TUIODimensions.Clear();
//                TUIODimensions.Add(AugmentaTUIODimension.TUIO2D.ToString());
//                TUIODimensions.Add(AugmentaTUIODimension.TUIO25D.ToString());
//                TUIODimensions.Add(AugmentaTUIODimension.TUIO3D.ToString());

//                TUIODescriptors.Clear();
//                TUIODescriptors.Add(AugmentaTUIODescriptor.OBJECT.ToString());
//                TUIODescriptors.Add(AugmentaTUIODescriptor.CURSOR.ToString());
//                TUIODescriptors.Add(AugmentaTUIODescriptor.BLOB.ToString());

//                dimension = TUIOManager.TUIODimension.ToString();
//                descriptor = TUIOManager.TUIODescriptor.ToString();
//                preset = TUIOManager.TUIOPreset.ToString();
//                */

//                break;

//            case AugmentaTUIOPreset.TOUCHDESIGNER:

//                if (TUIOManager.activeManager.outputPort != OSCManager.activeManager.outputPort)
//                {
//                    Debug.Log("Warning, Remote port for OSC will be set to the same value as remote port for TUIO.");
//                    OSCManager.activeManager.outputPort = TUIOManager.activeManager.outputPort;
//                }

//                /*
//                TUIODimensions.Clear();
//                TUIODimensions.Add(AugmentaTUIODimension.TUIO2D.ToString());
//                TUIODimensions.Add(AugmentaTUIODimension.TUIO25D.ToString());
//                TUIODimensions.Add(AugmentaTUIODimension.TUIO3D.ToString());

//                TUIODescriptors.Clear();
//                TUIODescriptors.Add(AugmentaTUIODescriptor.OBJECT.ToString());
//                TUIODescriptors.Add(AugmentaTUIODescriptor.CURSOR.ToString());
//                TUIODescriptors.Add(AugmentaTUIODescriptor.BLOB.ToString());
//                */

//                TUIOManager.activeManager.TUIODimension = AugmentaTUIODimension.TUIO25D;
//                TUIOManager.activeManager.TUIODescriptor = AugmentaTUIODescriptor.BLOB;

//                break;

//            case AugmentaTUIOPreset.NOTCH:

//                if (TUIOManager.activeManager.outputPort == OSCManager.activeManager.outputPort)
//                {
//                    Debug.Log("Warning, Remote ports for OSC and TUIO will be set to different values from each other.");
//                    OSCManager.activeManager.outputPort++;
//                }
//                /*
//                TUIODimensions.Clear();
//                TUIODimensions.Add(AugmentaTUIODimension.TUIO2D.ToString());

//                TUIODescriptors.Clear();
//                TUIODescriptors.Add(AugmentaTUIODescriptor.OBJECT.ToString());
//                TUIODescriptors.Add(AugmentaTUIODescriptor.CURSOR.ToString());
//                TUIODescriptors.Add(AugmentaTUIODescriptor.BLOB.ToString());
//                */

//                if (TUIODimension != AugmentaTUIODimension.TUIO2D)
//                {
//                    TUIODimension = AugmentaTUIODimension.TUIO2D;
//                    TUIOManager.activeManager.TUIODimension = AugmentaTUIODimension.TUIO2D;
//                    Debug.Log("Warning, the augmenta dimension is necessary in 2D with Notch preset");
//                }

//                TUIOManager.activeManager.TUIODescriptor = AugmentaTUIODescriptor.OBJECT;

//                break;

//            case AugmentaTUIOPreset.MINIMAL:
//                /*
//                TUIODimensions.Clear();
//                TUIODimensions.Add(AugmentaTUIODimension.TUIO2D.ToString());

//                TUIODescriptors.Clear();
//                TUIODescriptors.Add(AugmentaTUIODescriptor.CURSOR.ToString());
//                */
//                if (TUIODimension != AugmentaTUIODimension.TUIO2D)
//                {
//                    TUIODimension = AugmentaTUIODimension.TUIO2D;
//                    TUIOManager.activeManager.TUIODimension = AugmentaTUIODimension.TUIO2D;

//                    Debug.Log("Warning, the augmenta dimension and descriptor are necessary 2D and Cursor with Minimal preset");
//                }

//                if (TUIODescriptor != AugmentaTUIODescriptor.CURSOR)
//                {
//                    TUIODescriptor = AugmentaTUIODescriptor.CURSOR;
//                    TUIOManager.activeManager.TUIODescriptor = AugmentaTUIODescriptor.CURSOR;
//                    Debug.Log("Warning, the augmenta dimension and descriptor are necessary 2D and Cursor with Minimal preset");
//                }

//                break;

//            case AugmentaTUIOPreset.BEST:
//                /*
//                TUIODimensions.Clear();
//                TUIODimensions.Add(AugmentaTUIODimension.TUIO25D.ToString());
//                TUIODimensions.Add(AugmentaTUIODimension.TUIO3D.ToString());

//                TUIODescriptors.Clear();
//                TUIODescriptors.Add(AugmentaTUIODescriptor.BLOB.ToString());
//                */

//                if (TUIODimension != AugmentaTUIODimension.TUIO25D && TUIODimension != AugmentaTUIODimension.TUIO3D)
//                {
//                    TUIODimension = AugmentaTUIODimension.TUIO25D;
//                    TUIOManager.activeManager.TUIODimension = AugmentaTUIODimension.TUIO25D;
//                    Debug.Log("Warning, the augmenta dimension and descriptor are necessary 2,5D or 3D and Cursor with Best preset");
//                }

//                if (TUIODescriptor != AugmentaTUIODescriptor.BLOB)
//                {
//                    TUIODescriptor = AugmentaTUIODescriptor.BLOB;
//                    TUIOManager.activeManager.TUIODescriptor = AugmentaTUIODescriptor.BLOB;
//                }

//                break;
//        }
//    }

//private void ChangedParameterInFonctionOfPreset()
//{
//    switch (TUIOPreset)
//    {
//        //case AugmentaTUIOPreset.NONE:

//        //    break;

//        //case AugmentaTUIOPreset.TOUCHDESIGNER:

//        //    break;

//        case AugmentaTUIOPreset.NOTCH:

//            if (TUIODimension != AugmentaTUIODimension.TUIO2D)
//            {
//                TUIODimension = AugmentaTUIODimension.TUIO2D;
//                TUIOManager.activeManager.TUIODimension = AugmentaTUIODimension.TUIO2D;
//                Debug.Log("Warning, the augmenta dimension is necessary in 2D with Notch preset");
//            }

//            break;

//        case AugmentaTUIOPreset.MINIMAL:

//            if (TUIODimension != AugmentaTUIODimension.TUIO2D)
//            {
//                TUIODimension = AugmentaTUIODimension.TUIO2D;
//                TUIOManager.activeManager.TUIODimension = AugmentaTUIODimension.TUIO2D;

//                Debug.Log("Warning, the augmenta dimension and descriptor are necessary 2D and Cursor with Minimal preset");
//            }

//            if (TUIODescriptor != AugmentaTUIODescriptor.CURSOR)
//            {
//                TUIODescriptor = AugmentaTUIODescriptor.CURSOR;
//                TUIOManager.activeManager.TUIODescriptor = AugmentaTUIODescriptor.CURSOR;
//                Debug.Log("Warning, the augmenta dimension and descriptor are necessary 2D and Cursor with Minimal preset");
//            }

//            break;

//        case AugmentaTUIOPreset.BEST:

//            if (TUIODimension != AugmentaTUIODimension.TUIO25D && TUIODimension != AugmentaTUIODimension.TUIO3D)
//            {
//                TUIODimension = AugmentaTUIODimension.TUIO25D;
//                TUIOManager.activeManager.TUIODimension = AugmentaTUIODimension.TUIO25D;
//                Debug.Log("Warning, the augmenta dimension and descriptor are necessary 2,5D or 3D and Cursor with Best preset");
//            }

//            if (TUIODescriptor != AugmentaTUIODescriptor.BLOB)
//            {
//                TUIODescriptor = AugmentaTUIODescriptor.BLOB;
//                TUIOManager.activeManager.TUIODescriptor = AugmentaTUIODescriptor.BLOB;
//            }

//            break;

//        default:
//            break;
//    }
//}

//}
public class TUIOManagerControllable : Controllable
{
    [Header("OUTPUT SETTINGS")]
    [OSCProperty(TargetList = "TUIOPresets")]
    public string preset;
    private const String presetParameterName = "preset"; // if you change the variable name above do not forget to also change this name
    public List<string> TUIOPresets;

    [OSCProperty]
    public int outputPort;
    [OSCProperty]
    public string outputIP;

    [Header("TUIO SETTINGS")]
    [OSCProperty(TargetList = "TUIODimensions")]
    public string dimension;
    private const String dimensionParameterName = "dimension"; // if you change the variable name above do not forget to also change this name
    public List<string> TUIODimensions;


    [OSCProperty(TargetList = "TUIODescriptors")]
    public string descriptor;
    private const String descriptorParameterName = "descriptor"; // if you change the variable name above do not forget to also change this name
    public List<string> TUIODescriptors;

    public override void Awake()
    {
        dimension = TUIOManager.TUIODimension.ToString();
        TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO2D.ToString());
        TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO25D.ToString());
        TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO3D.ToString());

        descriptor = TUIOManager.TUIODescriptor.ToString();
        TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.OBJECT.ToString());
        TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.CURSOR.ToString());
        TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.BLOB.ToString());

        preset = TUIOManager.TUIOPreset.ToString();
        TUIOPresets.Add(TUIOManager.AugmentaTUIOPreset.NONE.ToString());
        TUIOPresets.Add(TUIOManager.AugmentaTUIOPreset.NOTCH.ToString());
        TUIOPresets.Add(TUIOManager.AugmentaTUIOPreset.TOUCHDESIGNER.ToString());
        TUIOPresets.Add(TUIOManager.AugmentaTUIOPreset.MINIMAL.ToString());
        TUIOPresets.Add(TUIOManager.AugmentaTUIOPreset.BEST.ToString());

        base.Awake();
    }

    public override void OnUiValueChanged(string name)
    {
        base.OnUiValueChanged(name);

        if (name == dimensionParameterName || name == descriptorParameterName)
        {
            RemoveAllTUIODescriptors();
            TUIOManager.TUIODimension = (TUIOManager.AugmentaTUIODimension)Enum.Parse(typeof(TUIOManager.AugmentaTUIODimension), dimension);
            TUIOManager.TUIODescriptor = (TUIOManager.AugmentaTUIODescriptor)Enum.Parse(typeof(TUIOManager.AugmentaTUIODescriptor), descriptor);
            //ChangedParameterInFonctionOfPreset();
        }

        if (name == presetParameterName)
        {
            RemoveAllTUIODescriptors();
            TUIOManager.TUIOPreset = (TUIOManager.AugmentaTUIOPreset)Enum.Parse(typeof(TUIOManager.AugmentaTUIOPreset), preset);
            ChangedParameterPreset();
        }
    }

    public override void OnScriptValueChanged(string name)
    {
        base.OnScriptValueChanged(name);


        if (name == dimensionParameterName || name == descriptorParameterName)
        {
            //removeAllTUIODescriptors();
            dimension = TUIOManager.TUIODimension.ToString();
            descriptor = TUIOManager.TUIODescriptor.ToString();
        }


        if (name == presetParameterName)
        {
            preset = TUIOManager.TUIOPreset.ToString();
            ChangedParameterPreset();
        }
    }

    private void RemoveAllTUIODescriptors()
    {
        try
        {
            foreach (int i in Enum.GetValues(typeof(TUIOManager.AugmentaTUIODimension)))//for (int i = 0; i < 3; i++)
            {
                foreach (int j in Enum.GetValues(typeof(TUIOManager.AugmentaTUIODescriptor)))//for (int j = 0; j < 3; j++)
                {
                    string addr = TUIOManager.activeManager.GetAddressTUIO((TUIOManager.AugmentaTUIODescriptor)j, (TUIOManager.AugmentaTUIODimension)i);
                    var msgAlive = new OSCMessage(addr);

                    msgAlive.Append("alive");
                    TUIOManager.activeManager.SendAugmentaMessage(msgAlive);
                    var msgFseq = new OSCMessage(addr);

                    msgFseq.Append("fseq");
                    msgFseq.Append(Time.frameCount + 1);
                    TUIOManager.activeManager.SendAugmentaMessage(msgFseq);
                }
            }
        }
        catch (NullReferenceException e)
        {
            Debug.Log("Null Reference " + e.ToString());
            //return;
        }

    }

    private void ChangedParameterPreset()
    {
        switch (TUIOManager.TUIOPreset)
        {
            case TUIOManager.AugmentaTUIOPreset.NONE:
                
                TUIODimensions.Clear();
                TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO2D.ToString());
                TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO25D.ToString());
                TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO3D.ToString());

                TUIODescriptors.Clear();
                TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.OBJECT.ToString());
                TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.CURSOR.ToString());
                TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.BLOB.ToString());

                dimension = TUIOManager.TUIODimension.ToString();
                descriptor = TUIOManager.TUIODescriptor.ToString();
                preset = TUIOManager.TUIOPreset.ToString();
                

                break;

            case TUIOManager.AugmentaTUIOPreset.TOUCHDESIGNER:

                if (TUIOManager.activeManager.outputPort != OSCManager.activeManager.outputPort)
                {
                    Debug.Log("Warning, Remote port for OSC will be set to the same value as remote port for TUIO.");
                    OSCManager.activeManager.outputPort = TUIOManager.activeManager.outputPort;
                }
                
                TUIODimensions.Clear();
                TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO2D.ToString());
                TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO25D.ToString());
                TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO3D.ToString());

                TUIODescriptors.Clear();
                TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.OBJECT.ToString());
                TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.CURSOR.ToString());
                TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.BLOB.ToString());

                TUIOManager.TUIODimension = TUIOManager.AugmentaTUIODimension.TUIO25D;
                TUIOManager.TUIODescriptor = TUIOManager.AugmentaTUIODescriptor.BLOB;
                dimension = TUIOManager.TUIODimension.ToString();
                descriptor = TUIOManager.TUIODescriptor.ToString();
                
                break;

            case TUIOManager.AugmentaTUIOPreset.NOTCH:

                if (TUIOManager.activeManager.outputPort == OSCManager.activeManager.outputPort)
                {
                    Debug.Log("Warning, Remote ports for OSC and TUIO will be set to different values from each other.");
                    OSCManager.activeManager.outputPort++;
                }
                
                TUIODimensions.Clear();
                TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO2D.ToString());

                TUIODescriptors.Clear();
                TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.OBJECT.ToString());
                TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.CURSOR.ToString());
                TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.BLOB.ToString());

                TUIOManager.TUIODimension = TUIOManager.AugmentaTUIODimension.TUIO2D;
                TUIOManager.TUIODescriptor = TUIOManager.AugmentaTUIODescriptor.OBJECT;
                dimension = TUIOManager.TUIODimension.ToString();
                descriptor = TUIOManager.TUIODescriptor.ToString();
                break;

            case TUIOManager.AugmentaTUIOPreset.MINIMAL:
                
                TUIODimensions.Clear();
                TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO2D.ToString());

                TUIODescriptors.Clear();
                TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.CURSOR.ToString());

                TUIOManager.TUIODimension = TUIOManager.AugmentaTUIODimension.TUIO2D;
                TUIOManager.TUIODescriptor = TUIOManager.AugmentaTUIODescriptor.CURSOR;
                dimension = TUIOManager.TUIODimension.ToString();
                descriptor = TUIOManager.TUIODescriptor.ToString();

                break;

            case TUIOManager.AugmentaTUIOPreset.BEST:
                
                TUIODimensions.Clear();
                TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO25D.ToString());
                TUIODimensions.Add(TUIOManager.AugmentaTUIODimension.TUIO3D.ToString());

                TUIODescriptors.Clear();
                TUIODescriptors.Add(TUIOManager.AugmentaTUIODescriptor.BLOB.ToString());

                TUIOManager.TUIODimension = TUIOManager.AugmentaTUIODimension.TUIO25D;
                TUIOManager.TUIODescriptor = TUIOManager.AugmentaTUIODescriptor.BLOB;
                dimension = TUIOManager.TUIODimension.ToString();
                descriptor = TUIOManager.TUIODescriptor.ToString();

                break;
        }
    }
}