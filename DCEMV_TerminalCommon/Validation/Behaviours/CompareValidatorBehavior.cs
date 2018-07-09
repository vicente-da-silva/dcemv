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
using Xamarin.Forms;

namespace DCEMV.TerminalCommon
{
    public class CompareValidatorBehavior : BaseEntryBehaviour
    {
        public static readonly BindableProperty CompareToEntryProperty = BindableProperty.Create("CompareToEntry", typeof(Entry), typeof(CompareValidatorBehavior), null);
        private string localEntryText;

        public Entry CompareToEntry
        {
            get { return (Entry)base.GetValue(CompareToEntryProperty); }
            set{ base.SetValue(CompareToEntryProperty, value);}
        }

        protected override void OnAttachedTo(Entry entry)
        {
            base.OnAttachedTo(entry);
            if (CompareToEntry != null)
            {
                localEntryText = ""; ;
                CompareToEntry.TextChanged += HandleCompareTextChanged;
            }
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            if (CompareToEntry != null)
            {
                localEntryText = "";
                CompareToEntry.TextChanged -= HandleCompareTextChanged;
            }
            base.OnDetachingFrom(entry);
        }

        void HandleCompareTextChanged(object sender, TextChangedEventArgs e)
        {
            if (localEntryText == "")
            {
                IsValid = false;
                return;
            }

            var second = e.NewTextValue;
            IsValid = (bool)localEntryText?.Equals(second);
            //Debug.WriteLine("Cpmpare HandleCompareTextChanged:" + IsValid);
        }
        public override void HandleTextChanged(object sender, TextChangedEventArgs e)
        {
            var first = CompareToEntry.Text;
            localEntryText = e.NewTextValue;

            if (localEntryText == "")
            {
                IsValid = false;
                return;
            }

            IsValid = (bool)first?.Equals(localEntryText);
            //Debug.WriteLine("Cpmpare HandleLocalCompareTextChanged:" + IsValid);
        }
    }
}
