using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenDoorApp.UI.ViewHolders
{
    public class DeviceItemViewHolder : RecyclerView.ViewHolder
    {
        private CardView _card;
        private TextView _deviceName;
        private Action<int> _updateList;

        public DeviceItemViewHolder(View itemView) : base(itemView)
        {
            _card = itemView.FindViewById<CardView>(Resource.Id.card);
            _deviceName = itemView.FindViewById<TextView>(Resource.Id.deviceName);

            _card.Click -= CardClick;
            _card.Click += CardClick;
        }

        private void CardClick(object sender, EventArgs e)
        {
            _updateList.Invoke(AdapterPosition);
        }

        internal void Bind(string deviceName, bool selected, Action<int> updateList)
        {
            _updateList = updateList;

            _deviceName.Text = deviceName;

            if (selected)
                _deviceName.SetBackgroundResource(Resource.Color.OrganzaViolet);
            else
                _deviceName.Background = null;
        }
    }
}