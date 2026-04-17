using UnityEngine;

[CreateAssetMenu(fileName = "TrashData", menuName = "ScriptableObjects/TrashData", order = 1)]
public class TrashData : ScriptableObject
{
    public string trashType;
    public Sprite trashSprite;
}