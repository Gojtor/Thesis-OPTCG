using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TCGSim
{
    public class Card : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
    {
        private CanvasGroup canvasGroup;
        private Image image;
        private bool isImgLoaded = false;
        //Init variables
        private string cardNumber;
        private Hand hand = null;
        private PlayerBoard playerBoard;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            hand = this.GetComponentInParent<Hand>();
            image = this.gameObject.GetComponent<Image>();
        }

        public void OnBeginDrag(PointerEventData pointerEventData)
        {
            if (!isImgLoaded)
            {
                loadCardImg();
            }
            this.transform.SetParent(this.transform.parent.parent);
            playerBoard.enableRaycastOnTopCard();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = .8f;
            Debug.Log("OnBeginDrag");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GameObject objectAtDragEnd = eventData.pointerEnter; // Which this object landed on
            if (eventData.pointerEnter == null || objectAtDragEnd.GetComponent<CharacterArea>() == null 
                || objectAtDragEnd.transform.parent != hand.transform.parent || objectAtDragEnd.transform.childCount==6)
            {
                this.transform.SetParent(hand.transform);
                canvasGroup.blocksRaycasts = true;
                Debug.Log("Cannot play the card!");
            }
            else
            {
                image.raycastTarget = false;
            }
            canvasGroup.alpha = 1f;
            Debug.Log("OnEndDrag");
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.transform.position = eventData.position;
            Debug.Log("OnDrag");
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("OnDrop");
        }

        public void loadCardImg()
        {
            image.sprite = Resources.Load<Sprite>("Cards/" + cardNumber.Split('-')[0] + "/" + cardNumber);
            isImgLoaded = true;
        }
        public void raycastTargetChange(bool on)
        {
            image.raycastTarget = on;
        }

        public void Init(PlayerBoard playerBoard,Hand hand, string cardNumber)
        {
            this.playerBoard = playerBoard;
            this.hand = hand;
            this.cardNumber = cardNumber;
        }
    }
}
