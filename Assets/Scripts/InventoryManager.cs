using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    

    
    private Inventory inventory; //Script del inventario del jugador
    public int oldUpgrade; //Mejora seleccionada

    private TMPro.TMP_Dropdown upgradeDropdown; //Referencia al Dropdown propio

    [SerializeField] private List<string> possibleOptions; //Lista de las posibles opciones de texto mapeadas a su valor numerico
    [SerializeField] private int id; //ID de slot, usado para inventario
    [SerializeField] TMPro.TMP_Dropdown[] otherDropdowns; //Referencias a los otros Dropdowns

    public bool isRunning = true; //Bool para que el Update de InventoryManager solo corra cuando se abre el inventario


    public TMPro.TMP_Dropdown.OptionData Tina; //Nombrado de esta forma por algo dicho en una llamada en la que estaba, arreglo mi codigo, usado para buscar y mantener la opcion seleccionada al modificar la lista de opciones
    
    public bool isChangedByPerson = true; //Creado para frenar error de Stack Overflow al cambiar de valor por codigo
    void Start()
    {
        //Preparaciones
        inventory = FindObjectOfType<Inventory>();
        upgradeDropdown = GetComponent<TMPro.TMP_Dropdown>();
        isRunning = false;
        List<TMPro.TMP_Dropdown.OptionData> newOptions = new List<TMPro.TMP_Dropdown.OptionData>(); //Crear lista vacia de nuevas opciones

        newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[0])); //Agregar vacio

        //https://stackoverflow.com/questions/55297626/disable-an-options-in-a-dropdown-unity
        TMPro.TMP_Dropdown.OptionData ddOD = new TMPro.TMP_Dropdown.OptionData(possibleOptions[Inventory.getEnabledUpgrades()[id]]);
        upgradeDropdown.options.Add(ddOD);

        oldUpgrade = Inventory.getEnabledUpgrades()[id]; //Guardar valor de mejora seleccionada

        Tina = upgradeDropdown.options[upgradeDropdown.options.IndexOf(ddOD)]; //Guardar mejora seleccionada


        if (Inventory.availableUpgrades.Count > 0) //Si hay mejoras disponibles, hacer lo siguiente:
        {
            for (int i = 0; i < Inventory.availableUpgrades.Count; i++) //Pasar por todas las mejoras disponibles
            {

                if (Inventory.availableUpgrades[i] > oldUpgrade && oldUpgrade > 0 && !newOptions.Contains(Tina)) //Si la mejora seleccionada no es vacio y es menor a la mejora disponible, agregar mejora seleccionada
                {
                    newOptions.Add(Tina);
                }

                newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[Inventory.availableUpgrades[i]])); //Agregar mejora disponible


                if (i == Inventory.availableUpgrades.Count - 1 && Inventory.availableUpgrades[i] < oldUpgrade && !newOptions.Contains(Tina)) //Si todas las mejoras disponibles han sido chequeadas y la mejora seleccionada aun no ha sido agregada, agregarla
                {
                    newOptions.Add(Tina);
                }
            }
        }
        else

        {
            if (oldUpgrade > 0) //Si no hay mejoras disponibles y la mejora seleccionada no es vacio, agregar mejora seleccionada
            {
                newOptions.Add(Tina);

            }
        }
        upgradeDropdown.options = newOptions; //Actualizar opciones
        if (upgradeDropdown.value != upgradeDropdown.options.IndexOf(Tina)) //Si el indice de la opcion seleccionada fue modificado, cambiar opcion seleccionada al nuevo indice y frenar error de Stack Overflow
        {
            isChangedByPerson = false; //Indicar que el proximo cambio sera realizado por codigo para detener error de Stack Overflow
            upgradeDropdown.value = upgradeDropdown.options.IndexOf(Tina);
        }



    }
    private void OnEnable()
    {
        isRunning = true;
    }

    private void Update()
    {
        if (Time.timeScale == 0 && isRunning) //Si se abre el menu, actualizar opciones
        {
            isRunning = false;
            List<TMPro.TMP_Dropdown.OptionData> newOptions = new List<TMPro.TMP_Dropdown.OptionData>(); //Crear lista vacia de nuevas opciones

            newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[0])); //Agregar vacio

            //https://stackoverflow.com/questions/55297626/disable-an-options-in-a-dropdown-unity

            oldUpgrade = possibleOptions.IndexOf(upgradeDropdown.captionText.text); //Guardar valor de mejora seleccionada

            Tina = upgradeDropdown.options[upgradeDropdown.value]; //Guardar mejora seleccionada


            if (Inventory.availableUpgrades.Count > 0) //Si hay mejoras disponibles, hacer lo siguiente:
            {
                for (int i = 0; i < Inventory.availableUpgrades.Count; i++) //Pasar por todas las mejoras disponibles
                {

                    if (Inventory.availableUpgrades[i] > oldUpgrade && oldUpgrade > 0 && !newOptions.Contains(Tina)) //Si la mejora seleccionada no es vacio y es menor a la mejora disponible, agregar mejora seleccionada
                    {
                        newOptions.Add(Tina);
                    }

                    newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[Inventory.availableUpgrades[i]])); //Agregar mejora disponible
                    
                    
                    if (i == Inventory.availableUpgrades.Count - 1 && Inventory.availableUpgrades[i] < oldUpgrade && !newOptions.Contains(Tina)) //Si todas las mejoras disponibles han sido chequeadas y la mejora seleccionada aun no ha sido agregada, agregarla
                    {
                        newOptions.Add(Tina);
                    }
                }
            }
            else

            {
                if (oldUpgrade > 0) //Si no hay mejoras disponibles y la mejora seleccionada no es vacio, agregar mejora seleccionada
                {
                    newOptions.Add(Tina); 
                    
                }
            }
            upgradeDropdown.options = newOptions; //Actualizar opciones
            if (upgradeDropdown.value != upgradeDropdown.options.IndexOf(Tina)) //Si el indice de la opcion seleccionada fue modificado, cambiar opcion seleccionada al nuevo indice y frenar error de Stack Overflow
            {
                isChangedByPerson = false; //Indicar que el proximo cambio sera realizado por codigo para detener error de Stack Overflow
                upgradeDropdown.value = upgradeDropdown.options.IndexOf(Tina);
            }
        }
    }


    public TMPro.TMP_Dropdown.OptionData returnSelected() //Devolver valor seleccionado
    {
        return upgradeDropdown.options[upgradeDropdown.value];
    }


    public void selectUpgrade() //Ocurre cada vez que una opcion es cambiada
    {
        if (isChangedByPerson) //Chequear que el cambio fue hecho por una persona
        {
            SoundSingleton.Instance.Boton();

            if (upgradeDropdown.value > 0) //Si una mejora es seleccionada, activarla
            {
                inventory.enableUpgrade(possibleOptions.IndexOf(upgradeDropdown.options[upgradeDropdown.value].text), id);
            }
            if (oldUpgrade > 0) //Si una mejora es deseleccionada, desactivarla
            {
                inventory.disableUpgrade(oldUpgrade, id);
            }


            //Guardar nuevos valores seleccionados
            oldUpgrade = possibleOptions.IndexOf(upgradeDropdown.captionText.text);
            Tina = upgradeDropdown.options[upgradeDropdown.value]; 

            inventory.CheckForDuplicates(); //Failsafe

            foreach (TMPro.TMP_Dropdown DD in otherDropdowns) //Pasar por todos los otros Dropdowns
            {
                List<TMPro.TMP_Dropdown.OptionData> newOptions = new List<TMPro.TMP_Dropdown.OptionData>(); //Crear lista de nuevas opciones
                InventoryManager DDIM = DD.GetComponent<InventoryManager>();
                newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[0])); //Agregar vacio a lista

                

                if (Inventory.availableUpgrades.Count > 0) //Si hay mejoras disponibles, hacer lo siguiente
                {
                    //Lo mismo que en Update pero para los otros Dropdowns
                    for (int i = 0; i < Inventory.availableUpgrades.Count; i++) //Pasar por todas las mejoras disponibles
                    {

                        if (Inventory.availableUpgrades[i] > DDIM.oldUpgrade && DDIM.oldUpgrade > 0 && !newOptions.Contains(DDIM.Tina)) //Si la mejora seleccionada no es vacio y es menor a la mejora disponible, agregar mejora seleccionada
                        {
                            newOptions.Add(DDIM.Tina); 
                        }
                        newOptions.Add(new TMPro.TMP_Dropdown.OptionData(possibleOptions[Inventory.availableUpgrades[i]])); //Agregar mejora disponible

                        if (i == Inventory.availableUpgrades.Count - 1 && Inventory.availableUpgrades[i] < DDIM.oldUpgrade && !newOptions.Contains(DDIM.Tina)) //Si todas las mejoras disponibles han sido chequeadas y la mejora seleccionada aun no ha sido agregada, agregarla
                        {
                            newOptions.Add(DDIM.Tina);
                        }
                    }
                }
                else

                {
                    if (DDIM.oldUpgrade > 0) //Si no hay mejoras disponibles y la mejora seleccionada no es vacio, agregar mejora seleccionada
                    {
                        newOptions.Add(DDIM.Tina);
                        
                    }
                }


                DD.options = newOptions; //Actualizar opciones
                if (DD.value != DD.options.IndexOf(DDIM.Tina)) //Si el indice de la opcion seleccionada fue modificado, cambiar opcion seleccionada al nuevo indice y frenar error de Stack Overflow
                {
                    DDIM.isChangedByPerson = false; //Indicar que el proximo cambio sera realizado por codigo para detener error de Stack Overflow
                    DD.value = DD.options.IndexOf(DDIM.Tina);
                }
                
            }
        }
        else //Indicar que el proximo cambio sera realizado por una persona
        {
            isChangedByPerson = true;
        }
        
    }
    
    
}
