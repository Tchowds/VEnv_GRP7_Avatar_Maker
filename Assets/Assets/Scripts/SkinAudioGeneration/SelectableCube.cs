using UnityEngine;
using TMPro;
using static SkinConstants;

public class SelectableCube : MonoBehaviour
{
    public RequestMode requestMode;
    public ApiRequestHandler apiRequestHandler;
    public TextMeshPro modeDisplayText;

    private void Start()
    {
        modeDisplayText.text = requestMode.ToString();
    }

    public void OnCubeClicked()
    {
        apiRequestHandler.CurrentMode = requestMode;
        apiRequestHandler.resultTextMesh.text = $"Mode selected: {requestMode}";
    }
}
