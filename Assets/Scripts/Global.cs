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

	public void Reset(){
		SetFocus (transform.GetChild (1));
	}

	public void SetFocus (Transform newFocus) {
		startTime = Time.time;

		// get zoom factor
		float zoomFactor = newFocus.GetComponent<ObjectController> ().level / actualLevel;

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
			if (subScript != null) { // if it's not the room !!
				float subLevel = subScript.level;
				// delete the one of higher level not focused
				if (subLevel > actualLevel && subScript.enclosing != actualFocus)
					Destroy (subObject.gameObject);
			// activate the one of the same level and not focused
				else if (subLevel == actualLevel && subObject != actualFocus || subScript.enclosing == actualFocus)
					subObject.gameObject.SetActive (true);
			}
		}
	}

	void Awake () {
		Transform folderClone = (Transform) Instantiate (firstFolder, transform);


		SetFocus(folderClone);

		// initialize first folder
		string path = new DirectoryInfo( Application.persistentDataPath).Parent.Parent.Parent.Parent.FullName; // experimental !
		folderClone.gameObject.GetComponent<ObjectController> ().path = path;
		DirectoryInfo dir = new DirectoryInfo(path);
		folderClone.GetComponent<ObjectController> ().SetText (folderClone, dir.FullName);
		/*Debug.Log ("-----------------------" + dir.FullName);
		foreach(FileSystemInfo f in dir.GetFileSystemInfos()){
			Debug.Log ("-----------------------" + f.Name);
		}*/
	}
		
	void Update () {
		// animates the object to move them in front of the camera

		float t = Time.time - startTime;

		Vector3 next = Vector3.Slerp (transform.localScale, targetScale, t * smoothTime);
		transform.localScale = next;

		transform.position = Vector3.Slerp (transform.position, targetPosition, t * smoothTime);
		transform.rotation = Quaternion.Slerp (transform.rotation, targetRotation, t * smoothTime);
	}
}
