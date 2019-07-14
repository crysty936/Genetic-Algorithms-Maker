using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Individual : MonoBehaviour
{
    [System.NonSerialized] public static float MoveSpeed = 30;
    [System.NonSerialized] public static float MaxSpeed = 0.0f;
    [System.NonSerialized] public static int CromozomSize = 30;

    [System.NonSerialized]  public Vector2[] Cromozom = new Vector2[CromozomSize];
    private Vector2 SpawnLocation;

    public Material DeadMaterial;
    public Material AliveMaterial;
    public Material ChampionMaterial;
    public Shader AlwaysOnTop;

    public int iterator;
    public bool dead = false;
    public bool ReachedTheGoal = false;
    public float Fitness = 0;
    public float DisplayedFitness = 0;
    public float DistToGoalFromSpawn;




    void Start()
    {
        SpawnLocation = transform.position;
        GetComponent<Renderer>().material = AliveMaterial;
        DistToGoalFromSpawn= Vector2.Distance(this.transform.position, GameObject.FindGameObjectWithTag("Goal").transform.position);

    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (c.gameObject.tag == "Individual")
        {

            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), c.gameObject.GetComponent<Collider2D>());

        }
        if (c.gameObject.tag == "Goal")
        {
            ReachedTheGoal = true;
            Die();
        }

    }
    public void Die()
    {
        dead = true;
        GetComponent<Renderer>().material = DeadMaterial;
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
    }

    public void MoveIndividual()
    {
        if (!dead)
        {
            GetComponent<Rigidbody2D>().AddForce(Cromozom[iterator] * MoveSpeed);
            iterator++;
        }

    }

    public void Respawn()
    {
        transform.position = SpawnLocation;
        iterator = 0;
        ReachedTheGoal = false;
        dead = false;
        GetComponent<Renderer>().material = AliveMaterial;
    }


    public void GenerateVectors()
    {
        iterator = 0;
        for (int i = 0; i < CromozomSize; i++)
        {
            Cromozom[i] = new Vector2(Random.Range(-10, 11), Random.Range(-10, 11));
        }
    }

    public void SetAsChampion()
    {
        GetComponent<Renderer>().material = ChampionMaterial;
        GetComponent<Renderer>().material.shader = AlwaysOnTop;

    }


}
