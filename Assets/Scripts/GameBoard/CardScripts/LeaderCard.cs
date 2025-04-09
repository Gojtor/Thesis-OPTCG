using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TCGSim
{
    public class LeaderCard : Card, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        private LineRenderer lineRenderer;
        private Vector2 mousePos;
        private Vector2 startMousePos;
        private bool drawing = false;
        public event Action<Card> CardAttacks;
        public int life { get; private set; } = 0;

        // Start is called before the first frame update
        void Start()
        {
            this.draggable = false;
            lineRenderer = this.gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 10f;
            lineRenderer.endWidth = 10f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Basic material
            lineRenderer.positionCount = 2;
            lineRenderer.startColor = Color.black;
            lineRenderer.endColor = Color.black;
            CardAttacks += LeaderCard_CardAttacks;
            GameManager.OnBattlePhaseChange += GameManager_OnBattlePhaseChange;
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

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (canAttack && GameManager.Instance.currentBattlePhase==BattlePhases.NOBATTLE)
            {
                lineRenderer.startColor = Color.black;
                lineRenderer.endColor = Color.black;
                lineRenderer.enabled = true;
                lineRenderer.sortingOrder = 4;
                drawing = true;
                startMousePos = this.gameObject.transform.position;
            }
        }

        public override void LoadDataFromCardData(CardData cardData)
        {
            if (this.originalPower == -1)
            {
                this.originalPower = cardData.power;
            }
            this.cardData = cardData;
            this.cardData.cardVisibility = CardVisibility.BOTH;
            this.life = this.cardData.cost;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (canAttack)
            {
                Card card = eventData.pointerEnter.GetComponent<Card>();
                if (card == null || (card.cardData.active && card.cardData.cardType != CardType.LEADER) || card.cardData.playerName == this.cardData.playerName)
                {
                    lineRenderer.enabled = false;
                }
                else
                {
                    lineRenderer.SetPosition(1, card.transform.position);
                    CardAttacks?.Invoke(card);
                }
                drawing = false;
            }
        }

        public void CardCanAttack()
        {
            canAttack = true;
            this.MakeBorderForThisCard();
        }

        public void CardCannotAttack()
        {
            canAttack = false;
            this.RemoveBorderForThisCard();
        }
        private void LeaderCard_CardAttacks(Card card)
        {
            GameManager.Instance.ChangeBattlePhase(BattlePhases.ATTACKDECLARATION, this, card);
        }
        public void DrawAttackLine(Card endPoint)
        {
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            this.EnableCanvasOverrideSorting();
            lineRenderer.sortingOrder = 4;
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, this.gameObject.transform.position);
            lineRenderer.SetPosition(1, endPoint.gameObject.transform.position);
        }

        public void RemoveAttackLine()
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }

        public void ReduceLife(int amount)
        {
            life = life - amount;
        }
        private async void GameManager_OnBattlePhaseChange(BattlePhases battlePhase, Card attacker, Card attacked)
        {
            if (battlePhase == BattlePhases.ENDOFBATTLE)
            {
                await UnityMainThreadDispatcher.RunOnMainThread(() =>
                {
                    RemoveAttackLine();
                });
            }
        }
    }
}
