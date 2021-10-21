using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenDoorApp.Helpers
{
    public static class FragmentsHelper
    {
        public static void ShowFragment(this Activity activity, Fragment fragment, string addToBackStackStr = null)
        {
            FragmentTransaction transaction = activity.FragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.fragmentContainer, fragment);
           
            if(addToBackStackStr != null)
                transaction.AddToBackStack(null);
            
            transaction.Commit();
        }
    }
}