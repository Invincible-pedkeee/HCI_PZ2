namespace NetworkService.Helpers
{
    public abstract class ValidationBase : BindableBase
    {
        private bool isValid;

        public ValidationErrors ValidationErrors { get; private set; }

        public bool IsValid
        {
            get
            {
                return isValid;
            }
            private set
            {
                SetProperty(ref isValid, value);
            }
        }

        protected ValidationBase()
        {
            ValidationErrors = new ValidationErrors();
        }

        public void Validate()
        {
            ValidationErrors.Clear();
            ValidateSelf();
            IsValid = ValidationErrors.IsValid;

            OnPropertyChanged("ValidationErrors");
        }

        protected abstract void ValidateSelf();
    }
}