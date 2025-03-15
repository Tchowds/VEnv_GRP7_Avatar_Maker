using UnityEngine;
using System.Collections.Generic;

public class SwapStudioShopManager : ShopManager
{
    private List<Rotator> shopRotators = new List<Rotator>();

    protected override void Start()
    {
        base.Start(); // Call `ShopManager` setup (handles audio)
        
        // Find all Rotators inside "MixMatchModelAvatars"
        Transform avatarsParent = transform.Find("MixMatchModelAvatars");
        if (avatarsParent != null)
        {
            shopRotators.AddRange(avatarsParent.GetComponentsInChildren<Rotator>());
        }
    }

    public override void EnterShop()
    {
        base.EnterShop(); // Handles music

        Debug.Log("SwapStudioShopManager: Activating avatars & rotators.");
        ToggleRotators(true);
    }

    public override void ExitShop()
    {
        base.ExitShop(); // Handles music

        Debug.Log("SwapStudioShopManager: Deactivating avatars & rotators.");
        ToggleRotators(false);
    }

    private void ToggleRotators(bool shouldSpin)
    {
        foreach (var rotator in shopRotators)
        {
            rotator.SetSpinning(shouldSpin);
        }
    }
}
