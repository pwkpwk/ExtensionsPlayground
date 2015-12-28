namespace ExtensionsPlayground.Toolbox
{
    using System.Diagnostics.Contracts;
    using System.Windows;

    abstract class UIExtension : FrameworkElement
    {
        internal abstract bool IsTargetCompatible(FrameworkElement target);

        internal abstract void OnAttachedTo(FrameworkElement element);
        internal abstract void OnDetachedFrom(FrameworkElement element);
    }

    abstract class UIExtension<TTargetType> : UIExtension where TTargetType : FrameworkElement
    {
        private TTargetType _attachedElement;

        protected TTargetType AttachedElement
        {
            get { return _attachedElement; }
        }

        internal sealed override bool IsTargetCompatible(FrameworkElement target)
        {
            return null != target && typeof(TTargetType).IsAssignableFrom(target.GetType());
        }

        public virtual void Attached(TTargetType attachedTo)
        {
        }

        public virtual void Detached(TTargetType detachedFrom)
        {
        }

        internal sealed override void OnAttachedTo(FrameworkElement element)
        {
            _attachedElement = (TTargetType)element;
            this.Attached(_attachedElement);
        }

        internal sealed override void OnDetachedFrom(FrameworkElement element)
        {
            Contract.Assert(object.ReferenceEquals(element, _attachedElement));
            this.Detached(_attachedElement);
            _attachedElement = null;
        }
    }
}
