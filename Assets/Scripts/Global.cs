using UnityEngine;
using System.Collections;
using System.IO;

public class Global : MonoBehaviour {


	public Transform firstFolder;
	public float smoothTime = .1f;

	// object selected
	private Transform actualFocus;
	private float actualLevel = 1;

	private float startTime = 0f;
	private Vector3 targetScale;

	private Vector3 targetPosition;
	private Quaternion targetRotation;

	public void setFocus (Transform newFocus) {
		startTime = Time.time;

		// get zoom factor
		float levelDiff = newFocus.GetComponent<ObjectController> ().level / actualLevel;
		float zoomFactor = levelDiff;//Mathf.Pow (5, levelDiff);

		// set vars to next focus
		actualFocus = newFocus;
		actualLevel = newFocus.GetComponent<ObjectController> ().level;

		// compute new scale, rotation and pos
		targetScale = transform.localScale * zoomFactor;
		Quaternion rotation = Quaternion.Inverse (actualFocus.rotation);
		targetRotation = rotation * transform.rotation;
		targetPosition = rotation * (transform.position - actualFocus.position) * zoomFactor;

		// go through all object and delete/activate the right ones
		foreach (Transform subObject in transform) {
			ObjectController subScript = subObject.GetComponent<ObjectController> ();
			float subLevel = subScript.level;
			// delete the one of higher level not focused
			if (subLevel > actualLevel && subScript.enclosing != actualFocus)
				Destroy (subObject.gameObject);
			// activate the one of the same level and not focused
			else if( subLevel == actualLevel && subObject != actualFocus)
				subObject.gameObject.SetActive(true);
		}
	}

	void Awake () {
		Transform folderClone = (Transform) Instantiate (firstFolder, transform);
		//folderClone.localPosition += new Vector3 (0f, 0f, 4);
		Camera.main.transform.position = new Vector3 (0f, 0f, -1f);

		// adjust camera at begining
		//CameraFollowsTarget followScript = Camera.main.GetComponent<CameraFollowsTarget>();
		//followScript.setTarget (transform.position - transform.rotation * new Vector3(0f, 0f, 4), transform.position, transform.up);

		setFocus(folderClone);
		folderClone.gameObject.GetComponent<ObjectController> ().path = "/";
		DirectoryInfo dir = new DirectoryInfo("/");
		folderClone.GetChild (0).GetComponent<TextMesh> ().text = dir.FullName;

	}

	void Update () {
		float t = Time.time - startTime;

		Vector3 next = Vector3.Slerp (transform.localScale, targetScale, t * smoothTime);
		/*if (next == transform.localScale) {
			if (Input.GetKeyDown ("space"))
				startRescale ();
		} else */
		transform.localScale = next;

		transform.position = Vector3.Slerp (transform.position, targetPosition, t * smoothTime);
		transform.rotation = Quaternion.Slerp (transform.rotation, targetRotation, t * smoothTime);
	}
}
