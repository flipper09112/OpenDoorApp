using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.ConstraintLayout.Widget;
using OpenDoorApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenDoorApp.UI.Fragments.Onboard
{
    [Obsolete]
    public class CloseToDeviceFragment : Fragment
    {
        private MainActivity _mainActivity;
        private ConstraintLayout _container;
        private AppCompatButton _nextBtn;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.CloseToDeviceFragment, container, false);

            _mainActivity = (MainActivity)Activity;

            _container = view.FindViewById<ConstraintLayout>(Resource.Id.container);
            _nextBtn = view.FindViewById<AppCompatButton>(Resource.Id.nextBtn);

            return view;
        }

        public override void OnPause()
        {
            base.OnPause();

            CleanBindings();
        }

        public override void OnResume()
        {
            base.OnResume();

            SetUI();
            SetupBindings();
        }

        private void SetUI()
        {
            _container.SetBackgroundDrawable(GradientHelper.GetBackground());
        }

        private void SetupBindings()
        {
            _nextBtn.Click += NextBtnClick;
        }

        private void CleanBindings()
        {
            _nextBtn.Click -= NextBtnClick;
        }

        private void NextBtnClick(object sender, EventArgs e)
        {
            FragmentsHelper.ShowFragment(Activity, new SearchDevicesToPairFragment(), nameof(SearchDevicesToPairFragment));
        }
    }
}