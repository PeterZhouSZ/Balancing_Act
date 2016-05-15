﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ConnectComponents : MonoBehaviour {
	// will eventually create a cylinder (hopefully extruded along a curve?), put both involved components and the connecting edge in the same
	// multibody pendant object, and update the center of mass (math and display)
	// will have to look at rigid connections between these things, and how to fix a point and simulate
	public GameObject connector; // cylinder to represent connector
	public Button myselfButton;
	private MobileMaster masterMobile;
	private GameObject obj1, obj2, newConnector;
	public float indicator_size = 1f;


	// Use this for initialization
	void Start () {
		myselfButton = GetComponent<Button>();
		myselfButton.onClick.AddListener(() => connect());

		masterMobile = GameObject.Find("Mobile_Master").GetComponent<MobileMaster> ();
	}

	void connect() {
		if (masterMobile.selected.Count == 2) {
			obj1 = ((GameObject)masterMobile.selected [0]);
			obj2 = ((GameObject)masterMobile.selected [1]);

			makeConnector ();
			createMultiBodyPendant ();
			makeImmutable ();
		} 
		else {
			Debug.Log ("Must have 2 selected");
		}
	}

	// connect the selected objects
	void makeConnector() {
		// instantiate the cylinder with one of the points as point of instantiation
		Vector3 obj1_pos = obj1.transform.position;
		Vector3 obj2_pos = obj2.transform.position;

		Vector3 pos = Vector3.Lerp (obj1_pos, obj2_pos, 0.5f); // put origin of object in between the two pieces

		newConnector = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
		newConnector.transform.position = pos;

		Vector3 scale_vec = new Vector3 (indicator_size, indicator_size, indicator_size);
		newConnector.transform.localScale = scale_vec;

		// transform.LookAt to make y axis of cylinder face the other point
		newConnector.transform.LookAt (obj2.transform.TransformPoint (obj2_pos));
		newConnector.transform.Rotate (new Vector3 (1.0f, 0, 0), 90);

		//scale cylinder based on distance between the points
		Vector3 newScale = newConnector.transform.localScale;
		newScale.y = Vector3.Distance (obj1_pos, obj2_pos) / 2;
		newConnector.transform.localScale = newScale;

		newConnector.AddComponent<Pendant> (); // we need material density, volume; adds rigid body and draggable
	}


	void createMultiBodyPendant() {
		GameObject pendant_group = new GameObject ();
		MultiBodyPendant mbp = pendant_group.AddComponent<MultiBodyPendant> ();

		mbp.addPendant (obj1.transform.parent.gameObject);
		Debug.Log ("added obj 1");
		mbp.addPendant (obj2.transform.parent.gameObject);
		Debug.Log ("added obj 2");
		mbp.addConnector (newConnector);
		Debug.Log ("added connector");
		Debug.Log ("current pendant length is " + mbp.pendants.Count);
	}

	/// <summary>
	/// TODO FIGURE OUT WHY THE CONNECTOR DESTROYS AREN'T WORKING
	/// ADD CONSTRAINTS BETWEEN THE OBJECTS SO WE CAN DO PHYSICS SIM
	/// UPDATE SUSP PT ACCORDING TO CENTER OF MASS
	/// 
	/// also, link the objects so the draggable attribute applies to the whole multibody pendant
	/// figure out a way to nest individual pendants as multibody? or figure out clean way to join them together.
	/// </summary>

	void makeImmutable() {
		Debug.Log ("trying to clean up");
		GameObject cube1 = obj1.transform.parent.gameObject;
		GameObject cube2 = obj2.transform.parent.gameObject;

		masterMobile.selected.Remove(obj1);			// clear out the selected list
		masterMobile.selected.Remove(obj2);
		Debug.Log ("trying to clean up 2");

		Destroy (cube1.GetComponent<DragRigidBody>());		// can no longer drag the cubes
		Destroy (cube2.GetComponent<DragRigidBody>());

		Debug.Log("1 object held in newConnector is: " + newConnector.ToString());
		Destroy (newConnector.GetComponent<DragRigidBody>());		// can't drag the connector either -- WHY DOESN'T THIS WORK
		Debug.Log ("trying to clean up 3");

		Destroy (cube1.GetComponent<RigidBodyEditor>());	// get rid of the suspension points
		Destroy (obj1);								
		Destroy (cube2.GetComponent<RigidBodyEditor>());
		Destroy (obj2);
		Destroy (newConnector.GetComponent<RigidBodyEditor>()); // OR THIS
		Destroy (newConnector.GetComponent<SuspensionPoint> ());// OR THIS

		Debug.Log ("trying to clean up 4");

	}

	// Update is called once per frame
	void Update () {

	}

	// make sure we free up the listener
	void Destroy() {
		myselfButton.onClick.RemoveListener(() => connect());
	}
}
