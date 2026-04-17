
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Ingredient))]
public class Throwable : MonoBehaviour
{
    public bool hurtsPlayer = false;
    Ingredient ing;
    // Start is called before the first frame update
    void Start()
    {
        ing = GetComponent<Ingredient>();
    }

    public void InitializeThrown() {
        hurtsPlayer = true;
    }

    void OnCollisionEnter(Collision c) {
        ing.ThrowableOnCollide(c);
        if (c.gameObject.tag == "Floor") {
            RemoveIngredient();
        }
        else {
            hurtsPlayer = false;
        }
    }

    

    public void RemoveIngredient() {
        // explode vfx?
        Destroy(gameObject);
    }

    public void OnGrab(SelectEnterEventArgs args) {
        Debug.Log("Ingredient Grabbed");
        hurtsPlayer = false;
    }
}
