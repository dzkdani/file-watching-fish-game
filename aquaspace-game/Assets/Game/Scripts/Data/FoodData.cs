using UnityEngine;

[CreateAssetMenu(fileName = "FoodData", menuName = "ScriptableObjects/FoodData", order = 1)]
public class FoodData : ScriptableObject, ISpawnable
{
    public string type;
    public string foodType;
    public Sprite foodSprite;
}