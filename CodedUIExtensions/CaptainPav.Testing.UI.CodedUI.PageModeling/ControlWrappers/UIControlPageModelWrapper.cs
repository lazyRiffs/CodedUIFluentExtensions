﻿using System;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace CaptainPav.Testing.UI.CodedUI.PageModeling.ControlWrappers
{
    /// <summary>
    /// Wraps a control in a page model to provide consistent, but abstracted
    /// access to properties of the control as though it were a page model
    /// </summary>
    /// <typeparam name="T">
    /// Type of control wrapped as a page model
    /// </typeparam>
    public class UIControlPageModelWrapper<T> : PageModelBase<T> where T : UITestControl
    {
        protected readonly T _control;
        public UIControlPageModelWrapper(T control)
        {
            if (null == control)
            {
                throw new ArgumentNullException(nameof(control));
            }
            this._control = control;
        }

        internal protected override T Me => this._control;
    }

    /// <summary>
    /// Wraps a control in a page model to provide consistent, but abstracted
    /// access to properties of the control as though it were a page model
    /// </summary>
    /// <remarks>
    /// Convenience wrapper when the control type does not matter
    /// </remarks>
    public class UIControlPageModelWrapper : UIControlPageModelWrapper<UITestControl>
    {
        public UIControlPageModelWrapper(UITestControl control) : base(control) { }
    }
}