using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtocolVersionManager : MonoBehaviour
{
    public enum AugmentaProtocolVersion {
        V1,
        V2
    }

    public static AugmentaProtocolVersion protocolVersion {
        get { return _protocolVersion; }
        set { _protocolVersion = value; augmentaProtocolVersionChanged?.Invoke(); }
    }
    private static AugmentaProtocolVersion _protocolVersion = AugmentaProtocolVersion.V2;

    public delegate void AugmentaProtocolVersionChanged();
    public static event AugmentaProtocolVersionChanged augmentaProtocolVersionChanged;
}
