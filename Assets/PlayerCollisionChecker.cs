using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionChecker : MonoBehaviour
{
    [Header("Property")] 
    [SerializeField] LayerMask platformLayerMask;
    [SerializeField] float groundHeight = 0.01f;
    [SerializeField] float collisionRadius = 0.1f;

    [Header("Wall Checker")]
    [SerializeField] Vector2 offset;
    [SerializeField] Vector2 length;

    [Header("Status")]
    public bool isGrounded = false;
    public bool isOnWall = false;
    public bool isOnRightWall = false;
    public bool isOnLeftWall = false;


    BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }
    private void Update()
    {
        CheckCollisions();
    }

    private void CheckCollisions()
    {
        isGrounded = IsGrounded();
        isOnRightWall = Physics2D.OverlapCircle((Vector2)transform.position + offset + length, collisionRadius, platformLayerMask);
        isOnLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + offset - length, collisionRadius, platformLayerMask);
        isOnWall = isOnRightWall || isOnLeftWall;
    }

    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f ,Vector2.down, groundHeight, platformLayerMask);

        Color rayColor = (hit.collider != null) ? Color.green : Color.red;

        Debug.DrawRay(boxCollider.bounds.center + new Vector3(boxCollider.bounds.extents.x, 0), Vector2.down * (boxCollider.bounds.extents.y + groundHeight), rayColor);
        Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x, 0), Vector2.down * (boxCollider.bounds.extents.y + groundHeight), rayColor);
        Debug.DrawRay(boxCollider.bounds.center - new Vector3(boxCollider.bounds.extents.x, boxCollider.bounds.extents.y, 1f), Vector2.right * (boxCollider.bounds.extents.x) * 2f, rayColor);

        return hit.collider != null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere((Vector2)transform.position + offset + length, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + offset - length, collisionRadius);
    }
}
