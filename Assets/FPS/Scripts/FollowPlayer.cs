using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private Transform m_PlayerTransform;
    private Vector3 m_OriginalOffset;

    void Start()
    {
        PlayerCharacterController playerCharacterController = GameObject.FindAnyObjectByType<PlayerCharacterController>();
        if (playerCharacterController != null)
        {
            m_PlayerTransform = playerCharacterController.transform;
        }
        else
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                m_PlayerTransform = playerObj.transform;
            }
            else
            {
                PlayertMovemet pm = GameObject.FindAnyObjectByType<PlayertMovemet>();
                if (pm != null)
                {
                    m_PlayerTransform = pm.transform;
                }
            }
        }

        if (m_PlayerTransform != null)
        {
            m_OriginalOffset = transform.position - m_PlayerTransform.position;
        }
        else
        {
            enabled = false;
        }
    }

    void LateUpdate()
    {
        if (m_PlayerTransform != null)
        {
            transform.position = m_PlayerTransform.position + m_OriginalOffset;
        }
    }
}
