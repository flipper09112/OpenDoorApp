using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenDoorApp.Helpers
{
    public static class GradientHelper
    {
        public static GradientDrawable GetBackground()
        {
            GradientDrawable gradient = new GradientDrawable(GradientDrawable.Orientation.TopBottom,
                new int[] {
                    Color.ParseColor("#98654e"),
                    Color.ParseColor("#a86462"),
                    Color.ParseColor("#b1677c"),
                    Color.ParseColor("#af6e9a"),
                    Color.ParseColor("#9f7bb8"),
                    Color.ParseColor("#a485c0"),
                    Color.ParseColor("#aa8fc8"),
                    Color.ParseColor("#b099d0"),
                    Color.ParseColor("#cca4ca"),
                    Color.ParseColor("#dcb3c6"),
                    Color.ParseColor("#e3c5ca"),
                    Color.ParseColor("#e5d7d6")
            });

            return gradient;
        }
    }
}