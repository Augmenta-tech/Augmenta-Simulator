using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtocolVersionManagerControllable : Controllable
{
	[OSCProperty(TargetList = "protocolVersions")]
	public string protocol;

	public List<string> protocolVersions;

    public override void Awake() {
        protocol = ProtocolVersionManager.protocolVersion.ToString();
        protocolVersions.Add(ProtocolVersionManager.AugmentaProtocolVersion.V1.ToString());
        protocolVersions.Add(ProtocolVersionManager.AugmentaProtocolVersion.V2.ToString());

        base.Awake();
    }

    public override void OnUiValueChanged(string name) {
        base.OnUiValueChanged(name);

        ProtocolVersionManager.protocolVersion = (ProtocolVersionManager.AugmentaProtocolVersion)Enum.Parse(typeof(ProtocolVersionManager.AugmentaProtocolVersion), protocol);
    }

    public override void OnScriptValueChanged(string name) {
        base.OnScriptValueChanged(name);

        protocol = ProtocolVersionManager.protocolVersion.ToString();
    }
}
