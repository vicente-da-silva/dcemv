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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DCEMV.FormattingUtils;
using DCEMV.DemoServer.Persistence.Api.Repository;
using DCEMV.DemoServer.Persistence.Api.Entities;
using DCEMV.ServerShared;

namespace DCEMV.DemoServer.Controllers.Api
{
    [Authorize]
    public class CardController : Controller
    {
        private readonly ICardsRepository _cardsRepository;
        private readonly IAccountsRepository _accountsRepository;

        public CardController(ICardsRepository cardsRepository, IAccountsRepository accountsRepository)
        {
            _cardsRepository = cardsRepository;
            _accountsRepository = accountsRepository;
        }
        
        [HttpPost]
        [Route("card/registercard")]
        public void RegisterCard(string cardSerialNumberId)
        {
            //Card card = Card.FromJsonString(json);

            if (!Validate.CardSerialNumberValidation(cardSerialNumberId))
                throw new ValidationException("Invalid Card Number");
            
            CardPM cardpm = new CardPM()
            {
                CardSerialNumberId = cardSerialNumberId,
                CardState = CardState.Active,
            };
            _cardsRepository.AddCard(cardpm, GetCurrentUserId());
        }

        [HttpPost]
        [Route("card/cancelcard")]
        public void CancelCard(string cardSerialNumberId)
        {
            if (!Validate.CardSerialNumberValidation(cardSerialNumberId))
                throw new ValidationException("Invalid Card Number");

            _cardsRepository.CancelCard(cardSerialNumberId, GetCurrentUserId());
        }

        [HttpPost]
        [Route("card/updatecarddetails")]
        public void UpdateCardDetails(string json)
        {
            Card card = Card.FromJsonString(json);

            if (!Validate.CardSerialNumberValidation(card.CardSerialNumberId))
                throw new ValidationException("Invalid Card Number");

            _cardsRepository.UpdateCard(
                new CardPM()
                {
                    CardSerialNumberId = card.CardSerialNumberId,
                    FreindlyName = card.FreindlyName,
                    DailySpendLimit = card.DailySpendLimit,
                    MonthlySpendLimit = card.MonthlySpendLimit,
                }, GetCurrentUserId());
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst("sub").Value;
        }
    }
}
