using UnityEngine;
using TMPro; // Use this if you're using TMP_Dropdown; otherwise use UnityEngine.UI for Dropdown
using static SkinConstants;

public class SkinPartDropdown : MonoBehaviour
{
    // Reference to the ApiRequestHandler that holds the selectedSkinPart field.
    public ApiRequestHandler apiRequestHandler;

    // Reference to the dropdown component (TMP_Dropdown or Dropdown)
    public TMP_Dropdown dropdown;

    void Start()
    {
        if(dropdown != null)
        {
            // Optionally, you can populate the dropdown dynamically.
            dropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string> { "Head", "Torso", "Both" };
            dropdown.AddOptions(options);
        }
    }

    // This method will be called whenever the dropdown value changes.
    public void OnDropdownValueChanged()
    {
        if (dropdown == null || apiRequestHandler == null)
            return;

        // Get the selected option's text.
        string selectedOption = dropdown.options[dropdown.value].text;

        // Compare the text (ignoring case) to set the appropriate enum value.
        switch (selectedOption.ToLower())
        {
            case "head":
                apiRequestHandler.selectedSkinPart = SkinConstants.SkinPart.Head;
                break;
            case "torso":
                apiRequestHandler.selectedSkinPart = SkinConstants.SkinPart.Torso;
                break;
            case "both":
                apiRequestHandler.selectedSkinPart = SkinConstants.SkinPart.Both;
                break;
            default:
                Debug.LogWarning("Dropdown selection did not match any SkinConstants enum values: " + selectedOption);
                break;
        }
        Debug.Log("Selected Skin Part: " + apiRequestHandler.selectedSkinPart);
    }
}
