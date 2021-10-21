using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpenDoorApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenDoorApp.Services.Implementatios
{
    public class OnboardService : IOnboardService
    {
        public bool IsOnboarding { get; set; }
    }
}