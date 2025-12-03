using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "MythBorn/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("General Info")]
    public string characterName;
    [TextArea] public string description;
    public Sprite characterSprite; // Para mostrar en selección de personaje

    [Header("Base Stats")]
    public int maxHealth = 100;
    public int maxMana = 100;
    public int maxEnergy = 100;

    public int minHealth = 0;
    public int minMana = 0;
    public int minEnergy = 0;

    public int healingHPFactor = 3;
    public int healingMNFactor = 5;
    public int tiringFactor = 10;

    [Header("Ability 1: Movement/Buff")]
    public string ability1Name;
    public int ability1Cost = 20;
    //Here I could add icons for each ability later on.

    [Header("Ability 2: Offensive")]
    public string ability2Name;
    public int ability2Cost = 40;
    public int ability2Damage = 10;
    public float ability2Range = 5f;

    [Header("Ability 3: Ultimate/Support")]
    public string ability3Name;
    public int ability3Cost = 50;
}