using System.Collections;
using System.Collections.Generic;
using TCGSim.CardResources;
using TCGSim;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        DeckBuilder.cardMagnifier.SetActive(true);
        DeckBuilder.cardMagnifier.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = this.gameObject.GetComponent<Image>().sprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DeckBuilder.cardMagnifier.SetActive(false);
    }
}
