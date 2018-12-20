# UnityLimbSim

# Introduction

Solves inverse kinematic problems by rotating limb segments incrementally toward target.

[Brief demo of limbs in motion](https://www.youtube.com/watch?v=pwzNadIxbiM)

In this video, 5 procedurally generated limbs orient themselves in real time to touch a moving sphere. The limbs are able to re-orient in real time as the target sphere's position is changed.

[Demonstration of Limb Simulator's properties](https://www.youtube.com/watch?v=pwzNadIxbiM)

In this video, I demonstrate some of the parameters and properties of the limb simulator. The limb simulation can be set to orient toward targets at that instant, to interpolate between targets in real time, or to interpolate between targets as the limb moves in world space. As targets are moved, whether in edit mode or in play mode, the inverse kinematic solver is able to compute a limb orientation which approaches the target.

# Implementation details

For each limb segment, 

* a *current* vector is drawn from that limb segment's base to an end-effector
* a *desired* vector is drawn from the lib segment's base to the kinematic target

The inverse kinematic solver computes the rotation necessary to align the segment-base-to-end-effector vector with the vector from segment-base-to-target. The rotation is successively constrained by

* limits on the limb's natural local orientation with respect to its parent rotation
* limits to the angular distension of the limb off of its natural local orientation
* a weighting toward the limb segment's current rotation, which produces smoother movement

# How to use

Create a gameObject and attach ScriptRoot.cs to it. Create a parent-child hierarchy of of PREFAB_NODE limb segments. In the ScriptRoot, create a LimbTrainer, specify a ScriptNode trainer_node to be oriented toward a target, and specify a GameObject target for the limb to orient toward.
