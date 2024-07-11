using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DiceThrower : MonoBehaviour
{
    public Dice diceToThrow;
    public int amountOfDice = 1;
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RollDice();
        }
    }

    private async void RollDice()
    {
        if(diceToThrow == null)
        {
            return;
        }

        foreach(var _dice in _spawnedDice)
        {
            Destroy(_dice);
        }

        for(int i = 0; i < amountOfDice; i++)
        {
            Dice dice = Instantiate(diceToThrow, transform.position, transform.rotation);
            _spawnedDice.Add(dice.gameObject);
            dice.RollDice(throwForce, rollForce, i);
            await Task.Yield();
        }
    }
}
