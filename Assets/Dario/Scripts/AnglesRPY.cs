using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AnglesRPY : MonoBehaviour {

    private Vector3 initRot;

    private Quaternion ConvertRightHandedToLeftHandedQuaternion(Quaternion rightHandedQuaternion)
    {
        return new Quaternion(-rightHandedQuaternion.x,
                               -rightHandedQuaternion.z,
                               -rightHandedQuaternion.y,
                                 rightHandedQuaternion.w);
    }

    void Update()
    {
        Quaternion q = ConvertRightHandedToLeftHandedQuaternion(transform.rotation);
        Quaternion z = ConvertRightHandedToLeftHandedQuaternion(q);
        Debug.Log("original quat is " + transform.rotation.eulerAngles);
        Debug.Log("this quat must be equal to the upper " +z.eulerAngles);

    }
}

