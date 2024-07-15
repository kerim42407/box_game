using Mirror;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DiceThrower : NetworkBehaviour
{
    public Dice diceToThrow;
    public int amountOfDice = 2;
    public float throwForce = 5f;
    public float rollForce = 10f;

    private List<GameObject> _spawnedDice = new();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public async void RollDice()
    {
        if (diceToThrow == null)
        {
            return;
        }

        foreach (var _dice in _spawnedDice)
        {
            Destroy(_dice);
        }

        for (int i = 0; i < amountOfDice; i++)
        {
            Dice dice = Instantiate(diceToThrow, transform.position, transform.rotation);
            _spawnedDice.Add(dice.gameObject);
            dice.RollDice(throwForce, rollForce, i);
            NetworkServer.Spawn(dice.gameObject);
            await Task.Yield();
        }
    }
}
