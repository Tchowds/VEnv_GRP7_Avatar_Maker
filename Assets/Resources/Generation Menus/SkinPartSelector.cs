using System;
using UnityEngine;
using UnityEngine.UI;

public class SkinPartSelector : MonoBehaviour
{
    public SkinConstants.SkinPart skinPart;

    public Button headButton;
    public Button torsoButton;
    public Button bothButton;

    // Define your selected and normal colors.
    // You can adjust these as needed.
    private Color selectedColor = Color.red;
    private Color normalColor = Color.white;

    public event Action<string> OnSkinPartSelected;

    public ApiRequestHandler apiRequestHandler;

    void Start()
    {
        // Add click listeners to each button.
        headButton.onClick.AddListener(() => OnButtonPressed(SkinConstants.SkinPart.Head));
        torsoButton.onClick.AddListener(() => OnButtonPressed(SkinConstants.SkinPart.Torso));
        bothButton.onClick.AddListener(() => OnButtonPressed(SkinConstants.SkinPart.Both));

        // Optionally, initialize the visuals with a default selection.
        OnButtonPressed(skinPart);
    }

    void OnButtonPressed(SkinConstants.SkinPart part)
    {
        skinPart = part;
        UpdateButtonVisuals();
        OnSkinPartSelected?.Invoke(skinPart.ToString());
        apiRequestHandler.selectedSkinPart = part;
    }

    void UpdateButtonVisuals()
    {
        // Change each button's color based on whether it is the current selection.
        UpdateButtonColor(headButton, skinPart == SkinConstants.SkinPart.Head);
        UpdateButtonColor(torsoButton, skinPart == SkinConstants.SkinPart.Torso);
        UpdateButtonColor(bothButton, skinPart == SkinConstants.SkinPart.Both);
    }

    void UpdateButtonColor(Button button, bool isSelected)
    {
        // Get the current ColorBlock settings.
        ColorBlock colors = button.colors;
        // Set the normal (idle) color to selected or normal.
        colors.normalColor = isSelected ? selectedColor : normalColor;
        button.colors = colors;
    }
}
