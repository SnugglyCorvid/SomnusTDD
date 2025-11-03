using UnityEngine;

public class ViewBob : MonoBehaviour
{
    public Transform playerBody;
    public float bobbingAmount = 0.10f;
    public float bobbingSpeed = 0.95f;
    public float returnSpeed = 2.0f;

    private float timer = 0.0f;
    private Vector3 startPosition;
    private Vector3 previousPosition;
    private Player player;


    void Start()
    {
        startPosition = transform.localPosition;
        previousPosition = playerBody.position;
        player = playerBody.GetComponent<Player>();
    }

    void Update()
    {
        bool isGrounded = player.isGrounded;

        Vector3 currentPosition = playerBody.position;
        Vector3 velocity = (currentPosition - previousPosition) / Time.deltaTime;
        previousPosition = currentPosition;

        float velocityMagnitude = velocity.magnitude;

        if (isGrounded && velocityMagnitude > 0.1f)
        {
            float waveSliceX = Mathf.Sin(timer);
            float waveSliceY = Mathf.Sin(timer * 2);
            Vector3 bobbingOffset = new Vector3(
                waveSliceX * bobbingAmount,
                waveSliceY * bobbingAmount,
                0
            );
            transform.localPosition = startPosition + bobbingOffset;
            timer += bobbingSpeed * velocityMagnitude * Time.deltaTime;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPosition, returnSpeed * Time.deltaTime);
            timer = 0.0f;
        }
    }
}
