namespace ExtensionsPlayground
{
    sealed class MainWindowViewModel
    {
        private bool _buttonHasFocus, _textHasFocus;

        public MainWindowViewModel()
        {
            _buttonHasFocus = false;
            _textHasFocus = true;
        }

        public bool ButtonHasFocus
        {
            get { return _buttonHasFocus; }
            set { _buttonHasFocus = value; }
        }

        public bool TextHasFocus
        {
            get { return _textHasFocus; }
            set { _textHasFocus = value; }
        }
    }
}
