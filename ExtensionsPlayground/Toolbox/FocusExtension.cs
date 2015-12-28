namespace ExtensionsPlayground.Toolbox
{
    using System.Windows;

    sealed class FocusExtension : UIExtension<FrameworkElement>
    {
        private bool _updatingSelf;

        public static readonly DependencyProperty HasFocusProperty = DependencyProperty.Register("HasFocus",
            typeof(bool),
            typeof(FocusExtension),
            new PropertyMetadata(false, (sender, e) => ((FocusExtension)sender).OnHasFocusPropertyChanged(e)));

        public bool HasFocus
        {
            get { return (bool)GetValue(HasFocusProperty); }
            set { SetValue(HasFocusProperty, value); }
        }

        public override void Attached(FrameworkElement attachedTo)
        {
            attachedTo.GotFocus += this.OnGotFocus;
            attachedTo.LostFocus += this.OnLostFocus;
        }

        public override void Detached(FrameworkElement attachedTo)
        {
            attachedTo.GotFocus -= this.OnGotFocus;
            attachedTo.LostFocus -= this.OnLostFocus;
        }

        private void OnHasFocusPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if(!_updatingSelf && (bool)e.NewValue)
            {
                if (null != this.AttachedElement)
                {
                    this.AttachedElement.Focus();
                }
            }
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if(object.ReferenceEquals(this.AttachedElement, e.OriginalSource))
            {
                _updatingSelf = true;
                this.HasFocus = true;
                _updatingSelf = false;
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (object.ReferenceEquals(this.AttachedElement, e.OriginalSource))
            {
                _updatingSelf = true;
                this.HasFocus = false;
                _updatingSelf = false;
            }
        }
    }
}
