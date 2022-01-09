using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class Rocket : MonoBehaviour
{
    [SerializeField] MMFeedbacks explosionFeedback = null;
    [SerializeField] float radius = 1f;
    [SerializeField] float explodeForce = 1f;
    [SerializeField] float flyForce = 1f;


    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() 
    {
        rb.AddForce(rb.velocity.normalized * flyForce * Time.fixedDeltaTime);    
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawWireSphere(transform.position, radius);    
    }

    private void Update()
    {
        Vector2 lookDir = new Vector2(transform.position.x, transform.position.y) + rb.velocity * 10f;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if(other.collider.CompareTag("Player")) return;

        explosionFeedback.PlayFeedbacks();

        Explode();
        Destroy(gameObject);
    }

    void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (Collider2D hit in colliders)
        {
            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if(rb.GetComponent<Mover>())
                {
                    Vector2 direction = (hit.transform.position - transform.position).normalized;
                    rb.GetComponent<Mover>().RocketJump(direction);
                }
            }
        }
    }
}
