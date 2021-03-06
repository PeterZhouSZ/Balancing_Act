﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ConnectComponents : MonoBehaviour {
	// will eventually create a cylinder (hopefully extruded along a curve?), put both involved components and the connecting edge in the same
	// multibody pendant object, and update the center of mass (math and display)
	// will have to look at rigid connections between these things, and how to fix a point and simulate
	public Button myselfButton;
	public GameObject pend;
	private MobileMaster masterMobile;
	private GameObject obj1, obj2, newConnector;
	private GameObject connLoop1, connLoop2;

	public float indicator_size = 1f;
	private int conn_num = 0;


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
			Debug.Log ("the value of newConnector is " + newConnector.ToString());
			makeImmutable_connect ();
		}
		else {
			Debug.Log ("Must have 2 selected");
		}
	}

	void makeConnector() {
		// instantiate the cylinder with one of the points as point of instantiation
		Vector3 obj1_pos = obj1.transform.position;
		Vector3 obj2_pos = obj2.transform.position;

		Vector3 pos = Vector3.Lerp (obj1_pos, obj2_pos, 0.5f); // put origin of object in between the two pieces

		Quaternion rot = Quaternion.identity;
		newConnector = Instantiate (pend, pos, rot) as GameObject;
		Pendant p = newConnector.AddComponent<Pendant> ();
		p.isConnector = true;

		newConnector.name = "Connector " + conn_num++;

		Vector3 scale_vec = new Vector3 (indicator_size, indicator_size, indicator_size);
		newConnector.transform.localScale = scale_vec;

		// transform.LookAt to make y axis of cylinder face the other point
		newConnector.transform.LookAt (obj2.transform.position );
		newConnector.transform.Rotate (new Vector3 (1.0f, 0, 0), 90);

		//scale cylinder based on distance between the points
		Vector3 newScale = newConnector.transform.localScale;
		//newScale.y = Vector3.Distance (obj1_pos, obj2_pos) / 2 - 1; // for diameter 1 cylinder
		newScale.y = Vector3.Distance (obj1_pos, obj2_pos) / 2 - 0.6f; // for diameter 2 cylinder
		Debug.Log ("scale of piece is " + newScale.ToString());
		newConnector.transform.localScale = newScale;

		connLoop1 = (GameObject) Instantiate(Resources.Load("torus_vert"), obj1.transform.position, Quaternion.identity);

		connLoop2 = (GameObject)Instantiate (Resources.Load("torus_vert"), obj2.transform.position, Quaternion.identity);

		Vector3 shift = new Vector3 (0f, 4f, 0f);
		newConnector.transform.Translate (shift, Space.World);
		connLoop1.transform.Translate (shift, Space.World);
		connLoop2.transform.Translate (shift, Space.World);

	}


	void createMultiBodyPendant() {
		GameObject pendant_group = new GameObject ();
		pendant_group.name = "pend_group " + conn_num;
		MultiBodyPendant mbp = pendant_group.AddComponent<MultiBodyPendant> ();

		mbp.addConnector (newConnector);
		mbp.addPendant (obj1.transform.parent.gameObject);
		mbp.addPendant (obj2.transform.parent.gameObject);

		mbp.SendMessage ("freezeGroup");

		// track the last connector so we can finish mobile
		masterMobile.last_susp_pt = mbp;
	}

	void makeImmutable_connect() {
		GameObject cube1 = obj1.transform.parent.gameObject;
		GameObject cube2 = obj2.transform.parent.gameObject;

		masterMobile.selected.Remove(obj1);			// clear out the selected list
		masterMobile.selected.Remove(obj2);

		cube1.SendMessage ("makeImmutable");
		cube2.SendMessage ("makeImmutable");
		newConnector.SendMessage ("makeImmutable");

		cube1.SendMessage ("invalidateSuspensionPoint");
		cube2.SendMessage ("invalidateSuspensionPoint");
		newConnector.SendMessage ("removeSuspensionPoint");
	}

	// Update is called once per frame
	void Update () {

	}

	// make sure we free up the listener
	void Destroy() {
		myselfButton.onClick.RemoveListener(() => connect());
	}
}
