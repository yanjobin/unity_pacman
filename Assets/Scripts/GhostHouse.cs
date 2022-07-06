using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostHouse : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.gameObject.GetComponent<Ghost>().IsInGhostHouse = true;
        collision.gameObject.GetComponent<Ghost>().OnEnterGhostHouse();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        collision.gameObject.GetComponent<Ghost>().IsInGhostHouse = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        collision.gameObject.GetComponent<Ghost>().IsInGhostHouse = false;
    }
}
