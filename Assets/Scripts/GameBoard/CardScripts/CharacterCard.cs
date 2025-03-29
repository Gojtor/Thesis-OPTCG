using System.Collections;
using System.Collections.Generic;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public class CharacterCard : Card, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    private LineRenderer lineRenderer;
    private Vector2 mousePos;
    private Vector2 startMousePos;
    private bool drawing = false;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = this.gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 10f;
        lineRenderer.endWidth = 10f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Basic material
        lineRenderer.positionCount = 2;
        lineRenderer.startColor=Color.black;
        lineRenderer.endColor = Color.black;
    }

    // Update is called once per frame
    void Update()
    {
        if (drawing)
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lineRenderer.SetPosition(0, new Vector3(startMousePos.x, startMousePos.y, 0f));
            lineRenderer.SetPosition(1, new Vector3(mousePos.x, mousePos.y, 0f));
        }
        CheckCardVisibility();
    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (this.cardData.active)
        {
            lineRenderer.enabled = true;
            drawing = true;
            Debug.Log("XD");
            startMousePos = this.gameObject.transform.position; 
        }
    }

    public override void LoadDataFromCardData(CardData cardData)
    {
        this.cardData = cardData;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (this.cardData.active)
        {
            Card card = eventData.pointerEnter.GetComponent<Card>();
            if (card == null || !card.cardData.active || card.cardData.playerName==this.cardData.playerName)
            {
                lineRenderer.enabled = false;
            }
            else
            {
                lineRenderer.SetPosition(1, card.transform.position);
            }
            drawing = false;
        }
    }
}
