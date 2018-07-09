/*
*************************************************************************
DC EMV
Open Source EMV
Copyright (C) 2018  Vicente Da Silva

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see http://www.gnu.org/licenses/
*************************************************************************
*/
using System.Collections.Generic;
using System.Linq;
using DCEMV.DemoServer.Persistence.Api.Entities;
using DCEMV.ServerShared;

namespace DCEMV.DemoServer.Persistence.Api.Repository
{
    public interface ICardsRepository
    {
        CardPM GetCard(string cardSerialNumber);
        void AddCard(CardPM card, string credentialsId);
        void CancelCard(string cardSerialNumber, string credentialsId);
        void UpdateCard(CardPM card, string credentialsId);
        List<CardPM> GetCardsForAccount(string credentialsId);
    }

    public class CardsRepository : ICardsRepository
    {
        private readonly ApiDbContext _context = null;

        private readonly IAccountsRepository _accountsRepository;

        public CardsRepository(ApiDbContext context, IAccountsRepository accountsRepository)
        {
            _accountsRepository = accountsRepository;
            _context = context;
        }

        public CardPM GetCard(string cardSerialNumber)
        {
            List<CardPM> cards = _context.Cards.Where(x => x.CardSerialNumberId == cardSerialNumber).ToList();
            if(cards.Count == 0)
                throw new ValidationException("No card found");
            if (cards.Count > 1)
                throw new ValidationException("More than 1 card found");

            return cards[0];
        }

        public void AddCard(CardPM card, string credentialsId)
        {
            if (_context.Cards.Count(x => x.CardSerialNumberId == card.CardSerialNumberId) > 0)
                throw new ValidationException("Card already in use");

            card.AccountNumberIdRef = _accountsRepository.GetAccountForUser(credentialsId).AccountNumberId;

            _context.Cards.Add(card);
            _context.SaveChanges();
        }

        public void CancelCard(string cardSerialNumber, string credentialsId)
        {
            CardPM card = GetCard(cardSerialNumber);
            AccountPM accountLoggedIn = _accountsRepository.GetAccountForUser(credentialsId);

            if (accountLoggedIn.AccountNumberId != card.AccountNumberIdRef)
                throw new ValidationException("Invalid AccountNumberId");

            card.CardState = CardState.Cancelled;
            _context.SaveChanges();
        }

        public void UpdateCard(CardPM card, string credentialsId)
        {
            CardPM cardToUpdate = GetCard(card.CardSerialNumberId);
            AccountPM accountLoggedIn = _accountsRepository.GetAccountForUser(credentialsId);

            if (accountLoggedIn.AccountNumberId != cardToUpdate.AccountNumberIdRef)
                throw new ValidationException("Invalid AccountNumberId");

            cardToUpdate.UpdateCard(card);
            cardToUpdate.AvailablegDailySpendLimit = cardToUpdate.DailySpendLimit;
            cardToUpdate.AvailableMonthlySpendLimit = cardToUpdate.MonthlySpendLimit;
            _context.SaveChanges();
        }

        public List<CardPM> GetCardsForAccount(string credentialsId)
        {
            string accountLoggedIn = _accountsRepository.GetAccountForUser(credentialsId).AccountNumberId;

            List<CardPM> cards = _context.Cards.Where(x => x.Account.AccountNumberId == accountLoggedIn).ToList();
            return cards;
        }
    }
}
