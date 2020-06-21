using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace SpeedViewTest
{
    public class MainPage : ContentPage
    {
        private Label _captionLabel;
        private AbsoluteLayout _absoluteLayout;
        private Button _startButton;
        private Button _stopButton;
        private Random _random;
        private Stopwatch _stopWatch;
        private bool _isTestRunning;
        private double _snapShot;
        private int _totalElements;
        private int _replaceIndex;

        private const int ThresholdMin = 10;
        private const int ThresholdMax = 600;
        private const int ThresholdDefault = 100;
        private int Threshold = ThresholdDefault;
        private Label _theshholdLabel;
        private static readonly TimeSpan RefreshRate = TimeSpan.FromMilliseconds(1);

        public MainPage()
        {
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);

            BackgroundColor = Color.Black;

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition()
                },
                RowSpacing = 0
            };

            _captionLabel = new Label()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Silver,
                Text = "Tap Start To Run",
                Padding = new Thickness(5),
                HorizontalTextAlignment = TextAlignment.Center
                
            };

            Grid.SetColumnSpan(_captionLabel, 2);
            grid.Children.Add(_captionLabel);


            _absoluteLayout = new AbsoluteLayout() { BackgroundColor = Color.Black, IsClippedToBounds = true };

            Grid.SetRow(_absoluteLayout, 1);
            Grid.SetColumnSpan(_absoluteLayout, 2);
            grid.Children.Add(_absoluteLayout);

            _startButton = new Button() { Text = "Start", Margin = new Thickness(10), HorizontalOptions = LayoutOptions.Start };
            _startButton.Clicked += StartButton_Clicked;
            Grid.SetRow(_startButton, 2);
            Grid.SetColumn(_startButton, 0);

            _stopButton = new Button() { Text = "Stop", Margin = new Thickness(10), HorizontalOptions = LayoutOptions.End };
            _stopButton.Clicked += StopButton_Clicked;
            Grid.SetRow(_stopButton, 2);
            Grid.SetColumn(_stopButton, 1);

            _theshholdLabel = new Label()
            {
                Text = Threshold.ToString(),
                TextColor = Color.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            Grid.SetRow(_theshholdLabel, 2);
            Grid.SetColumnSpan(_theshholdLabel, 2);

            grid.Children.Add(_startButton);
            grid.Children.Add(_stopButton);
            grid.Children.Add(_theshholdLabel);

            var slider = new Xamarin.Forms.Slider
            {
                Maximum = ThresholdMax,
                Minimum = ThresholdMin,
                Value = Threshold
            };
            slider.ValueChanged += Slider_ValueChanged;
            Grid.SetRow(slider, 3);
            Grid.SetColumnSpan(slider, 2);

            grid.Children.Add(slider);

            Content = grid;

            _random = new Random(1);
            _stopWatch = new Stopwatch();
        }

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Threshold = (int)e.NewValue;
            _theshholdLabel.Text = Threshold.ToString();
        }

        private void StopButton_Clicked(object sender, EventArgs e)
        {
            _isTestRunning = false;
            _absoluteLayout.Children.Clear();
            _stopWatch.Stop();
            OutputCalculatedRate(true);
        }

        private void OutputCalculatedRate(bool setCaption = true)
        {
            var elapsed = _stopWatch.ElapsedMilliseconds - _snapShot;
            var takeTime = elapsed > 1000;
            if (takeTime)
            {
                _snapShot = _stopWatch.ElapsedMilliseconds;
                Debug.WriteLine("Elapsed Time: " + elapsed);

                var elapsedSeconds = _stopWatch.Elapsed.TotalSeconds;
                var averageLabelsPerSecond = _totalElements / elapsedSeconds;
                var labelText = $"Avg Labels/s: {averageLabelsPerSecond}";
                Debug.WriteLine(labelText);
                if (setCaption)
                {
                    _captionLabel.Text = labelText;
                }
            }
        }

        private async void StartButton_Clicked(object sender, EventArgs e)
        {
            if (_isTestRunning)
            {
                return;
            }

            InitializeTest();

            while (_isTestRunning)
            {
                var totalChildren = _absoluteLayout.Children.Count;

                if (totalChildren > Threshold)
                {
                    _replaceIndex++;
                    if (_replaceIndex > Threshold)
                    {
                        _replaceIndex = 0;
                    }
                    _captionLabel.BackgroundColor = Color.Orange;
                }
                else
                {
                    _replaceIndex = -1;
                }

                var label = new Label
                {
                    Text = $"Label {_totalElements + 1}",
                    TextColor = new Color(_random.NextDouble(), _random.NextDouble(), _random.NextDouble()),
                    Rotation = _random.NextDouble() * 360
                };

                AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.PositionProportional);
                AbsoluteLayout.SetLayoutBounds(label, new Rectangle(_random.NextDouble(), _random.NextDouble(), 80, 24));

                if (_replaceIndex > -1)
                {
                    var oldLabel = (Label)_absoluteLayout.Children[_replaceIndex];
                    oldLabel.Parent = null;
                    _absoluteLayout.Children[_replaceIndex] = label;
                    oldLabel = null;
                    GC.Collect();
                }
                else
                {
                    _absoluteLayout.Children.Add(label);
                }

                _totalElements++;

                OutputCalculatedRate();


                await Task.Delay(RefreshRate);

            }
        }

        private void InitializeTest()
        {
            _stopWatch.Stop();
            _stopWatch.Reset();
            _stopWatch.Start();
            _isTestRunning = true;
            _totalElements = 0;
            _replaceIndex = -1;
            _captionLabel.Text = "Running...";
            _captionLabel.BackgroundColor = Color.Silver;
            _absoluteLayout.Children.Clear();
        }
    }
}

