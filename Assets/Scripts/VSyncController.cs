using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VSyncController : MonoBehaviour
{
    public bool enableVSync {
		get { return IsVSyncEnabled(); }
		set { EnableVSync(value); }
	}

	void EnableVSync(bool enable) {

		QualitySettings.vSyncCount = enable ? 1 : 0;
	}

	bool IsVSyncEnabled() {

		return QualitySettings.vSyncCount > 0;
	}
}
