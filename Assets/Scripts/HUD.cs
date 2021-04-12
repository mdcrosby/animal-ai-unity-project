using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    // Start is called before the first frame update

    public Image healthFillImage;
    Health m_PlayerHealth;

    void Start()
    {
        Agent agent = GameObject.FindObjectOfType<Agent>();
        // DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, PlayerHealthBar>(playerCharacterController, this);

        // m_PlayerHealth = agent.GetComponent<Health>();
        // DebugUtility.HandleErrorIfNullGetComponent<Health, PlayerHealthBar>(m_PlayerHealth, this, playerCharacterController.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // healthFillImage.fillAmount = m_PlayerHealth.currentHealth / m_PlayerHealth.maxHealth;
    }
}
