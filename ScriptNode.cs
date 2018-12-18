using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An individual node in a limb simulation.
/// angular_max: a value in range [0-1] which represents flexibility of this node
/// ref_capsule: a local gameObject which represents the actual limb
/// </summary>
public class ScriptNode : MonoBehaviour {
	[Range(0f, 1f)]
	public float angular_max;

	[SerializeField]
	private GameObject ref_capsule;
	private Quaternion natural_local;

	// For debugging. Delete to speed up
	[SerializeField]
	private LineRenderer lr_assemblage;
	[SerializeField]
	private LineRenderer lr_ideal;
	public bool indicator;

	// Use this for initialization
	void Start () {
		ReaffixNaturalLocal ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	/// <summary>
	/// Stores new local position and treats it as the default rotation off of parent.
	/// </summary>
	[ContextMenu("Reaffix the default rotation of this node to current rotation of this node")]
	public void ReaffixNaturalLocal() {
		natural_local = transform.localRotation;
	}

	/// <summary>
	/// Computes vector from self's terminus, to target
	/// </summary>
	/// <param name="ee">Ee. An end-effector. A terminal node in a sequence of limbs</param>
	/// <param name="target">Target. An object in space</param>
	public void CorrectError(ref ScriptNode ee, ref GameObject target) {
		Vector3 error = target.transform.position - ee.GetTerminus ();
		ApplyDeltaLook (ref ee, error);
	}

	/// <summary>
	/// Applies a rotation which maps vector space from the vector "limb-to-end-effector" to "limb-to-end-effector-plus-error"
	/// </summary>
	/// <param name="ee">Ee. An end-effector. A terminal node in a sequence of limbs</param>
	/// <param name="error">Error. The difference between end-effector's current position, and end-effector's ideal position</param>
	public void ApplyDeltaLook(ref ScriptNode ee, Vector3 error) {
		Vector3 la = GetLocalAssemblage (ref ee);
		Quaternion root_rot = transform.root.rotation;
		float dot_modifier = (Mathf.Abs (Vector3.Dot (Vector3.forward, root_rot * Vector3.forward)) >= 0.5f ? 1 : -1);
		root_rot = Quaternion.Euler (root_rot.eulerAngles.x, root_rot.eulerAngles.y * dot_modifier, root_rot.eulerAngles.z);

		Quaternion delta = Quaternion.FromToRotation (root_rot * la, root_rot * (la + error));

		Quaternion ideal_local = Quaternion.Inverse (transform.parent.rotation) * (transform.rotation * delta);
		Quaternion limited_local = Quaternion.RotateTowards (natural_local, ideal_local, angular_max * 360f);

		// max rotation at one update: 36 degrees
		Quaternion incremental_local = Quaternion.RotateTowards(transform.localRotation, limited_local, 0.1f * 360f);
		transform.localRotation = incremental_local;

		lr_assemblage.gameObject.SetActive (indicator);
		lr_ideal.gameObject.SetActive (indicator);
		lr_assemblage.SetPositions (new Vector3[] { transform.position, transform.position + la });
		lr_ideal.SetPositions (new Vector3[] { transform.position, transform.position + la + error });
	}

	/// <summary>
	/// Gets the vector from self to end-effector
	/// </summary>
	/// <returns>local assemblage.</returns>
	/// <param name="ee">Ee. An end-effector</param>
	private Vector3 GetLocalAssemblage(ref ScriptNode ee) {
		return(ee.GetTerminus() - transform.position);
	}

	/// <summary>
	/// Gets the representation of the terminus of this limb in world coordinates.
	/// </summary>
	/// <returns>Position of the end of this limb, in world coordinates.</returns>
	public Vector3 GetTerminus() {
		return(transform.position + transform.forward * (ref_capsule.transform.localPosition.z + ref_capsule.transform.localScale.y));
	}
}
