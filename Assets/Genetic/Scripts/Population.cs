using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Population : MonoBehaviour
{
    public GameObject individual;
    public GameObject Goal;
    public Text CromozomTextGUI;
    public Text FitnessTextGUI;
    public Text TimeTakenGUI;
    public Text FitnessAverageGUI;
    public Text BestAndLowestFitnessGUI;


    [System.NonSerialized] public int NrOfIndividuals;
    [System.NonSerialized] public GameObject[] Individuals;
    public float FitnessSum = 0;
    public float displayedFintessSum = 0;
    public float timeSum = 0;

    public enum MetodeSelectie { MetodaRuletei, MetodaStochasticaUniversala, MetodaRancului };

    public enum MetodeIncrucisare { IncrucisareSimpla, IncrucisareMultipla, IncrucisareUniforma };

    public enum MetodeMutatie { MutatieSimpla, MutatiePrinInterschimbare, MutatiePrinInversiune };

    [System.NonSerialized] private Vector2 spawn = new Vector2(-12.55f, -0.65f);
    private long k = 0;
    private readonly float MutationRate = 0.01f;
    private float GenerationNumber = 1;
    private MetodeSelectie selectedSelectionMethod;
    private MetodeIncrucisare selectedCrossoverMethod;
    private MetodeMutatie selectedMutationMethod;
    private int inputSeed;

    void Start()
    {
        getSelectedParameters();
        Individuals = new GameObject[NrOfIndividuals];
        SpawnPlayers();
        Time.timeScale = 15.0f;
        //DisplayCromozom();
    }

    void FixedUpdate()
    {
        if (!GoalReached())
        {
            ShowTime();
            if (k % 5 == 0)
            {
                CalculateFitness();
                DisplayFitness();

                if (AllDead())
                {
                    //DisplayCromozom();
                    DisplayFitnessAveragePerGeneration();
                    DisplayBestAndLowestFitness();
                    NaturalSelection();
                    StartCoroutine(PauseAndRespawn());
                    GenerationNumber++;
                }
                else
                {
                    MoveIndividuals();
                }
            }
            k++;
        }
        else
        {
            Finished();
        }
    }
    private void getSelectedParameters()
    {
        selectedSelectionMethod = getMetodaSelectieFromEnum(PlayerPrefs.GetString("SelectionMethod"));
        selectedCrossoverMethod = getMetodaIncrucisareFromEnum(PlayerPrefs.GetString("CrossoverMethod"));
        selectedMutationMethod = getMetodeMutatieFromEnum(PlayerPrefs.GetString("MutationMethod"));

        inputSeed = PlayerPrefs.GetInt("InputSeed");
        NrOfIndividuals = PlayerPrefs.GetInt("nrOfIndividuals");

    }

    private MetodeSelectie getMetodaSelectieFromEnum(string enumToString)
    {
        foreach (Population.MetodeSelectie metoda in Enum.GetValues(typeof(Population.MetodeSelectie)))
            if (metoda.ToString() == enumToString)
                return metoda;
        return MetodeSelectie.MetodaStochasticaUniversala;
    }
    private MetodeIncrucisare getMetodaIncrucisareFromEnum(string enumToString)
    {
        foreach (Population.MetodeIncrucisare metoda in Enum.GetValues(typeof(Population.MetodeIncrucisare)))
            if (metoda.ToString() == enumToString)
                return metoda;
        return MetodeIncrucisare.IncrucisareSimpla;
    }
    private MetodeMutatie getMetodeMutatieFromEnum(string enumToString)
    {
        foreach (Population.MetodeMutatie metoda in Enum.GetValues(typeof(Population.MetodeMutatie)))
            if (metoda.ToString() == enumToString)
                return metoda;
        return MetodeMutatie.MutatieSimpla;
    }
    private void DistrugeIndiviziNeselectati(List<GameObject> Indiviziselectati)
    {
        for (int i = 0; i < NrOfIndividuals; i++)
        {
            if (!Indiviziselectati.Contains(Individuals[i]))
            {
                Destroy(Individuals[i]);
            }
        }
    }
    private void MoveIndividuals()
    {
        for (int i = 0; i < NrOfIndividuals; i++)
        {
            if (Individuals[i].GetComponent<Rigidbody2D>().velocity.magnitude <= Individual.MaxSpeed)
            {
                if (Individuals[i].GetComponent<Individual>().iterator >= Individual.CromozomSize)
                {
                    Individuals[i].GetComponent<Individual>().Die();
                }
                else
                {
                    Individuals[i].GetComponent<Individual>().MoveIndividual();
                }
            }
        }
    }
    private void ShowTime()
    {
        timeSum += Time.deltaTime;
        TimeTakenGUI.gameObject.GetComponent<Text>().text = ("Time: " + (int)(timeSum / 15));
    }

    private void Finished()
    {
        DisplayFitnessAveragePerGeneration();
        DisplayBestAndLowestFitness();
        enabled = false;

    }

    IEnumerator PauseAndRespawn()
    {
        enabled = false;
        yield return new WaitForSeconds(0.0f);
        enabled = true;

        RespawnAll();
    }
    private bool GoalReached()
    {
        for (int i = 0; i < NrOfIndividuals; i++)
        {
            if (Individuals[i].GetComponent<Individual>().ReachedTheGoal)
                return true;
        }
        return false;
    }

    private void RespawnAll()
    {
        for (int i = 0; i < NrOfIndividuals; i++)
        {
            Individuals[i].GetComponent<Individual>().Respawn();

        }


    }
    void SpawnPlayers()
    {
        UnityEngine.Random.seed = inputSeed;
        GameObject individual_x;
        for (int i = 0; i < NrOfIndividuals; i++)
        {
            individual_x = Instantiate(individual, spawn, Quaternion.identity) as GameObject;
            individual_x.GetComponent<Individual>().GenerateVectors();
            Individuals[i] = individual_x;
        }
    }

    private bool AllDead()
    {
        for (int i = 0; i < NrOfIndividuals; i++)
        {
            if (!Individuals[i].GetComponent<Individual>().dead)
                return false;
        }
        return true;

    }

    private void NaturalSelection()
    {
        CalculateFitness();
        CalculateFitnessSum();

        List<GameObject> IndiviziSelectati = Selectie(selectedSelectionMethod);

        List<GameObject> Descendenti = Incrucisare(selectedCrossoverMethod, IndiviziSelectati);

        DistrugeIndiviziNeselectati(IndiviziSelectati);


        GenerareGeneratieNoua(Descendenti, IndiviziSelectati);

        Mutatie(selectedMutationMethod);

    }
    private void GenerareGeneratieNoua(List<GameObject> Descendenti, List<GameObject> Indiviziselectati)
    {
        for (int i = 0; i < Descendenti.Count; i++)
        {
            Individuals[i] = Descendenti[i];
        }
        for (int i = Descendenti.Count; i < (Indiviziselectati.Count + Descendenti.Count); i++)
        {
            Individuals[i] = Indiviziselectati[i - Descendenti.Count];
        }
    }

    private List<GameObject> GetIndiviziNeselectati(List<GameObject> IndiviziSelectati)
    {
        List<GameObject> IndiviziNeselectati = new List<GameObject>();
        for (int i = 0; i < NrOfIndividuals; i++)
        {
            if (!IndiviziSelectati.Contains(Individuals[i]))
            {
                IndiviziNeselectati.Add(Individuals[i]);
            }
        }

        return IndiviziNeselectati;

    }

    private List<GameObject> Incrucisare(MetodeIncrucisare MetodaIncrucisare, List<GameObject> SelectedIndividuals)
    {
        List<GameObject> Descendenti = new List<GameObject>();

        int NrDeIncrucisari = NrOfDescedantsNeeded() / 2;

        if (MetodaIncrucisare == MetodeIncrucisare.IncrucisareSimpla)
        {
            for (int i = 0; i < NrDeIncrucisari; i++)
            {
                GameObject Parent1 = GetParent(SelectedIndividuals);
                GameObject Parent2 = GetParent(SelectedIndividuals, Parent1);
                System.Tuple<GameObject, GameObject> C12 = IncrucisareSimpla(Parent1, Parent2);


                Descendenti.Add(C12.Item1);
                Descendenti.Add(C12.Item2);
            }
        }
        else if (MetodaIncrucisare == MetodeIncrucisare.IncrucisareMultipla)
        {
            for (int i = 0; i < NrDeIncrucisari; i++)
            {
                GameObject Parent1 = GetParent(SelectedIndividuals);
                GameObject Parent2 = GetParent(SelectedIndividuals, Parent1);
                System.Tuple<GameObject, GameObject> C12 = IncrucisareMultipla(Parent1, Parent2);


                Descendenti.Add(C12.Item1);
                Descendenti.Add(C12.Item2);
            }
        }
        else if (MetodaIncrucisare == MetodeIncrucisare.IncrucisareUniforma)
        {
            for (int i = 0; i < NrDeIncrucisari; i++)
            {
                GameObject Parent1 = GetParent(SelectedIndividuals);
                GameObject Parent2 = GetParent(SelectedIndividuals, Parent1);
                System.Tuple<GameObject, GameObject> C12 = IncrucisareUniforma(Parent1, Parent2);


                Descendenti.Add(C12.Item1);
                Descendenti.Add(C12.Item2);
            }
        }

        return Descendenti;
    }
    private int NrOfDescedantsNeeded()
    {
        int NrIndiviziPe2 = NrOfIndividuals / 2;
        if (NrOfIndividuals % 2 == 0)
        {
            if (NrIndiviziPe2 % 2 == 0)
            {
                return NrIndiviziPe2;
            }
            else
            {
                return NrIndiviziPe2 + 1;
            }
        }
        else
        {
            if (NrIndiviziPe2 % 2 == 0)
            {
                return NrIndiviziPe2;
            }
            else
            {
                return NrIndiviziPe2 + 1;
            }
        }

    }
    private GameObject GetParent(List<GameObject> SelectedIndividuals, GameObject FirstParent = null)
    {
        int ParentNumber = UnityEngine.Random.Range(0, SelectedIndividuals.Count - 1);
        while (SelectedIndividuals[ParentNumber] == FirstParent)
        {
            ParentNumber = UnityEngine.Random.Range(0, SelectedIndividuals.Count - 1); ;
        }

        return SelectedIndividuals[ParentNumber];
    }
    private System.Tuple<GameObject, GameObject> IncrucisareSimpla(GameObject Parent1, GameObject Parent2)
    {
        GameObject Descendant1;
        GameObject Descendant2;

        Descendant1 = Instantiate(individual, spawn, Quaternion.identity) as GameObject;
        Descendant2 = Instantiate(individual, spawn, Quaternion.identity) as GameObject;

        CopyCromozom(Descendant1, Parent1);
        CopyCromozom(Descendant2, Parent2);

        int PozitieIncrucisare = UnityEngine.Random.Range(1, Individual.CromozomSize - 1);

        for (int i = PozitieIncrucisare; i < Individual.CromozomSize; i++)
        {
            Descendant1.GetComponent<Individual>().Cromozom[i][0] = Parent2.GetComponent<Individual>().Cromozom[i][0];
            Descendant1.GetComponent<Individual>().Cromozom[i][1] = Parent2.GetComponent<Individual>().Cromozom[i][1];


            Descendant2.GetComponent<Individual>().Cromozom[i][0] = Parent1.GetComponent<Individual>().Cromozom[i][0];
            Descendant2.GetComponent<Individual>().Cromozom[i][1] = Parent1.GetComponent<Individual>().Cromozom[i][1];
        }

        return System.Tuple.Create<GameObject, GameObject>(Descendant1, Descendant2);
    }

    private System.Tuple<GameObject, GameObject> IncrucisareMultipla(GameObject Parent1, GameObject Parent2)
    {
        GameObject Descendant1;
        GameObject Descendant2;

        Descendant1 = Instantiate(individual, spawn, Quaternion.identity) as GameObject;
        Descendant2 = Instantiate(individual, spawn, Quaternion.identity) as GameObject;

        CopyCromozom(Descendant1, Parent1);
        CopyCromozom(Descendant2, Parent2);

        List<float> PozitiiIncrucisare = Algoritm_Auxiliar_Sortare_Multipla();
        int helper = -1;
        int j = 0;

        for (int i = 0; i < Individual.CromozomSize; i++)
        {
            if (j < PozitiiIncrucisare.Count)
            {
                if (PozitiiIncrucisare[j] == i)
                {
                    helper *= -1;
                    j++;
                }
            }
            if (helper == 1)
            {
                Descendant1.GetComponent<Individual>().Cromozom[i][0] = Parent2.GetComponent<Individual>().Cromozom[i][0];
                Descendant1.GetComponent<Individual>().Cromozom[i][1] = Parent2.GetComponent<Individual>().Cromozom[i][1];
                Descendant2.GetComponent<Individual>().Cromozom[i][0] = Parent1.GetComponent<Individual>().Cromozom[i][0];
                Descendant2.GetComponent<Individual>().Cromozom[i][1] = Parent1.GetComponent<Individual>().Cromozom[i][1];
            }
        }
        return System.Tuple.Create<GameObject, GameObject>(Descendant1, Descendant2);
    }
    private System.Tuple<GameObject, GameObject> IncrucisareUniforma(GameObject Parent1, GameObject Parent2)
    {
        GameObject Descendent1;
        GameObject Descendent2;

        Descendent1 = Instantiate(individual, spawn, Quaternion.identity) as GameObject;
        Descendent2 = Instantiate(individual, spawn, Quaternion.identity) as GameObject;

        bool Ban;

        for (int i = 0; i < Individual.CromozomSize; i++)
        {
            Ban = (UnityEngine.Random.value > 0.5f);

            if (Ban)
            {
                Descendent1.GetComponent<Individual>().Cromozom[i][0] = Parent1.GetComponent<Individual>().Cromozom[i][0];
                Descendent1.GetComponent<Individual>().Cromozom[i][1] = Parent1.GetComponent<Individual>().Cromozom[i][1];

                Descendent2.GetComponent<Individual>().Cromozom[i][0] = Parent2.GetComponent<Individual>().Cromozom[i][0];
                Descendent2.GetComponent<Individual>().Cromozom[i][1] = Parent2.GetComponent<Individual>().Cromozom[i][1];

            }
            else
            {
                Descendent1.GetComponent<Individual>().Cromozom[i][0] = Parent2.GetComponent<Individual>().Cromozom[i][0];
                Descendent1.GetComponent<Individual>().Cromozom[i][1] = Parent2.GetComponent<Individual>().Cromozom[i][1];

                Descendent2.GetComponent<Individual>().Cromozom[i][0] = Parent1.GetComponent<Individual>().Cromozom[i][0];
                Descendent2.GetComponent<Individual>().Cromozom[i][1] = Parent1.GetComponent<Individual>().Cromozom[i][1];
            }
        }
        return System.Tuple.Create<GameObject, GameObject>(Descendent1, Descendent2);
    }
    private List<float> Algoritm_Auxiliar_Sortare_Multipla()
    {
        List<float> PozitiiIncrucisare = new List<float>();
        int NrPozitii = 3;
        for (int i = 0; i < NrPozitii; i++)
        {
            int PozitieToBeAdded = UnityEngine.Random.Range(1, Individual.CromozomSize - 1);
            if (!(PozitiiIncrucisare.Contains(PozitieToBeAdded)))
            {
                PozitiiIncrucisare.Add(PozitieToBeAdded);
            }
            else
            {
                i -= 1;
            }
        }
        PozitiiIncrucisare.Sort();

        return PozitiiIncrucisare;
    }

    private List<GameObject> Selectie(MetodeSelectie MetodaSelectie)
    {
        List<GameObject> IndiviziSelectati = new List<GameObject>();

        if (MetodaSelectie == MetodeSelectie.MetodaRuletei)
        {
            IndiviziSelectati = Selectie_MetodaRuletei(NrOfIndividualsToBeSelected());
        }
        if (MetodaSelectie == MetodeSelectie.MetodaStochasticaUniversala)
        {
            IndiviziSelectati = Selectie_MetodaStochasticaUniversala(NrOfIndividualsToBeSelected());
        }
        if (MetodaSelectie == MetodeSelectie.MetodaRancului)
        {
            IndiviziSelectati = Selectie_MetodaRancului(NrOfIndividualsToBeSelected());
        }


        return IndiviziSelectati;
    }
    private int NrOfIndividualsToBeSelected()
    {
        int NrIndiviziPe2 = NrOfIndividuals / 2;
        if (NrOfIndividuals % 2 == 0)
        {
            if (NrIndiviziPe2 % 2 == 0)
            {
                return NrIndiviziPe2;
            }
            else
            {
                return NrIndiviziPe2 - 1;
            }
        }
        else
        {
            if (NrIndiviziPe2 % 2 == 0)
            {
                return NrIndiviziPe2 + 1;
            }
            else
            {
                return NrIndiviziPe2;
            }
        }

    }

    private List<GameObject> Selectie_MetodaRuletei(int NrOfIndividualsToBeSelected)
    {
        List<GameObject> SelectedIndividuals = new List<GameObject>();
        for (int i = 0; i < NrOfIndividualsToBeSelected; i++)
        {
            float SelectorScanare = 0;
            float RandomSelector = UnityEngine.Random.Range(0.0f, FitnessSum);
            for (int j = 0; j < NrOfIndividuals; j++)
            {
                SelectorScanare += Individuals[j].GetComponent<Individual>().Fitness;

                if (SelectorScanare >= RandomSelector)
                {
                    if (!SelectedIndividuals.Contains(Individuals[j]))
                    {
                        SelectedIndividuals.Add(Individuals[j]);
                        break;
                    }
                    else
                    {
                        j = -1;
                        RandomSelector = UnityEngine.Random.Range(0.0f, FitnessSum);
                        SelectorScanare = 0;
                    }
                }
            }
        }
        return SelectedIndividuals;
    }
    private List<GameObject> Selectie_MetodaStochasticaUniversala(int nrOfIndividualsToBeSelected)
    {
        List<GameObject> SelectedIndividuals = new List<GameObject>();
        float DistantaDintrePointeri = FitnessSum / nrOfIndividualsToBeSelected;
        float Pointer = UnityEngine.Random.Range(0.0f, FitnessSum / NrOfIndividuals);

        float SelectorScanare = 0;
        for (int j = 0; j < NrOfIndividuals; j++)
        {
            SelectorScanare += Individuals[j].GetComponent<Individual>().Fitness;
            if (SelectorScanare >= Pointer)
            {
                SelectedIndividuals.Add(Individuals[j]);
                Pointer += DistantaDintrePointeri;
            }

        }

        return SelectedIndividuals;
    }
    private List<GameObject> Selectie_MetodaRancului(int nrOfIndividualsToBeSelected)
    {
        List<GameObject> SelectedIndividuals = new List<GameObject>();
        GameObject[] ArrangedIndividuals = new GameObject[NrOfIndividuals];

        ArrangedIndividuals = Individuals;

        bool ok = true;

        while (ok)
        {
            ok = false;
            for (int i = 0; i < NrOfIndividuals - 1; i++)
            {
                if (ArrangedIndividuals[i].GetComponent<Individual>().Fitness < ArrangedIndividuals[i + 1].GetComponent<Individual>().Fitness)
                {
                    (ArrangedIndividuals[i], ArrangedIndividuals[i + 1]) = (ArrangedIndividuals[i + 1], ArrangedIndividuals[i]);
                    ok = true;
                }
            }
        }

        for (int i = 0; i < nrOfIndividualsToBeSelected; i++)
        {
            SelectedIndividuals.Add(ArrangedIndividuals[i]);
        }

        return SelectedIndividuals;
    }
    private void CalculateFitness()
    {
        for (int i = 0; i < NrOfIndividuals; i++)
        {
            float DistanceOfCurrentIndividualToGoal = Vector2.Distance(Individuals[i].transform.position, Goal.transform.position);
            var displayedFitness = (Vector2.Distance(spawn, Goal.transform.position) - Vector2.Distance(Individuals[i].transform.position, Goal.transform.position)) / Vector2.Distance(spawn, Goal.transform.position) * 100;
            Individuals[i].GetComponent<Individual>().Fitness = 10.0f / (DistanceOfCurrentIndividualToGoal * DistanceOfCurrentIndividualToGoal * DistanceOfCurrentIndividualToGoal * DistanceOfCurrentIndividualToGoal);
            if (displayedFitness > 0)
                Individuals[i].GetComponent<Individual>().DisplayedFitness = displayedFitness;
            else
                Individuals[i].GetComponent<Individual>().DisplayedFitness = 0;

        }
    }

    private void CalculateFitnessSum()
    {
        FitnessSum = 0;
        displayedFintessSum = 0;
        for (int i = 0; i < NrOfIndividuals; i++)
        {
            FitnessSum += Individuals[i].GetComponent<Individual>().Fitness;
            displayedFintessSum += Individuals[i].GetComponent<Individual>().DisplayedFitness;

        }
    }

    private void CopyCromozom(GameObject I1, GameObject I2)
    {
        for (int i = 0; i < Individual.CromozomSize; i++)
        {
            I1.GetComponent<Individual>().Cromozom[i][0] = I2.GetComponent<Individual>().Cromozom[i][0];
            I1.GetComponent<Individual>().Cromozom[i][1] = I2.GetComponent<Individual>().Cromozom[i][1];
        }
    }




    private void Mutatie(MetodeMutatie MetodaMutatie)
    {
        if (MetodaMutatie == MetodeMutatie.MutatieSimpla)
        {
            MutatieSimpla();
        }

        if (MetodaMutatie == MetodeMutatie.MutatiePrinInterschimbare)
        {
            MutatiePrinInterschimbare();
        }

        if (MetodaMutatie == MetodeMutatie.MutatiePrinInversiune)
        {
            MutatiePrinInversiune();
        }

    }

    private void MutatieSimpla()
    {
        for (int i = 1; i < NrOfIndividuals; i++)
        {
            for (int j = 0; j < Individual.CromozomSize; j++)
            {
                float Rand = UnityEngine.Random.Range(0.0f, 1.0f);
                if (Rand < MutationRate)
                {
                    Individuals[i].GetComponent<Individual>().Cromozom[j] = new Vector2(UnityEngine.Random.Range(10, -11), UnityEngine.Random.Range(10, -11));
                }
            }
        }
    }
    private void MutatiePrinInterschimbare()
    {
        for (int i = 0; i < NrOfIndividuals; i++)
        {
            int primaGena = UnityEngine.Random.Range(0, Individual.CromozomSize);
            int aDouaGena = UnityEngine.Random.Range(0, Individual.CromozomSize);

            while (primaGena == aDouaGena)
                aDouaGena = UnityEngine.Random.Range(0, Individual.CromozomSize);

            (Individuals[i].GetComponent<Individual>().Cromozom[primaGena][0], Individuals[i].GetComponent<Individual>().Cromozom[aDouaGena][0]) =
            (Individuals[i].GetComponent<Individual>().Cromozom[aDouaGena][0], Individuals[i].GetComponent<Individual>().Cromozom[primaGena][0]);

            (Individuals[i].GetComponent<Individual>().Cromozom[primaGena][1], Individuals[i].GetComponent<Individual>().Cromozom[aDouaGena][1]) =
            (Individuals[i].GetComponent<Individual>().Cromozom[aDouaGena][1], Individuals[i].GetComponent<Individual>().Cromozom[primaGena][1]);
        }
    }
    private void MutatiePrinInversiune()
    {
        for (int i = 0; i < NrOfIndividuals; i++)
        {
            int primaPozitie = UnityEngine.Random.Range(0, Individual.CromozomSize - 1);
            int aDouaPozitie = UnityEngine.Random.Range(primaPozitie + 1, Individual.CromozomSize);

            while (primaPozitie < aDouaPozitie)
            {
                (Individuals[i].GetComponent<Individual>().Cromozom[primaPozitie][0], Individuals[i].GetComponent<Individual>().Cromozom[aDouaPozitie][0]) =
                (Individuals[i].GetComponent<Individual>().Cromozom[aDouaPozitie][0], Individuals[i].GetComponent<Individual>().Cromozom[primaPozitie][0]);

                (Individuals[i].GetComponent<Individual>().Cromozom[primaPozitie][1], Individuals[i].GetComponent<Individual>().Cromozom[aDouaPozitie][1]) =
                (Individuals[i].GetComponent<Individual>().Cromozom[aDouaPozitie][1], Individuals[i].GetComponent<Individual>().Cromozom[primaPozitie][1]);


                primaPozitie++; aDouaPozitie--;
            }
        }
    }
    private float getFitnessAverage()
    {
        return (displayedFintessSum / NrOfIndividuals);
    }
    private void DisplayFitnessAveragePerGeneration()
    {
        CalculateFitnessSum();
        FitnessAverageGUI.GetComponent<Text>().text += "Generation " + GenerationNumber + " [" + getValueWithTwoDecimals(getFitnessAverage()) + "]" + "\n";
        // FitnessAverageGUI.GetComponent<Text>().text +=   "Generation " + GenerationNumber + " [" + getFitnessAverage() + "]"+ "\n";
    }
    private void DisplayBestAndLowestFitness()
    {
        BestAndLowestFitnessGUI.GetComponent<Text>().text += "[ " + GenerationNumber + " ]" + " B:" + getValueWithTwoDecimals(getBestFitness()) + " L:" + getValueWithTwoDecimals(getLowestFitness()) + "\n";
        // BestAndLowestFitnessGUI.GetComponent<Text>().text += "[ " + GenerationNumber + " ]" + " B:" + getBestFitness() + " L:" + getLowestFitness() + "\n";

    }

    private float getBestFitness()
    {
        if (GoalReached())
            return 100;
        float bestfitness = Individuals[0].GetComponent<Individual>().Fitness;
        float bestDisplayedFitness = Individuals[0].GetComponent<Individual>().DisplayedFitness;
        for (int i = 1; i < NrOfIndividuals; i++)
        {
            if (Individuals[i].GetComponent<Individual>().Fitness > bestfitness)
            {
                bestfitness = Individuals[i].GetComponent<Individual>().Fitness;
                bestDisplayedFitness = Individuals[i].GetComponent<Individual>().DisplayedFitness;
            }
        }
        return bestDisplayedFitness;
    }
    private float getLowestFitness()
    {
        float lowestfitness = Individuals[0].GetComponent<Individual>().Fitness;
        float lowestDisplayedFitness = Individuals[0].GetComponent<Individual>().DisplayedFitness;
        for (int i = 1; i < NrOfIndividuals; i++)
        {
            if (Individuals[i].GetComponent<Individual>().Fitness < lowestfitness)
            {
                lowestfitness = Individuals[i].GetComponent<Individual>().Fitness;
                lowestDisplayedFitness = Individuals[i].GetComponent<Individual>().DisplayedFitness;
            }
        }
        if (lowestDisplayedFitness < 0)
            return 0;
        else
            return lowestDisplayedFitness;
    }
    private void DisplayFitness(List<GameObject> SelectedIndividuals = null)
    {
        string IndividualsData = "";

        for (int i = 0; i < NrOfIndividuals; i++)
        {
            IndividualsData = IndividualsData + "[" + i.ToString() + "]" + "    ";
            IndividualsData += getValueWithTwoDecimals(Individuals[i].GetComponent<Individual>().DisplayedFitness) + "  ";
            if (i > 12)
                IndividualsData += " \n";
        }

        FitnessTextGUI.text = IndividualsData;
    }
    private string getValueWithTwoDecimals(float value)
    {

        string ValueToString = value.ToString();

        if (ValueToString.Contains("."))
        {


            if (ValueToString.Length - ValueToString.IndexOf(".") >= 3)
                return ValueToString.Substring(0, ValueToString.IndexOf(".") + 3);
            else
                return ValueToString.Substring(0, ValueToString.IndexOf(".") + 2);
        }
        return ValueToString;
    }
    private void DisplayCromozom()
    {
        string IndividualsData = "";
        for (int i = 0; i < NrOfIndividuals; i++)
        {
            IndividualsData = IndividualsData + "[" + i.ToString() + "]" + "    ";

            for (int j = 0; j < Individual.CromozomSize; j++)
            {
                IndividualsData += Individuals[i].GetComponent<Individual>().Cromozom[j].ToString() + "  ";

            }
            IndividualsData += '\n';
        }
        CromozomTextGUI.text = IndividualsData;
    }

}
