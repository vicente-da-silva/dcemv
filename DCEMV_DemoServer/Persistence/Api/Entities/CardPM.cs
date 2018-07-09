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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DCEMV.ServerShared;

namespace DCEMV.DemoServer.Persistence.Api.Entities
{


    public class CardPM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CardId { get; set; }

        public string CardSerialNumberId { get; set; }

        [StringLength(200)]
        public string FreindlyName { get; set; }

        public long DailySpendLimit { get; set; }
        public long AvailablegDailySpendLimit { get; set; }
        public long MonthlySpendLimit { get; set; }
        public long AvailableMonthlySpendLimit { get; set; }
        [Required]
        public CardState CardState { get; set; }

        /***************************/
        //Foreign Keys
        //[Required]
        public string AccountNumberIdRef { get; set; }
        //[ForeignKey("AccountNumberId")]
        public AccountPM Account { get; set; }
        /***************************/

        public CardPM()
        {
        }

        public void UpdateCard(CardPM card)
        {
            DailySpendLimit = card.DailySpendLimit;
            MonthlySpendLimit = card.MonthlySpendLimit;
            FreindlyName = card.FreindlyName;
        }
    }
}
