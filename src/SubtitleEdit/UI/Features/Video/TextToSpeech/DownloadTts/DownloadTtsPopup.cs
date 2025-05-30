//using CommunityToolkit.Maui.Views;
//using Microsoft.Maui.Controls.Shapes;
//using SubtitleAlchemist.Logic;
//using SubtitleAlchemist.Logic.Constants;

//namespace SubtitleAlchemist.Features.Video.TextToSpeech.DownloadTts;

//public sealed class DownloadTtsPopup : Popup
//{
//    public DownloadTtsPopup(DownloadTtsPopupModel vm)
//    {
//        BindingContext = vm;

//        this.BindDynamicTheme();
//        CanBeDismissedByTappingOutsideOfPopup = false;

//        var grid = new Grid
//        {
//            RowDefinitions =
//            {
//                new RowDefinition { Height = GridLength.Auto },
//                new RowDefinition { Height = GridLength.Auto },
//                new RowDefinition { Height = GridLength.Auto },
//                new RowDefinition { Height = GridLength.Auto },
//            },
//            ColumnDefinitions =
//            {
//                new ColumnDefinition { Width = GridLength.Auto },
//            },
//            Margin = new Thickness(2),
//            Padding = new Thickness(30, 20, 30 ,10),
//            RowSpacing = 20,
//            ColumnSpacing = 10,
//            HorizontalOptions = LayoutOptions.Fill,
//            VerticalOptions = LayoutOptions.Fill,
//        }.BindDynamicTheme();

//        var titleLabel = new Label
//        {
//            FontAttributes = FontAttributes.Bold,
//            FontSize = 18,
//            Padding = new Thickness(0, 0, 0, 10),
//        }.BindDynamicTheme();
//        titleLabel.SetBinding(Label.TextProperty, nameof(vm.TitleText));
//        grid.Add(titleLabel, 0, 0);

//        var progressLabel = new Label
//        {
//            Text = "...",
//            FontAttributes = FontAttributes.Bold,
//        }.BindDynamicTheme(); 
//        progressLabel.SetBinding(Label.TextProperty, nameof(vm.Progress));
//        grid.Add(progressLabel, 0, 1);

//        var progressBar = new ProgressBar
//        {
//            Progress = 0.5,
//            ProgressColor = (Color)Application.Current!.Resources[ThemeNames.ProgressColor],
//            HorizontalOptions = LayoutOptions.Fill,
//        };
//        progressBar.SetBinding(ProgressBar.ProgressProperty, nameof(vm.ProgressValue));
//        grid.Add(progressBar, 0, 2);

//        var cancelButton = new Button
//        {
//            Text = "Cancel",
//            HorizontalOptions = LayoutOptions.Center,
//            Command = vm.CancelCommand,
//        }.BindDynamicTheme();
//        grid.Add(cancelButton, 0, 3);

//        var border = new Border
//        {
//            StrokeThickness = 1,
//            Padding = new Thickness(4, 1, 1, 0),
//            Margin = new Thickness(2),
//            HorizontalOptions = LayoutOptions.Fill,
//            VerticalOptions = LayoutOptions.Fill,
//            StrokeShape = new RoundRectangle
//            {
//                CornerRadius = new CornerRadius(5)
//            },
//            Content = grid,
//        }.BindDynamicTheme();

//        Content = border;

//        vm.Popup = this;
//    }
//}