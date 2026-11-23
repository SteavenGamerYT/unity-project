using UnityEngine;
using UnityEngine.UI;

public class StanceHUD : MonoBehaviour
{
    [Tooltip("Image component for the stance sprites")]
    public Image stanceImage;
    [Tooltip("Sprite to display when standing")]
    public Sprite standingSprite;
    [Tooltip("Sprite to display when crouching")]
    public Sprite crouchingSprite;

    private void Start()
    {
        PlayerCharacterController character = FindAnyObjectByType<PlayerCharacterController>();
        if (character != null)
        {
            character.onStanceChanged += OnStanceChanged;
            OnStanceChanged(character.isCrouching);
        }
        else
        {
            enabled = false;
        }
    }

    void OnStanceChanged(bool crouched)
    {
        stanceImage.sprite = crouched ? crouchingSprite : standingSprite;
    }
}
