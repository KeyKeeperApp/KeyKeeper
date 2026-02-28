using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;

namespace KeyKeeper.Views
{
    public partial class ToastNotificationHost : UserControl
    {
        private CancellationTokenSource? _hideCancellation;
        private Animation? _fadeOutAnimation;

        public static readonly StyledProperty<string> MessageProperty =
            AvaloniaProperty.Register<ToastNotificationHost, string>(nameof(Message), string.Empty);

        public static readonly StyledProperty<TimeSpan> DurationProperty =
            AvaloniaProperty.Register<ToastNotificationHost, TimeSpan>(nameof(Duration), TimeSpan.FromSeconds(5));

        public string Message
        {
            get => GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public TimeSpan Duration
        {
            get => GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        public ToastNotificationHost()
        {
            InitializeComponent();

            _fadeOutAnimation = new Animation
            {
                Duration = TimeSpan.FromSeconds(0.5),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(OpacityProperty, 1.0) },
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(OpacityProperty, 0.0) },
                    }
                }
            };
            Opacity = 0;
        }

        public void Show(string message)
        {
            _hideCancellation?.Cancel();
            _hideCancellation = new CancellationTokenSource();
            Message = message;
            Opacity = 1;

            _ = HideAfterDelay(_hideCancellation.Token);
        }

        private void FadeOut()
        {
            _fadeOutAnimation?.RunAsync(this);
        }

        private async Task HideAfterDelay(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(Duration, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    FadeOut();
                }
            }
            catch (TaskCanceledException)
            {
                // caught
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _hideCancellation?.Cancel();
            base.OnDetachedFromVisualTree(e);
        }
    }
}