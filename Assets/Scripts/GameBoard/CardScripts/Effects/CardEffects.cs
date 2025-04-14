using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace TCGSim
{
    public interface ICardEffects
    {
        public void Activate(Card card);
    }

    public class DonPowerBonus : ICardEffects
    {
        private int howMuchDonNeeded;
        private int howMuchPower;
        private bool thisEffectActive = false;

        public DonPowerBonus(int howMuchDonNeeded, int howMuchPower)
        {
            this.howMuchDonNeeded = howMuchDonNeeded;
            this.howMuchPower = howMuchPower;
        }

        public void Activate(Card card)
        {
            if (card.hasDonAttached && card.GetAttachedDonCount() >= howMuchDonNeeded)
            {
                if (!thisEffectActive)
                {
                    card.AddToPlusPower(howMuchPower);
                    card.MakeOrUpdatePlusPowerSeenOnCard();
                    thisEffectActive = true;
                    ChatManager.Instance.AddMessage("Card: " + card.cardData.cardID + " activated its DON!! effect!");
                }
                if (card.plusPower == 0)
                {
                    thisEffectActive = false;
                }

            }
            else if (thisEffectActive && !card.hasDonAttached)
            {
                thisEffectActive = false;
            }
        }
    }

    public class WhenAttackingEnemyCantBlockOver : ICardEffects
    {
        int whenAttackingDonReq = 0;
        int enemyBlockerMinPower = 0;
        private bool passiveEffect = true;

        public WhenAttackingEnemyCantBlockOver(int whenAttackingDonReq, int enemyBlockerMinPower, bool passiveEffect)
        {
            this.whenAttackingDonReq = whenAttackingDonReq;
            this.enemyBlockerMinPower = enemyBlockerMinPower;
            this.passiveEffect = passiveEffect;
        }

        public async void Activate(Card card)
        {
            if (passiveEffect) { return; }
            if (card.GetAttachedDonCount() >= whenAttackingDonReq)
            {
                await ServerCon.Instance.EnemyCantActivateBlockerOver(card.cardData.customCardID, enemyBlockerMinPower);
            }
            ChatManager.Instance.AddMessage("Card: " + card.cardData.cardID + " activated its When Attacking effect!");
            await ServerCon.Instance.ImDoneWithWhenAttackingEffect();
        }
    }

    public class SelectSubTypeCardCannotBeBlocked : ICardEffects
    {
        int upTo;
        bool activated = false;
        private List<Card> possibleTargets = new List<Card>();
        private List<Card> selectedTargets = new List<Card>();
        private List<Card> cardsThatCouldAttackBeforeEffect = new List<Card>();
        private Card effectCallerCard;
        public SelectSubTypeCardCannotBeBlocked(int upTo)
        {
            this.upTo = upTo;
        }

        public void Activate(Card card)
        {
            PlayerBoard.Instance.SetEffectInProgress(true);
            if (activated) { return; }
            effectCallerCard = card;
            cardsThatCouldAttackBeforeEffect = PlayerBoard.Instance.GetCardsThatCouldAttack();
            possibleTargets = PlayerBoard.Instance.GetCharacterAreaCards();
            possibleTargets.Add(PlayerBoard.Instance.leaderCard);
            if (possibleTargets.Count == 0) { return; }
            card.transform.position = PlayerBoard.Instance.leaderCard.transform.position;
            card.transform.Translate(-150, 0, 0);
            ShowCancelButton();
            foreach (Card cardCanAttack in cardsThatCouldAttackBeforeEffect)
            {
                switch (cardCanAttack.cardData.cardType)
                {
                    case CardResources.CardType.CHARACTER:
                        cardCanAttack.GetComponent<CharacterCard>().CardCannotAttack();
                        break;
                    case CardResources.CardType.LEADER:
                        cardCanAttack.GetComponent<LeaderCard>().CardCannotAttack();
                        break;
                    default:
                        UnityEngine.Debug.LogError("Missing card type");
                        break;
                }
            }
            foreach (Card target in possibleTargets)
            {
                if (target != card)
                {
                    target.IsTargetForEffect(true);
                    target.SetClickAction(OnTargetSelected);
                }
            }
        }

        private void OnTargetSelected(Card target)
        {
            if (selectedTargets.Count < upTo)
            {
                selectedTargets.Add(target);
                target.ClearClickAction();
                target.IsTargetForEffect(false);
            }
            if (selectedTargets.Count == upTo)
            {
                CompleteEffect();
            }
        }

        private void CompleteEffect()
        {
            foreach (Card card in selectedTargets)
            {
                card.AddWhenAttackingEffectToCard(
                        new Effects
                        {
                            triggerType = EffectTriggerTypes.WhenAttacking,
                            cardEffect = new WhenAttackingEnemyCantBlockOver(0, -1, false)
                        }
                );
            }
            Cleanup();
        }

        private void CancelEffect()
        {
            CompleteEffect();
            Cleanup();
        }

        private void Cleanup()
        {
            foreach (Card card in possibleTargets)
            {
                card.ClearClickAction();
                card.IsTargetForEffect(false);
            }
            foreach (Card card in cardsThatCouldAttackBeforeEffect)
            {
                switch (card.cardData.cardType)
                {
                    case CardResources.CardType.CHARACTER:
                        card.GetComponent<CharacterCard>().CardCanAttack();
                        break;
                    case CardResources.CardType.LEADER:
                        card.GetComponent<LeaderCard>().CardCanAttack();
                        break;
                    default:
                        break;
                }
            }
            HideCancelButton();
            PlayerBoard.Instance.SetEffectInProgress(false);
            PlayerBoard.Instance.MoveCardToTrash(effectCallerCard);
            activated = true;
        }
        private void ShowCancelButton()
        {
            PlayerBoard.Instance.endOfTurnBtn.gameObject.SetActive(false);
            Button cancelButton = PlayerBoard.Instance.cancelBtn;
            cancelButton.gameObject.SetActive(true);
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CancelEffect);
        }

        private void HideCancelButton()
        {
            PlayerBoard.Instance.endOfTurnBtn.gameObject.SetActive(true);
            Button cancelButton = PlayerBoard.Instance.cancelBtn;
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.gameObject.SetActive(false);
        }
    }

    public class WhenAttackingPlusPower : ICardEffects
    {
        int upTo = 0;
        int powerPlus = 0;
        private bool passiveEffect = true;
        private List<Card> possibleTargets = new List<Card>();
        private List<Card> selectedTargets = new List<Card>();
        private List<Card> cardsThatCouldAttackBeforeEffect = new List<Card>();
        private Card effectCallerCard;

        public WhenAttackingPlusPower(int upTo, int powerPlus, bool passiveEffect)
        {
            this.upTo = upTo;
            this.powerPlus = powerPlus;
            this.passiveEffect = passiveEffect;
        }

        public void Activate(Card card)
        {
            PlayerBoard.Instance.SetEffectInProgress(true);
            if (passiveEffect) { return; }
            effectCallerCard = card;
            cardsThatCouldAttackBeforeEffect = PlayerBoard.Instance.GetCardsThatCouldAttack();
            possibleTargets = PlayerBoard.Instance.GetCharacterAreaCards();
            possibleTargets.Add(PlayerBoard.Instance.leaderCard);
            if (possibleTargets.Count == 0) { return; }
            ShowCancelButton();
            foreach (Card cardCanAttack in cardsThatCouldAttackBeforeEffect)
            {
                switch (cardCanAttack.cardData.cardType)
                {
                    case CardResources.CardType.CHARACTER:
                        cardCanAttack.GetComponent<CharacterCard>().CardCannotAttack();
                        break;
                    case CardResources.CardType.LEADER:
                        cardCanAttack.GetComponent<LeaderCard>().CardCannotAttack();
                        break;
                    default:
                        UnityEngine.Debug.LogError("Missing card type");
                        break;
                }
            }
            foreach (Card target in possibleTargets)
            {
                if (target != card)
                {
                    target.IsTargetForEffect(true);
                    target.SetClickAction(OnTargetSelected);
                }
            }
        }

        private void OnTargetSelected(Card target)
        {
            if (selectedTargets.Count < upTo)
            {
                selectedTargets.Add(target);
                target.ClearClickAction();
                target.IsTargetForEffect(false);
            }
            if (selectedTargets.Count == upTo)
            {
                CompleteEffect();
            }
        }

        private async void CompleteEffect()
        {
            foreach (Card card in selectedTargets)
            {
                await UnityMainThreadDispatcher.RunOnMainThread(async () =>
                {
                    switch (card.cardData.cardType)
                    {
                        case CardResources.CardType.CHARACTER:
                            card.GetComponent<CharacterCard>().AddToPlusPower(powerPlus);
                            card.GetComponent<CharacterCard>().MakeOrUpdatePlusPowerSeenOnCard();

                            break;
                        case CardResources.CardType.LEADER:
                            card.GetComponent<LeaderCard>().AddToPlusPower(powerPlus);
                            card.GetComponent<LeaderCard>().MakeOrUpdatePlusPowerSeenOnCard();
                            break;
                        default:
                            break;
                    }
                    await ServerCon.Instance.AddPlusPowerToCardFromEffectForThisTurn(effectCallerCard.cardData.customCardID, card.cardData.customCardID, powerPlus);
                    card.ClearClickAction();
                });

            }
            Cleanup();
        }

        private void CancelEffect()
        {
            CompleteEffect();
            Cleanup();
        }

        private async void Cleanup()
        {
            foreach (Card card in possibleTargets)
            {
                card.ClearClickAction();
                card.IsTargetForEffect(false);
            }
            foreach (Card card in cardsThatCouldAttackBeforeEffect)
            {
                switch (card.cardData.cardType)
                {
                    case CardResources.CardType.CHARACTER:
                        card.GetComponent<CharacterCard>().CardCanAttack();
                        break;
                    case CardResources.CardType.LEADER:
                        card.GetComponent<LeaderCard>().CardCanAttack();
                        break;
                    default:
                        break;
                }
            }
            await ServerCon.Instance.ImDoneWithWhenAttackingEffect();
            HideCancelButton();
            PlayerBoard.Instance.SetEffectInProgress(false);
        }
        private void ShowCancelButton()
        {
            PlayerBoard.Instance.endOfTurnBtn.gameObject.SetActive(false);
            Button cancelButton = PlayerBoard.Instance.cancelBtn;
            cancelButton.gameObject.SetActive(true);
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CancelEffect);
        }

        private void HideCancelButton()
        {
            Button cancelButton = PlayerBoard.Instance.cancelBtn;
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.gameObject.SetActive(false);
        }
    }

    public class SimpleDonAttachedEffect : ICardEffects
    {
        int howManyDon;
        string effectType;
        public SimpleDonAttachedEffect(int howManyDon, string effect)
        {
            this.howManyDon = howManyDon;
            this.effectType = effect;
        }

        public void Activate(Card card)
        {
            switch (effectType.ToLower())
            {
                case "rush":
                    GetRush(card);
                    break;
                case "blocker":
                    GetBlocker(card);
                    break;
                default:
                    UnityEngine.Debug.LogError("Missing effect type");
                    break;
            }
        }

        private void GetRush(Card card)
        {
            if (!card.rested && !card.canAttack && card.GetAttachedDonCount() >= howManyDon)
            {
                switch (card.cardData.cardType)
                {
                    case CardResources.CardType.CHARACTER:
                        card.GetComponent<CharacterCard>().CardCanAttack();
                        break;
                    default:
                        UnityEngine.Debug.LogError("Missing card type");
                        break;
                }
            }
        }

        private void GetBlocker(Card card)
        {
            //This simply added for possible future implementation
        }

    }


    public class Rush : ICardEffects
    {

        public Rush()
        {
        }

        public void Activate(Card card)
        {
            if (!card.rested && !card.canAttack && card.transform.parent.parent == PlayerBoard.Instance.transform)
            {
                switch (card.cardData.cardType)
                {
                    case CardResources.CardType.CHARACTER:
                        card.GetComponent<CharacterCard>().CardCanAttack();
                        break;
                    default:
                        UnityEngine.Debug.LogError("Missing card type");
                        break;
                }
            }
        }
    }

    public class KoEnemyBasedOnPowerOrLess : ICardEffects
    {
        int upTo;
        int powerOrLess;
        bool activated = false;

        private List<Card> possibleTargets = new List<Card>();
        private List<Card> cardsThatCouldAttackBeforeOnPlay = new List<Card>();
        private List<Card> selectedTargets = new List<Card>();
        private Card effectCaller;

        public KoEnemyBasedOnPowerOrLess(int upTo, int powerOrLess)
        {
            this.upTo = upTo;
            this.powerOrLess = powerOrLess;
        }

        public void Activate(Card card)
        {
            if (GameManager.Instance.currentPlayerTurnPhase == PlayerTurnPhases.MAINPHASE && !activated)
            {
                PlayerBoard.Instance.SetEffectInProgress(true);
                cardsThatCouldAttackBeforeOnPlay = PlayerBoard.Instance.GetCardsThatCouldAttack();
                possibleTargets = EnemyBoard.Instance.GetCharacterAreaCardsUnderThisPower(powerOrLess);
                effectCaller = card;
                if (possibleTargets.Count == 0) { return; }
                card.transform.position = PlayerBoard.Instance.leaderCard.transform.position;
                card.transform.Translate(-150, 0, 0);
                foreach (Card cardCanAttack in cardsThatCouldAttackBeforeOnPlay)
                {
                    switch (cardCanAttack.cardData.cardType)
                    {
                        case CardResources.CardType.CHARACTER:
                            cardCanAttack.GetComponent<CharacterCard>().CardCannotAttack();
                            break;
                        case CardResources.CardType.LEADER:
                            cardCanAttack.GetComponent<LeaderCard>().CardCannotAttack();
                            break;
                        default:
                            UnityEngine.Debug.LogError("Missing card type");
                            break;
                    }
                }
                ShowCancelButton();
                foreach (Card target in possibleTargets)
                {
                    target.IsTargetForEffect(true);
                    target.SetClickAction(OnTargetSelected);
                }
            }
        }

        private void OnTargetSelected(Card target)
        {
            if (selectedTargets.Count < upTo)
            {
                selectedTargets.Add(target);
                target.IsTargetForEffect(false);
                target.ClearClickAction();
            }
            if (selectedTargets.Count == upTo)
            {
                CompleteEffect();
            }
        }

        private async void CompleteEffect()
        {
            foreach (Card card in selectedTargets)
            {
                await UnityMainThreadDispatcher.RunOnMainThread(async () =>
                {
                    await ServerCon.Instance.KoThisCard(card.cardData.customCardID, effectCaller.cardData.customCardID);
                });
            }
            Cleanup();
        }

        private void CancelEffect()
        {
            CompleteEffect();
            Cleanup();
        }

        private void Cleanup()
        {
            foreach (Card card in possibleTargets)
            {
                card.ClearClickAction();
                card.IsTargetForEffect(false);
            }
            foreach (Card card in cardsThatCouldAttackBeforeOnPlay)
            {
                switch (card.cardData.cardType)
                {
                    case CardResources.CardType.CHARACTER:
                        card.GetComponent<CharacterCard>().CardCanAttack();
                        break;
                    case CardResources.CardType.LEADER:
                        card.GetComponent<LeaderCard>().CardCanAttack();
                        break;
                    default:
                        break;
                }
            }
            HideCancelButton();
            PlayerBoard.Instance.SetEffectInProgress(false);
            PlayerBoard.Instance.MoveCardToTrash(effectCaller);
            activated = true;
        }

        private void ShowCancelButton()
        {
            PlayerBoard.Instance.endOfTurnBtn.gameObject.SetActive(false);
            Button cancelButton = PlayerBoard.Instance.cancelBtn;
            cancelButton.gameObject.SetActive(true);
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CancelEffect);
        }

        private void HideCancelButton()
        {
            Button cancelButton = PlayerBoard.Instance.cancelBtn;
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.gameObject.SetActive(false);
            PlayerBoard.Instance.endOfTurnBtn.gameObject.SetActive(true);
        }

    }

    public class EventGiveCounter : ICardEffects
    {
        int upTo;
        int counterPower;
        bool activated = false;
        public EventGiveCounter(int upTo, int counterPower)
        {
            this.upTo = upTo;
            this.counterPower = counterPower;
        }

        public void Activate(Card card)
        {
            if (PlayerBoard.Instance.activeDon >= card.cardData.cost && GameManager.Instance.currentBattlePhase == BattlePhases.COUNTERSTEP && !activated)
            {
                card.IsTargetForEffect(true);
                card.SetClickAction(OnCounterClicked);
            }
        }

        public void OnCounterClicked(Card card)
        {
            activated = true;
            card.ClearClickAction();
            card.RemoveBorderForThisCard();
            PlayerBoard.Instance.MoveCardToTrash(card);
            PlayerBoard.Instance.GiveCounterToCurrentlyAttackedCard(card, counterPower);
        }

    }

    public class RestStageGivePowerToType : ICardEffects
    {
        int upTo;
        int plusPower;
        string subType;
        bool passiveEffect = false;

        private List<Card> possibleTargets = new List<Card>();
        private List<Card> cardsThatCouldAttackBeforeOnPlay = new List<Card>();
        private List<Card> selectedTargets = new List<Card>();
        private Card effectCaller;
        public RestStageGivePowerToType(int upTo, int plusPower, string subType, bool passiveEffect)
        {
            this.upTo = upTo;
            this.plusPower = plusPower;
            this.subType = subType;
            this.passiveEffect = passiveEffect;
        }

        public void Activate(Card card)
        {
            if (!card.rested)
            {
                effectCaller = card;
                card.Rest();
                PlayerBoard.Instance.SetEffectInProgress(true);
                cardsThatCouldAttackBeforeOnPlay = PlayerBoard.Instance.GetCardsThatCouldAttack();
                possibleTargets = PlayerBoard.Instance.GetCharacterAreaCards();
                possibleTargets.Add(PlayerBoard.Instance.leaderCard);
                if (possibleTargets.Count == 0) { return; }
                foreach (Card cardCanAttack in cardsThatCouldAttackBeforeOnPlay)
                {
                    switch (cardCanAttack.cardData.cardType)
                    {
                        case CardResources.CardType.CHARACTER:
                            cardCanAttack.GetComponent<CharacterCard>().CardCannotAttack();
                            break;
                        case CardResources.CardType.LEADER:
                            cardCanAttack.GetComponent<LeaderCard>().CardCannotAttack();
                            break;
                        default:
                            UnityEngine.Debug.LogError("Missing card type");
                            break;
                    }
                }
                ShowCancelButton();
                foreach (Card target in possibleTargets)
                {
                    target.IsTargetForEffect(true);
                    target.SetClickAction(OnTargetSelected);
                }
            }
        }

        private void OnTargetSelected(Card target)
        {
            if (selectedTargets.Count < upTo)
            {
                if (!EnumUtils.GetEnumMemberValue(target.cardData.characterType).ToString().Contains(subType))
                {
                    ChatManager.Instance.AddMessage("The target: " + target.cardData.customCardID + " for effect is not the correct sub type!");
                }
                else
                {
                    selectedTargets.Add(target);
                    target.IsTargetForEffect(false);
                    target.ClearClickAction();
                }
            }
            if (selectedTargets.Count == upTo)
            {
                CompleteEffect();
            }
        }

        private async void CompleteEffect()
        {
            foreach (Card card in selectedTargets)
            {
                await UnityMainThreadDispatcher.RunOnMainThread(async () =>
                {
                    card.AddToPlusPower(plusPower);
                    card.MakeOrUpdatePlusPowerSeenOnCard();
                    await ServerCon.Instance.AddPlusPowerToCardFromEffectForThisTurn(effectCaller.cardData.customCardID, card.cardData.customCardID, plusPower);
                });
            }
            Cleanup();
        }

        private void CancelEffect()
        {
            CompleteEffect();
            Cleanup();
        }

        private void Cleanup()
        {
            foreach (Card card in possibleTargets)
            {
                card.ClearClickAction();
                card.IsTargetForEffect(false);
            }
            foreach (Card card in cardsThatCouldAttackBeforeOnPlay)
            {
                switch (card.cardData.cardType)
                {
                    case CardResources.CardType.CHARACTER:
                        card.GetComponent<CharacterCard>().CardCanAttack();
                        break;
                    case CardResources.CardType.LEADER:
                        card.GetComponent<LeaderCard>().CardCanAttack();
                        break;
                    default:
                        break;
                }
            }
            HideCancelButton();
            PlayerBoard.Instance.SetEffectInProgress(false);
        }

        private void ShowCancelButton()
        {
            PlayerBoard.Instance.endOfTurnBtn.gameObject.SetActive(false);
            Button cancelButton = PlayerBoard.Instance.cancelBtn;
            cancelButton.gameObject.SetActive(true);
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CancelEffect);
        }

        private void HideCancelButton()
        {
            Button cancelButton = PlayerBoard.Instance.cancelBtn;
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.gameObject.SetActive(false);
            PlayerBoard.Instance.endOfTurnBtn.gameObject.SetActive(true);
        }

    }

    public class GiveUpToRestedDonToLeaderOrCharacter : ICardEffects
    {
        int upTo;
        int toThisMany;

        private List<Card> selectedDons = new List<Card>();
        private List<Card> restedDons = new List<Card>();
        private List<Card> possibleTargets = new List<Card>();
        private List<Card> cardsThatCouldAttackBeforeOnPlay = new List<Card>();
        private Card selectedTarget;
        private bool oncePerTurn;
        private bool activated = false;
        private bool passiveEffect = true;

        public GiveUpToRestedDonToLeaderOrCharacter(int upTo, int toThisMany, bool oncePerTurn, bool passiveEffect)
        {
            this.upTo = upTo;
            this.toThisMany = toThisMany;
            this.oncePerTurn = oncePerTurn;
            this.passiveEffect = passiveEffect;
        }

        public void Activate(Card card)
        {
            if (passiveEffect) { return; }
            if (activated && oncePerTurn) { return; }
            PlayerBoard.Instance.SetEffectInProgress(true);
            activated = true;
            cardsThatCouldAttackBeforeOnPlay = PlayerBoard.Instance.GetCardsThatCouldAttack();
            restedDons = PlayerBoard.Instance.GetRestedDons();
            possibleTargets = PlayerBoard.Instance.GetCharacterAreaCards();
            possibleTargets.Add(PlayerBoard.Instance.leaderCard);
            if (restedDons.Count == 0 || possibleTargets.Count == 0)
            {
                return;
            }
            foreach (Card cardCanAttack in cardsThatCouldAttackBeforeOnPlay)
            {
                switch (cardCanAttack.cardData.cardType)
                {
                    case CardResources.CardType.CHARACTER:
                        cardCanAttack.GetComponent<CharacterCard>().CardCannotAttack();
                        break;
                    case CardResources.CardType.LEADER:
                        cardCanAttack.GetComponent<LeaderCard>().CardCannotAttack();
                        break;
                    default:
                        UnityEngine.Debug.LogError("Missing card type");
                        break;
                }
            }
            ShowCancelButton();
            foreach (Card target in possibleTargets)
            {
                target.IsTargetForEffect(true);
                target.SetClickAction(OnTargetSelected);
            }
        }

        private void OnTargetSelected(Card target)
        {
            selectedTarget = target;

            foreach (Card targetCard in possibleTargets)
            {
                targetCard.ClearClickAction();
                targetCard.IsTargetForEffect(false);
            }

            foreach (Card don in restedDons)
            {
                don.IsTargetForEffect(true);
                don.SetClickAction(OnDonSelected);
            }
        }

        private void OnDonSelected(Card don)
        {
            if (selectedDons.Count < upTo)
            {
                selectedDons.Add(don);
                don.ClearClickAction();
                don.IsTargetForEffect(false);
            }
            if (selectedDons.Count == upTo)
            {
                CompleteEffect();
            }
        }

        private async void CompleteEffect()
        {
            foreach (Card donCard in selectedDons)
            {
                await UnityMainThreadDispatcher.RunOnMainThread(() =>
                {
                    donCard.AttachDon(selectedTarget, donCard);
                    donCard.ClearClickAction();
                    restedDons.Remove(donCard);
                    donCard.SendCardToServer();
                });
            }
            Cleanup();
        }

        private void CancelEffect()
        {
            CompleteEffect();
            Cleanup();
        }

        private void Cleanup()
        {
            foreach (Card card in possibleTargets)
            {
                card.ClearClickAction();
                card.IsTargetForEffect(false);
            }
            foreach (Card card in restedDons)
            {
                card.ClearClickAction();
                card.IsTargetForEffect(false);
            }
            foreach (Card card in cardsThatCouldAttackBeforeOnPlay)
            {
                switch (card.cardData.cardType)
                {
                    case CardResources.CardType.CHARACTER:
                        card.GetComponent<CharacterCard>().CardCanAttack();
                        break;
                    case CardResources.CardType.LEADER:
                        card.GetComponent<LeaderCard>().CardCanAttack();
                        break;
                    default:
                        break;
                }
            }
            HideCancelButton();
            PlayerBoard.Instance.SetEffectInProgress(false);
        }
        private void ShowCancelButton()
        {
            PlayerBoard.Instance.endOfTurnBtn.gameObject.SetActive(false);
            Button cancelButton = PlayerBoard.Instance.cancelBtn;
            cancelButton.gameObject.SetActive(true);
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CancelEffect);
        }

        private void HideCancelButton()
        {
            Button cancelButton = PlayerBoard.Instance.cancelBtn;
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.gameObject.SetActive(false);
            PlayerBoard.Instance.endOfTurnBtn.gameObject.SetActive(true);
        }
    }

    public static class EnumUtils
    {
        public static string GetEnumMemberValue<T>(T enumValue) where T : Enum
        {
            var memberInfo = typeof(T).GetMember(enumValue.ToString()).FirstOrDefault();
            if (memberInfo != null)
            {
                var attribute = memberInfo.GetCustomAttribute<EnumMemberAttribute>();
                if (attribute != null)
                {
                    return attribute.Value;
                }
            }

            return enumValue.ToString();
        }
    }
}
