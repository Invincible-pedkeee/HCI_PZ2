using System.Windows.Threading;
using NetworkService.Helpers;

namespace NetworkService.Services
{
    public class ToastNotificationService : BindableBase
    {
        private readonly DispatcherTimer hideTimer;

        private bool isVisible;
        private string title;
        private string message;
        private string backgroundColor;
        private string borderColor;

        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
            set
            {
                SetProperty(ref isVisible, value);
            }
        }

        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                SetProperty(ref title, value);
            }
        }

        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                SetProperty(ref message, value);
            }
        }

        public string BackgroundColor
        {
            get
            {
                return backgroundColor;
            }
            set
            {
                SetProperty(ref backgroundColor, value);
            }
        }

        public string BorderColor
        {
            get
            {
                return borderColor;
            }
            set
            {
                SetProperty(ref borderColor, value);
            }
        }

        public ToastNotificationService()
        {
            hideTimer = new DispatcherTimer();
            hideTimer.Interval = System.TimeSpan.FromSeconds(3);
            hideTimer.Tick += HideTimer_Tick;

            BackgroundColor = "#E8EDF3";
            BorderColor = "#A8A8A8";
        }

        public void Show(string title, string message, ToastNotificationType type)
        {
            Title = title;
            Message = message;

            ApplyColors(type);

            IsVisible = true;

            hideTimer.Stop();
            hideTimer.Start();
        }

        public void ShowSuccess(string message)
        {
            Show("Uspješna operacija", message, ToastNotificationType.Success);
        }

        public void ShowInfo(string message)
        {
            Show("Informacija", message, ToastNotificationType.Info);
        }

        public void ShowWarning(string message)
        {
            Show("Upozorenje", message, ToastNotificationType.Warning);
        }

        public void ShowError(string message)
        {
            Show("Greška", message, ToastNotificationType.Error);
        }

        public void Hide()
        {
            hideTimer.Stop();
            IsVisible = false;
        }

        private void HideTimer_Tick(object sender, System.EventArgs e)
        {
            Hide();
        }

        private void ApplyColors(ToastNotificationType type)
        {
            if (type == ToastNotificationType.Success)
            {
                BackgroundColor = "#E8F5E9";
                BorderColor = "#2E7D32";
            }
            else if (type == ToastNotificationType.Warning)
            {
                BackgroundColor = "#FFF8E1";
                BorderColor = "#F9A825";
            }
            else if (type == ToastNotificationType.Error)
            {
                BackgroundColor = "#FFEBEE";
                BorderColor = "#C62828";
            }
            else
            {
                BackgroundColor = "#E8EDF3";
                BorderColor = "#607D8B";
            }
        }
    }
}