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
using System;
using Xamarin.Forms;

namespace DCEMV.TerminalCommon
{
    public class ItemPickedValidatorBehavior : Behavior<Picker>
    {
        static readonly BindablePropertyKey IsValidPropertyKey = BindableProperty.CreateReadOnly("IsValid", typeof(bool), typeof(ItemPickedValidatorBehavior), false);

        public static readonly BindableProperty IsValidProperty = IsValidPropertyKey.BindableProperty;

        public bool IsValid
        {
            get { return (bool)base.GetValue(IsValidProperty); }
            protected set { base.SetValue(IsValidPropertyKey, value); }
        }

        protected override void OnAttachedTo(Picker picker)
        {
            base.OnAttachedTo(picker);
            picker.SelectedIndexChanged += Picker_SelectedIndexChanged;
        }

        protected override void OnDetachingFrom(Picker picker)
        {
            picker.SelectedIndexChanged -= Picker_SelectedIndexChanged;
            base.OnDetachingFrom(picker);
        }

        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            IsValid = (sender as Picker).SelectedIndex == -1 ? false : true; 
        }
    }
}

