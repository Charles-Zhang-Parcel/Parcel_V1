﻿using System.Windows;

namespace Parcel.FrontEnd.NodifyWPF
{
    public partial class PropertyWindow : BaseWindow
    {
        public PropertyWindow(Window owner)
        {
            Owner = owner;
            InitializeComponent();
        }
    }
}