using UnityEngine;

[System.Serializable]
public class PlayerState
{
    public string playerID;
    public PlayerLocationMessage location;
    public int playerNum; // 1 or 2. -1 by default, until they save onto a mannequin.

    public int skinsSavedOnMannequins;
}
