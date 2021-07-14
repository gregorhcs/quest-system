using UnityEngine;
using System.Collections;

public class QG_LineUIHandler : MonoBehaviour
{
	struct GUILine
	{
		public Vector2 startPt;
		public Vector2 endPt;
	}

	private GUILine newline;
	private bool mouseDown;
	private bool mouseUp;
	private bool shift;
	private ArrayList lines;
	private float length;

	void Start ()
	{
		lines = new ArrayList (); 
	}

	void Update ()
	{
		if (Input.GetMouseButtonDown (0)) {
			newline = new GUILine ();
			newline.startPt = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);
			mouseDown = true;
		}
		if (Input.GetMouseButtonUp (0)) {
			mouseDown = false;
			mouseUp = true;
		} 
		if (Input.GetMouseButtonUp (1)) { 
			if (lines.Count != 0)
				lines.RemoveAt (lines.Count - 1); 
		}  
		if (Input.GetKeyDown (KeyCode.LeftShift) || Input.GetKeyDown (KeyCode.RightShift)) {
			shift = true;
		}
		if (Input.GetKeyUp (KeyCode.LeftShift) || Input.GetKeyUp (KeyCode.RightShift)) {
			shift = false;
		}
		
		if (Input.GetKey (KeyCode.KeypadEnter) || Input.GetKey ("enter")) {
			
		} 
	}

	void OnGUI ()
	{  
		if (mouseDown) { 
			newline.endPt = new Vector2 (Input.mousePosition.x, Input.mousePosition.y); 
			if (shift) { 
				if (difference (newline.endPt.x, newline.startPt.x) > difference (newline.endPt.y, newline.startPt.y)) {
					newline.endPt = new Vector2 (Input.mousePosition.x, newline.startPt.y); 
				} else {
					newline.endPt = new Vector2 (newline.startPt.x, Input.mousePosition.y); 
				}
			}
		}
		if (mouseUp) { 
			addGUILine (newline);
			mouseUp = false; 			
		}
		
		int lineCnt = 0; 
		foreach (GUILine line in lines) {   
			setLinePoints (line);
			DrawLine (line.startPt, line.endPt);
			DrawCube (line.startPt, line.endPt, lineCnt);
			lineCnt++;
		}
	}

	Vector2 setPoint (Vector2 point)
	{
		point.x = (int)point.x;
		point.y = Screen.height - (int)point.y;
		return point;
	}

	float difference (float val1, float val2)
	{
		float diff = val1 - val2;
		if (diff < 0)
			diff = -diff;
		return diff;	
	}

	float average (float val1, float val2)
	{  
		float avg = (int)val1 + (int)val2;
		avg = avg / 2;
		return avg;	
	}

	void addGUILine (GUILine line)
	{
		lines.Add (line);
	}

	void DrawCube (Vector2 pointA, Vector2 pointB, int lineNo)
	{  
		GameObject oneinst = GameObject.Find ("line" + lineNo);
		if (!oneinst) {			
			GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
			cube.name = "line" + lineNo;
			float rotangle = Mathf.Atan2 ((pointB.y - pointA.y), (pointB.x - pointA.x)) * 180 / Mathf.PI; 
			
			rotangle = 360 - rotangle; 
			cube.transform.localEulerAngles = new Vector3 (0, rotangle, 0);
			cube.transform.position = new Vector3 (average (pointA.x, pointB.x) / 20, 0, average (pointA.y, pointB.y) / 20); 
		 
			//cube.transform.localScale = new Vector3 ((length / 20), 1, 1);
			//cube.AddComponent ("Rigidbody"); 	 
			//cube.rigidbody.mass = 0.001f;
			//cube.rigidbody.useGravity = false;
			//cube.AddComponent ("Collide"); 
		}
	}

	void setLinePoints (GUILine line)
	{ 
		line.startPt = setPoint (line.startPt);
		line.endPt = setPoint (line.endPt);
		length = (line.startPt - line.endPt).magnitude; 
	}

	void DrawLine (Vector2 pointA, Vector2 pointB)
	{
		pointA = setPoint (pointA);
		pointB = setPoint (pointB);
		Texture2D lineTex = new Texture2D (1, 1);  
		Matrix4x4 matrixBackup = GUI.matrix; 
		float width = 8.0f; 	 	   	
		GUI.color = Color.red;  		
		float angle = Mathf.Atan2 (pointB.y - pointA.y, pointB.x - pointA.x) * 180f / Mathf.PI;

		GUIUtility.RotateAroundPivot (angle, pointA);
		GUI.DrawTexture (new Rect (pointA.x, pointA.y, length, width), lineTex);
		GUI.matrix = matrixBackup;  
	}
}