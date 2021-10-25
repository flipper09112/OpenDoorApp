using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using OpenDoorApp.UI.Fragments;
using OpenDoorApp.UI.ViewHolders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenDoorApp.UI.Adapters
{
    public class SelectDoorDeviceAdapter : RecyclerView.Adapter
    {
        private Action<int> _selectDevicePosition;
        public List<string> PairedDevices;
        private Func<int> getSelectDevicePosition;

        public SelectDoorDeviceAdapter(List<string> pairedDevices, Action<int> selectDevicePosition, Func<int> getSelectDevicePosition)
        {
            this.PairedDevices = pairedDevices;
            _selectDevicePosition = selectDevicePosition;
            this.getSelectDevicePosition = getSelectDevicePosition;
        }

        public override int ItemCount => PairedDevices?.Count ?? 0;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if(holder is DeviceItemViewHolder deviceItemVH)
            {
                deviceItemVH.Bind(PairedDevices[holder.AdapterPosition], holder.AdapterPosition == getSelectDevicePosition.Invoke(), UpdateList);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.DeviceItem, parent, false);
            return new DeviceItemViewHolder(view);
        }

        public void UpdateList(int pos)
        {
            _selectDevicePosition.Invoke(pos);
            NotifyDataSetChanged();
        }
    }
}