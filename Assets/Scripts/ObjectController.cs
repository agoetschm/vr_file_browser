using UnityEngine;
using System.Collections;
using System.IO;

public class ObjectController : MonoBehaviour {

	public Transform subObjectPrefab;
	public Transform textPrefab;


	// level of the object to which this script is attached
	public float level = 1f;
	// enclosing folder of the object
	public Transform enclosing;
	// path of the file/folder represented
	public string path = ".";
	// foler or file
	public bool isFolder = true;
	// previous button
	public bool isPrev = false;
	// fraction of an enclosing file/folder
	public float resize_factor;


	private float FOV = 60 * Mathf.PI / 180;//100; // "field of view" -> angle covered by the objects from the point of view of the user

	/// <summary>
	/// Open this folder, if it is one.
	/// </summary>
	public void Open()
	{
		Debug.Log ("------------------------------Click");
		if (isFolder) {
			// global script
			Global globalScript = transform.parent.GetComponent<Global> ();

			// if up
			if (isPrev) {
				// if it's not the top folder
				if (enclosing.gameObject.GetComponent<ObjectController> ().enclosing != null) {
					enclosing.gameObject.SetActive (true);
					globalScript.SetFocus (enclosing.gameObject.GetComponent<ObjectController> ().enclosing);
				}
			} else { // else explore folder			
				DirectoryInfo dir = new DirectoryInfo (path);
				FileSystemInfo[] info = dir.GetFileSystemInfos ();
				IEnumerator fileEnumerator = info.GetEnumerator ();

				// choose size of the file grid
				float sqrt = Mathf.Sqrt (info.Length + 1);
				int col_num;
				if (sqrt > 4)
					col_num = Mathf.CeilToInt (sqrt);
				else
					col_num = 4;
				
				int row_num = col_num; // => each sub-object has the same proportion as its parent => same number of rows as columns
				resize_factor = col_num + 1;

				// some intermediary values (cf schema)
				float w = transform.lossyScale.x;
				float h = transform.lossyScale.y;
				float d = (w / 2) / Mathf.Tan (FOV / 2);
				float r = d / Mathf.Cos (FOV / 2);
				float fov_vertical = Mathf.Atan (h / 2 / d) * 2;

				// TODO
				bool first = true;

				// "split" the object in (COL_NUM * row_num) smaller ones
				for (int j = row_num - 1; j >= 0; --j) {
					// x angle, same for the row
					float xAngle = (-fov_vertical / 2) + (j + 0.5f) * (fov_vertical / row_num);
					//Debug.Log (xAngle / Mathf.PI * 180);

					for (int i = 0; i < col_num; ++i) {
						// y angle (=> around y axis)
						float yAngle = (-FOV / 2) + (i + 0.5f) * (FOV / col_num);

						if ((first && dir.Parent != null) || fileEnumerator.MoveNext ()) { // no MoveNext if first and not root
							FileSystemInfo actFile;
							if (first && dir.Parent != null) {
								actFile = dir.Parent;
							} else {
								if (first)
									first = false; // no prev button in first level
								actFile = (FileSystemInfo)fileEnumerator.Current;
								//Debug.Log (actFile.Name);
							}

							// adjust the position to fit the curve
							Vector3 relPos = new Vector3 (0f, 0f, -d); // go to "camera position"
							relPos += new Vector3 (
								r * Mathf.Sin (yAngle) * Mathf.Cos (xAngle), 
								r * Mathf.Sin (xAngle), 
								r * Mathf.Cos (yAngle) * Mathf.Cos (xAngle)); // go to position 
							Vector3 subPos = transform.position + transform.rotation * relPos;

							// instantiate the sub object
							Transform subObject = (Transform)Instantiate (subObjectPrefab, subPos, 
								                     transform.rotation * Quaternion.Euler (-xAngle / Mathf.PI * 180, yAngle / Mathf.PI * 180, 0f)//);
						, transform.parent);


							// TODO
							// rescale it
							subObject.localScale /= resize_factor;

							// assign script vars
							ObjectController subScript = subObject.GetComponent<ObjectController> ();
							subScript.level = level * resize_factor;
							subScript.enclosing = transform;
							subScript.path = actFile.FullName;
							subScript.isFolder = Directory.Exists (subScript.path);

							// set text
							string pathText;
							if (first) {
								first = false;
								pathText = "..";
								subScript.isPrev = true;
							} else
								pathText = actFile.Name;

							// manage text display
							SetText(subObject, pathText);

							// set color
							MeshRenderer meshRenderer = subObject.GetComponent<MeshRenderer> ();
							meshRenderer.material.shader = Shader.Find ("Transparent/Bumped Diffuse");//("Self-Illumin/VertexLit");
							if (subScript.isFolder)
								meshRenderer.material.color = new Color (13f / 255, 71f / 255, 161f / 255, .5f);
							else
								meshRenderer.material.color = new Color (191f / 255, 54f / 255, 12f / 255, .5f);
						}
					}
				}

				// disable enclosing object
				gameObject.SetActive (false);

				// start the rescale simulating the zoom (because the camera doesn't "see" objects too close)
				globalScript.SetFocus (transform);
			}
		}
	}

	/// <summary>
	/// Sets the text of a sub folder/file.
	/// </summary>
	/// <param name="subObject">Sub object.</param>
	/// <param name="text">Text.</param>
	public void SetText(Transform subObject, string text){
		Transform textTransform = subObject.GetChild (0); // should be only the text as child
		TextMesh textMesh = textTransform.GetComponent<TextMesh> ();
		textMesh.text = text;
		// if overflow :
		// try to replace spaces first
		float limit = subObject.GetComponent<Renderer> ().bounds.size.x * 0.8f;
		if (textMesh.GetComponent<Renderer> ().bounds.size.x > limit) {
			text = text.Replace (' ', '\n');
			textMesh.text = text;
		} // then the dots 
		if (textMesh.GetComponent<Renderer> ().bounds.size.x > limit) {
			text = text.Replace (".", "\n.");
			textMesh.text = text;
		} // finally limit to 7 char per line (I know, it's not precise...)
		if (textMesh.GetComponent<Renderer> ().bounds.size.x > limit) {
			string[] lines = text.Split ('\n');
			text = "";
			foreach (string line in lines) {
				int len = line.Length;
				for (int k = 0; k < len; k += 7) {
					text += line.Substring (k, Mathf.Min (7, len - k));
					text += "\n";
				}
			}
			text = text.Trim (); // remove \n at the end
			textMesh.text = text;
		}

	}
}
