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
        private SelectDoorDeviceFragment _frag;
        private Action<int> _selectDevicePosition;
        private List<string> pairedDevices;

        public SelectDoorDeviceAdapter(List<string> pairedDevices, Action<int> selectDevicePosition, SelectDoorDeviceFragment selectDoorDeviceFragment)
        {
            _frag = selectDoorDeviceFragment;
            _selectDevicePosition = selectDevicePosition;
            this.pairedDevices = pairedDevices;
        }

        public override int ItemCount => pairedDevices?.Count ?? 0;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if(holder is DeviceItemViewHolder deviceItemVH)
            {
                deviceItemVH.Bind(pairedDevices[holder.AdapterPosition], holder.AdapterPosition == _frag.SelectedDevicePosition, UpdateList);
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