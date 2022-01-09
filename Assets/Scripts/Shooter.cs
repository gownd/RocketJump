using UnityEngine;
using MoreMountains.Feedbacks;

public class Shooter : MonoBehaviour 
{
    [SerializeField] MMFeedbacks shootFeedback;
    [SerializeField] Transform firePoint = null;
    [SerializeField] GameObject rocketPrefab = null;

    [SerializeField] float shootForce = 20f;

    Rigidbody2D rb;
    Vector2 mousePos;

    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();    
    }

    private void Update() 
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); 

        if(Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void FixedUpdate() 
    {
        Vector2 lookDir = mousePos - new Vector2(transform.position.x, transform.position.y);
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    void Shoot()
    {
        shootFeedback.PlayFeedbacks();

        GameObject rocket = Instantiate(rocketPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rbRocket = rocket.GetComponent<Rigidbody2D>();
        rbRocket.AddForce(firePoint.right * shootForce, ForceMode2D.Impulse);
    }
}