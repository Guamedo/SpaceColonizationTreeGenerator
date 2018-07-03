using UnityEngine;

public class FirsPersonCamera : MonoBehaviour {

    public float speedH = 2.0f;
    public float speedV = 2.0f;
    public float speed = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Update() {
        if (Input.GetMouseButton(0)) {
            yaw += speedH * Input.GetAxis("Mouse X") * Time.deltaTime;
            pitch -= speedV * Input.GetAxis("Mouse Y") * Time.deltaTime;

            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }

        if (Input.GetKey(KeyCode.W)) {
            transform.position += speed * transform.forward * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S)) {
            transform.position -= speed * transform.forward * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D)) {
            transform.position += speed * transform.right * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A)) {
            transform.position -= speed * transform.right * Time.deltaTime;
        }
    }
}
