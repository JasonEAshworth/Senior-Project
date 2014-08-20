using UnityEngine;
using System.Collections;
[RequireComponent (typeof (CharacterController))]
public class p1Control : MonoBehaviour {

	// Handling
	public float rotationSpeed = 450;
	public float walkSpeed = 5;
	public float runSpeed = 9;
	public float jumpSpeed = 3;
	//system
	private Quaternion targetRotation;

	//components
	private CharacterController controller;
	private Camera cam;
	public Gun gun;


	void Start () {
		controller = GetComponent<CharacterController>();
		cam = Camera.main;
	}
	void ControlMOUSE(){
		Vector3 mPos = Input.mousePosition;
		mPos = cam.ScreenToWorldPoint(new Vector3(mPos.x,mPos.y, cam.transform.position.y - transform.position.y));
		targetRotation = Quaternion.LookRotation(mPos - new Vector3(transform.position.x,0,transform.position.z));
		transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, rotationSpeed *Time.deltaTime);


		Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal1"),0,Input.GetAxisRaw("Vertical1"));

		Vector3 motion = input;
		motion *= (Mathf.Abs(input.x) == 1 && Mathf.Abs(input.z) == 1)?.7f:1;
		motion *= (Input.GetButton("Run"))?runSpeed:walkSpeed;
		motion += Vector3.up * -18;
		
		controller.Move(motion * Time.deltaTime);

	}


	void ControlWASD () {
		Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal1"),0,Input.GetAxisRaw("Vertical1"));
		if (input != Vector3.zero)
		{
			targetRotation = Quaternion.LookRotation(input);
			transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, rotationSpeed *Time.deltaTime);
		}

		Vector3 motion = input;
		motion *= (Mathf.Abs(input.x) == 1 && Mathf.Abs(input.z) == 1)?.7f:1;
		motion *= (Input.GetButton("Run"))?runSpeed:walkSpeed;
		motion += Vector3.up * -18;

		controller.Move(motion * Time.deltaTime);

	}
	void Update(){
		//ControlMOUSE();
		ControlWASD();

		if (Input.GetButtonDown("Shoot1")){
			gun.Shoot();
		}
		else if (Input.GetButton("Shoot1"))
		{
			gun.ShootContinuous();
		}

	}
}
