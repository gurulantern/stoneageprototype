using UnityEngine;

[CreateAssetMenu(menuName = "Player Stats", fileName = "New Player Stats", order = 0)]
public class PlayerStats : ScriptableObject
{
    [SerializeField] private float maxStamina = 100;
    public float MaxStamina => maxStamina;
    [SerializeField] private int food = 0;
    public int Food => food;
    [SerializeField] private int wood = 0;
    public int Wood => wood;
    [SerializeField] private float speed = 2.5f;
    public float Speed  => speed;
    [SerializeField] private float tiredSpeed = .5f;
    public float TiredSpeed => tiredSpeed;
    [SerializeField] private float tireLimit = 10f;
    public float TireLimit => tireLimit;
    [SerializeField] private float tireRate = .01f;
    public float TireRate => tireRate;
    [SerializeField] private float restoreRate = .5f;
    public float RestoreRate => restoreRate;
}
