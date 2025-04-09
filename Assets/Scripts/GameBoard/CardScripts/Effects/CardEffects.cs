using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public DonPowerBonus(int howMuchDonNeeded, int howMuchPower)
        {
            this.howMuchDonNeeded = howMuchDonNeeded;
            this.howMuchPower = howMuchPower;
        }

        public void Activate(Card card)
        {
            if (card.hasDonAttached && card.GetAttachedDonCount() >= howMuchDonNeeded)
            {
                card.AddToPlusPower(howMuchPower);
                card.MakeOrUpdatePlusPowerSeenOnCard();
            }
        }
    }

    public class Rush : ICardEffects
    {
        public void Activate(Card card)
        {

        }
    }

    public class OnPlayGiveUpToRestedDonToLeaderOrCharacter : ICardEffects
    {
        int upTo;
        int toThisMany;

        private List<Card> selectedDons = new List<Card>();
        private List<Card> restedDons = new List<Card>();
        private List<Card> possibleTargets = new List<Card>();
        private List<Card> cardsThatCouldAttackBeforeOnPlay = new List<Card>();
        private Card selectedTarget;

        public OnPlayGiveUpToRestedDonToLeaderOrCharacter(int upTo, int toThisMany)
        {
            this.upTo = upTo;
            this.toThisMany = toThisMany;
        }

        public void Activate(Card card)
        {
            cardsThatCouldAttackBeforeOnPlay = PlayerBoard.Instance.GetCardsThatCouldAttack();
            restedDons = PlayerBoard.Instance.GetRestedDons();
            possibleTargets = PlayerBoard.Instance.characterAreaCards;
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
                        break;
                }
            }
            ShowCancelButton();
            foreach (Card target in possibleTargets)
            {
                target.IsTargetForOnPlayEffect(true);
                target.SetClickAction(OnTargetSelected);
            }
        }

        private void OnTargetSelected(Card target)
        {
            selectedTarget = target;

            foreach (Card targetCard in possibleTargets)
            {
                targetCard.ClearClickAction();
                targetCard.IsTargetForOnPlayEffect(false);
            }

            foreach (Card don in restedDons)
            {
                don.IsTargetForOnPlayEffect(true);
                don.SetClickAction(OnDonSelected);
            }
        }

        private void OnDonSelected(Card don)
        {
            if (selectedDons.Count < upTo)
            {
                selectedDons.Add(don);
                don.ClearClickAction();
                don.IsTargetForOnPlayEffect(false);
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
                    donCard.IsTargetForOnPlayEffect(false);
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
                card.IsTargetForOnPlayEffect(false);
            }
            foreach (Card card in restedDons)
            {
                card.ClearClickAction();
                card.IsTargetForOnPlayEffect(false);
            }
            selectedTarget.EnableCanvasOverrideSorting();
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
}
