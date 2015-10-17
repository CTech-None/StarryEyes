﻿using System.Windows;
using StarryEyes.ViewModels.Notifications;

namespace StarryEyes.Views.Notifications
{
    /// <summary>
    /// NormalNotificatorView.xaml の相互作用ロジック
    /// </summary>
    public partial class NormalNotificatorView : Window
    {
        public NormalNotificatorView()
        {
            InitializeComponent();
        }

        private void NormalNotificatorView_OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as NormalNotificatorViewModel;
            if (viewModel == null) return;
            Left = viewModel.Left;
            Top = viewModel.Top;
        }
    }
}
