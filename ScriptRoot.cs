using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptRoot : MonoBehaviour {
	public enum display_modes: int {instant, sequence, target0, modulate_a_b};
	public display_modes display_mode;

	private ScriptNode[] nodes;

	public List<LimbTrainer> trainers = new List<LimbTrainer>();

	private Vector3 last_instant_position;
	public float ab_alpha = 0f;

	// Use this for initialization
	void Start () {
		last_instant_position = transform.position;
		nodes = transform.GetComponentsInChildren<ScriptNode> ();

		foreach (LimbTrainer t in trainers)
			t.path = nodes;
	}
	
	// Update is called once per frame
	void Update () {
		if (display_mode == display_modes.instant) {
			DisplayInstant ();
		}

		if (display_mode == display_modes.sequence) {
			DisplaySequence ();
		}
		if (display_mode == display_modes.target0) {
			DisplayTarget ();
		}
		if (display_mode == display_modes.modulate_a_b && trainers.Count > 1) {
			DisplayModulateAB ();
		}
	}

	private void DisplayModulateAB() {
		int f1 = 0;
		int f2 = 1;

		Vector3 delta = transform.position - last_instant_position;
		last_instant_position = transform.position;
		Vector3 movement_direction = (trainers [f1].target.transform.position - trainers [f2].target.transform.position);
		float d = Mathf.Max(1f, Vector3.Distance (trainers [f1].target.transform.position, trainers [f2].target.transform.position));
		ab_alpha = (ab_alpha + Vector3.Project (delta, movement_direction).magnitude / d) % 2f;

		float alpha = Mathf.Abs (ab_alpha - 1f);

		List<Quaternion> frame_1 = trainers [f1].GetSolution ();
		List<Quaternion> frame_2 = trainers [f2].GetSolution ();

		for (int i = 0; i < trainers[f1].path.Length; i++) {
			trainers [f1].path [i].transform.rotation = Quaternion.Lerp (frame_1 [i], frame_2 [i], alpha);
		}
	}

	/// <summary>
	/// Displays limb orientations for only first target
	/// </summary>
	private void DisplayTarget() {
		float theta = 0f;
		int f1 = Mathf.FloorToInt (theta);

		List<Quaternion> frame = trainers [f1].GetSolution ();
		for (int i = 0; i < trainers[f1].path.Length; i++) {
			trainers [f1].path [i].transform.rotation = frame [i];
		}
	}

	/// <summary>
	/// Displays limb orientations at an interpolation between instants
	/// </summary>
	private void DisplayInstant() {
		float theta = Time.timeSinceLevelLoad % (1f * trainers.Count);
		int f1 = Mathf.FloorToInt (theta);

		List<Quaternion> frame = trainers [f1].GetSolution ();
		for (int i = 0; i < trainers[f1].path.Length; i++) {
			trainers [f1].path [i].transform.rotation = frame [i];
		}
	}

	/// <summary>
	/// Forces limbs to orient in the way which the limb simulator has concluded is best
	/// </summary>
	private void DisplaySequence() {
		List<List<Quaternion>> dispositions = new List<List<Quaternion>> ();
		for (int i = 0; i < trainers.Count; i++) {
			dispositions.Add(trainers [i].GetSolution ());
		}

		float theta = Time.timeSinceLevelLoad % (1f * trainers.Count);
		float alpha = Time.timeSinceLevelLoad % 1f;

		int f1 = Mathf.FloorToInt (theta);
		int f2 = (f1 + 1) % trainers.Count;

		List<Quaternion> frame_1 = dispositions [f1];
		List<Quaternion> frame_2 = dispositions [f2];

		for (int i = 0; i < trainers[f1].path.Length; i++) {
			trainers [f1].path [i].transform.rotation = Quaternion.Lerp (frame_1 [i], frame_2 [i], alpha);
		}
	}
}
	
/// <summary>
/// A parameterizable manager for specifying a target, iterating on solutions, and returning a solution.
/// trainer_node: a node in a series of limbs which we wish to move toward a target
/// target: an object in space which we will move a limb toward
/// </summary>
[System.Serializable]
public class LimbTrainer {
	private List<Quaternion> dispositions;
	// likely overkill, reduce iter_count as desired
	private const int iter_count = 50;

	// populated by the ScriptRoot
	public  ScriptNode[] path;

	public ScriptNode trainer_node;
	public GameObject target;

	private List<Quaternion> solution;
	private Vector3 last_inst;

	/// <summary>
	/// Finds a list of quaternions which produces a limb orientation which brings the terminus of a trainer node toward the target
	/// </summary>
	/// <returns>The solution.</returns>
	public List<Quaternion> GetSolution() {
		if (solution != null && last_inst == target.transform.position)
			return(solution);
		
		solution = new List<Quaternion> ();

		for (int iters = 0; iters < iter_count; iters++) {
			foreach (ScriptNode sn in path) {
				sn.CorrectError (ref trainer_node, ref target);
			}
		}

		foreach (ScriptNode sn in path)
			solution.Add (sn.transform.rotation);

		last_inst = target.transform.position;

		return(solution);
	}
}