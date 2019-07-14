using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{


    public Dropdown selectionDropdown;
    public Dropdown crossoverDropdown;
    public Dropdown mutationDropdown;
    public InputField seedInputField;
    public InputField numberofIndividualsField;

    void Start()
    {
        loadSelectionDropdownValues();
        loadCrossoverDropdownValues();
        loadMutationDropdownValues();
    }
    private void loadSelectionDropdownValues()
    {
        foreach (Population.MetodeSelectie metoda in Enum.GetValues(typeof(Population.MetodeSelectie)))
        {
            Dropdown.OptionData newitem = new Dropdown.OptionData();
            newitem.text = metoda.ToString();

            selectionDropdown.options.Add(newitem);
        }
        showFirstItem(selectionDropdown);
    }
    private void loadCrossoverDropdownValues()
    {
        foreach (Population.MetodeIncrucisare metoda in Enum.GetValues(typeof(Population.MetodeIncrucisare)))
        {
            Dropdown.OptionData newitem = new Dropdown.OptionData();
            newitem.text = metoda.ToString();

            crossoverDropdown.options.Add(newitem);
        }
        showFirstItem(crossoverDropdown);
    }
    private void loadMutationDropdownValues()
    {
        foreach (Population.MetodeMutatie metoda in Enum.GetValues(typeof(Population.MetodeMutatie)))
        {
            Dropdown.OptionData newitem = new Dropdown.OptionData();
            newitem.text = metoda.ToString();

            mutationDropdown.options.Add(newitem);
        }
        showFirstItem(mutationDropdown);
    }
    private void showFirstItem(Dropdown dropdown)
    {
        int TempInt = selectionDropdown.value;
        dropdown.value = dropdown.value + 1;
        dropdown.value = TempInt;
    }


    public void loadScene()
    {
        PlayerPrefs.SetString("SelectionMethod", selectionDropdown.options[selectionDropdown.value].text);
        PlayerPrefs.SetString("CrossoverMethod", crossoverDropdown.options[crossoverDropdown.value].text);
        PlayerPrefs.SetString("MutationMethod", mutationDropdown.options[mutationDropdown.value].text);

        if(seedInputField.text!="")
        PlayerPrefs.SetInt("InputSeed", int.Parse(seedInputField.text));
        else
        PlayerPrefs.SetInt("InputSeed", 1);

        if (numberofIndividualsField.text != "" && numberofIndividualsField.text != "1")
            PlayerPrefs.SetInt("nrOfIndividuals", int.Parse(numberofIndividualsField.text));
        else
            PlayerPrefs.SetInt("nrOfIndividuals", 13);

        SceneManager.LoadScene("Genetic Algorithm");
    }


}
