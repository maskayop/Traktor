using UnityEngine;
using UnityEngine.InputSystem;

namespace TobyFredson
{
	public class ExtendedFlycam : MonoBehaviour
	{
		[Header("Settings")]
		public float cameraSensitivity = 3f;
		public float climbSpeed = 4f;
		public float normalMoveSpeed = 10f;
		public float slowMoveFactor = 0.25f;
		public float fastMoveFactor = 3f;

		[Header("Mouse Look")]
		[Tooltip("Invert mouse Y axis")]
		public bool invertY = true;        // ← Changed default to true

		private float rotationX = 0.0f;
		private float rotationY = 0.0f;

		private bool cursorLocked = true;

		void Start()
		{
			LockCursor();
		}

		void Update()
		{
			HandleCursorLock();

			if (!cursorLocked) return;

			HandleRotation();
			HandleMovement();
		}

		private void HandleRotation()
		{
			Vector2 mouseDelta = Mouse.current.delta.ReadValue() * cameraSensitivity * Time.deltaTime;

			rotationX += mouseDelta.x;

			// Y axis - this is the critical line
			float yMovement = invertY ? -mouseDelta.y : mouseDelta.y;
			rotationY += yMovement;

			rotationY = Mathf.Clamp(rotationY, -90f, 90f);

			if (mouseDelta.sqrMagnitude > 0.0001f)
			{
				transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
				transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
			}
		}

		private void HandleMovement()
		{
			float speed = normalMoveSpeed;

			if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
				speed *= fastMoveFactor;
			else if (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed)
				speed *= slowMoveFactor;

			Vector3 moveDir = Vector3.zero;

			if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveDir += transform.forward;
			if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveDir -= transform.forward;
			if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveDir -= transform.right;
			if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveDir += transform.right;

			if (moveDir.sqrMagnitude > 0.01f)
				transform.position += moveDir.normalized * speed * Time.deltaTime;

			if (Keyboard.current.qKey.isPressed)
				transform.position += transform.up * climbSpeed * Time.deltaTime;
			if (Keyboard.current.eKey.isPressed)
				transform.position -= transform.up * climbSpeed * Time.deltaTime;
		}

		private void HandleCursorLock()
		{
			if (Keyboard.current.endKey.wasPressedThisFrame)
			{
				cursorLocked = !cursorLocked;
				if (cursorLocked) LockCursor();
				else UnlockCursor();
			}
		}

		private void LockCursor()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		private void UnlockCursor()
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		private void OnDisable()
		{
			UnlockCursor();
		}
	}
}