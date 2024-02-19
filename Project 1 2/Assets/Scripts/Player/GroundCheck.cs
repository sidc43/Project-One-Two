using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    #region COLLISIONS
    public bool onground;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
            onground = true;

        transform.parent.GetComponent<PlayerController>().onGround = onground;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
            onground = false;

        transform.parent.GetComponent<PlayerController>().onGround = onground;
    }
    #endregion
}
