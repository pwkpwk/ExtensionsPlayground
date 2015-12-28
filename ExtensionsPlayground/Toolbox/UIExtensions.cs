namespace ExtensionsPlayground.Toolbox
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics.Contracts;
    using System.Windows;
    using System.Windows.Markup;

    [ContentProperty("RegisteredExtensions")]
    sealed class UIExtensions : FrameworkElement
    {
        private readonly ISet<UIExtension> _stash;
        private FrameworkElement _attachedElement;

        public static readonly DependencyProperty ExtensionsProperty = DependencyProperty.RegisterAttached("Extensions",
            typeof(UIExtensions),
            typeof(UIExtensions),
            new PropertyMetadata(null, OnExtensionsPropertyChanged));

        public static readonly DependencyProperty RegisteredExtensionsProperty = DependencyProperty.Register("RegisteredExtensions",
            typeof(ObservableCollection<UIExtension>),
            typeof(UIExtensions),
            new PropertyMetadata(null, (sender, e) => ((UIExtensions)sender).OnRegisteredExtensionsPropertyChanged(e)));

        public UIExtensions()
        {
            _stash = new HashSet<UIExtension>();
            this.DataContextChanged += this.OnDataContextChanged;
            this.RegisteredExtensions = new ObservableCollection<UIExtension>();
        }

        public static UIExtensions GetExtensions(DependencyObject obj)
        {
            return (UIExtensions)obj.GetValue(ExtensionsProperty);
        }

        public static void SetExtensions(DependencyObject obj, UIExtensions value)
        {
            obj.SetValue(ExtensionsProperty, value);
        }

        public ObservableCollection<UIExtension> RegisteredExtensions
        {
            get { return (ObservableCollection<UIExtension>)GetValue(RegisteredExtensionsProperty); }
            set { SetValue(RegisteredExtensionsProperty, value); }
        }

        private static void OnExtensionsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement parent = sender as FrameworkElement;

            if(null != parent)
            {
                if(null != e.OldValue)
                {
                    UIExtensions exts = (UIExtensions)e.OldValue;
                    exts.DetachedFrom(parent);
                }

                if(null != e.NewValue)
                {
                    UIExtensions exts = (UIExtensions)e.NewValue;
                    exts.AttachedTo(parent);
                }
            }
        }

        private void OnRegisteredExtensionsPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if(null != e.OldValue)
            {
                ObservableCollection<UIExtension> oldCollection = (ObservableCollection<UIExtension>)e.OldValue;
                oldCollection.CollectionChanged -= this.OnCollectionContentsChanged;

                foreach (UIExtension extension in oldCollection)
                {
                    if (extension.IsTargetCompatible(_attachedElement))
                        extension.OnDetachedFrom(_attachedElement);
                }
                _stash.Clear();
            }

            if (null != e.NewValue)
            {
                ObservableCollection<UIExtension> newCollection = (ObservableCollection<UIExtension>)e.NewValue;
                newCollection.CollectionChanged += this.OnCollectionContentsChanged;

                foreach (UIExtension extension in newCollection)
                {
                    if (extension.IsTargetCompatible(_attachedElement))
                        extension.OnAttachedTo(_attachedElement);
                    _stash.Add(extension);
                }
            }
        }

        private void AttachedTo(FrameworkElement element)
        {
            Contract.Assert(null == _attachedElement);
            Contract.Assert(null != element);
            Contract.Assert(0 == _stash.Count);

            _attachedElement = element;
            _attachedElement.DataContextChanged += OnParentDataContextChanged;
            this.DataContext = element.DataContext;

            ObservableCollection<UIExtension> extensions = this.RegisteredExtensions;

            if(null != extensions)
            {
                foreach(UIExtension extension in extensions)
                {
                    if(extension.IsTargetCompatible(_attachedElement))
                    {
                        extension.OnAttachedTo(_attachedElement);
                    }
                    _stash.Add(extension);
                }
            }
        }

        private void DetachedFrom(FrameworkElement element)
        {
            Contract.Assert(object.ReferenceEquals(element, _attachedElement));
            ObservableCollection<UIExtension> extensions = this.RegisteredExtensions;

            _attachedElement.DataContextChanged -= OnParentDataContextChanged;

            if (null != extensions)
            {
                foreach (UIExtension extension in extensions)
                {
                    if (extension.IsTargetCompatible(_attachedElement))
                    {
                        extension.OnDetachedFrom(_attachedElement);
                    }
                }
            }

            _attachedElement = null;
            _stash.Clear();
        }

        private void OnParentDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.DataContext = e.NewValue;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(null != _attachedElement)
            {
                ObservableCollection<UIExtension> extensions = this.RegisteredExtensions;

                if (null != extensions)
                {
                    foreach (UIExtension extension in extensions)
                    {
                        extension.DataContext = e.NewValue;
                    }
                }
            }
        }

        private void OnCollectionContentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (UIExtension extension in e.NewItems)
                    {
                        extension.DataContext = this.DataContext;
                        if (extension.IsTargetCompatible(_attachedElement))
                        {
                            extension.OnAttachedTo(_attachedElement);
                        }
                        _stash.Add(extension);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (UIExtension extension in e.OldItems)
                    {
                        if (extension.IsTargetCompatible(_attachedElement))
                        {
                            extension.OnDetachedFrom(_attachedElement);
                        }
                        _stash.Remove(extension);
                        extension.DataContext = null;
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    foreach (UIExtension extension in _stash)
                    {
                        if (extension.IsTargetCompatible(_attachedElement))
                        {
                            extension.OnDetachedFrom(_attachedElement);
                        }
                        extension.DataContext = null;
                    }
                    _stash.Clear();

                    if(null != e.NewItems)
                    {
                        foreach (UIExtension extension in e.NewItems)
                        {
                            extension.DataContext = this.DataContext;
                            if (extension.IsTargetCompatible(_attachedElement))
                            {
                                extension.OnAttachedTo(_attachedElement);
                            }
                            _stash.Add(extension);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    //
                    // Do nothing
                    //
                    break;
            }
        }
    }
}
