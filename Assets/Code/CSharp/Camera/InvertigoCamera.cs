using UnityEngine;

public class InvertigoCamera : MonoBehaviour //CONSIDER: delete
{
    public void Awake()
    {
        OVRManager.tracker.isEnabled = false;
        OVRCameraRig rig = GameObject.FindObjectOfType<OVRCameraRig>();
		rig.UpdatedAnchors += ZeroIPD;
    }

    private void ZeroIPD(OVRCameraRig rig)
    {
        rig.trackingSpace.FromOVRPose(rig.rightEyeAnchor.ToOVRPose(true).Inverse());
        rig.trackingSpace.FromOVRPose(rig.leftEyeAnchor.ToOVRPose(true).Inverse());
    }
}
