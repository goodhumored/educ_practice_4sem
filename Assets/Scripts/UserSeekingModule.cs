using System;
using StarterAssets;
using UnityEngine;
using UnityEngine.Events;

public class UserSeekingModule : MonoBehaviour
{
    public UnityAction<GameObject> OnUserFound;
    public UnityAction OnUserLost;

    private void OnTriggerEnter(Collider roomCollider)
    {
        if (roomCollider.GetComponent<FirstPersonController>())
        {
            Debug.Log("I see user! He is");
            Debug.Log(roomCollider.transform.position);
            try
            {
                OnUserFound(roomCollider.gameObject);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("I lost user");
        try
        {
            OnUserLost();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}