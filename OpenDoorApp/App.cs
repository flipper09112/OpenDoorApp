using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Autofac;
using OpenDoorApp.Services;
using OpenDoorApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using IContainer = Autofac.IContainer;

namespace OpenDoorApp
{
    [Application]
    public class App : Application
    {
        public static IContainer Container { get; set; }

        public App(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {

        }

        public override void OnCreate()
        {
            Initialize();

            base.OnCreate();
        }

        private static void Initialize()
        {
            var builder = new ContainerBuilder();

            RegisterServices(builder);

            App.Container = builder.Build();
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<BluetoothService>().As<IBluetoothService>().SingleInstance();
        }
    }
}