using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace MonoVarmint.Widgets
{
    public partial class VarmintWidget
    {
        List<Action> _bindingActions = new List<Action>();

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateBindings - Recalculate binding actions based on the new binding context.
        ///                 Binding actions are run every time an object is rendered
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void UpdateBindings(object newContext)
        {
            if (_bindingTemplates.ContainsKey(BindingContextPropertyName))
            {
                // pull the new binding context off of the incoming context
                var contextProperty = _bindingTemplates[BindingContextPropertyName].Trim('{', '}', ' ');

                var propertyInfo = newContext.GetType().GetProperty(contextProperty);
                if (propertyInfo == null)
                {
                    throw new ApplicationException("Could not find property '" + contextProperty
                        + "' on incoming context object of type: " + newContext.GetType().Name);
                }

                newContext = propertyInfo.GetValue(newContext);
            }

            // Update the children first.  Stop when we find a child that has had it's 
            // binding context set explicitely to something else.

            foreach (var child in children)
            {
                if (child.BindingContext == null || child.BindingContext == this.BindingContext)
                {
                    child.BindingContext = newContext;
                }
            }

            _bindingContext = newContext;

            _bindingActions = new List<Action>();
            foreach (var bindingPair in _bindingTemplates)
            {
                if (bindingPair.Key == BindingContextPropertyName) continue;
                var bindingName = bindingPair.Value.Trim('{', '}', ' ');

                var targetEventInfo = GetType().GetEvent(bindingPair.Key, _publicInstance);
                if (targetEventInfo != null)
                {
                    var sourceMethodInfo = _bindingContext.GetType().GetMethod(bindingName, _publicInstance);
                    if (sourceMethodInfo == null)
                    {
                        throw new ApplicationException("Could not find method '" + bindingName
                            + "' on type " + _bindingContext.GetType());
                    }

                    Delegate handler =
                         Delegate.CreateDelegate(targetEventInfo.EventHandlerType,
                                                 _bindingContext,
                                                 sourceMethodInfo);
                    targetEventInfo.AddEventHandler(this, handler);

                }
                else
                {
                    if (_bindingContext != null)
                    {
                        var sourcePropertyInfo = _bindingContext.GetType().GetProperty(bindingName, _publicInstance);
                        if (sourcePropertyInfo == null) throw new ApplicationException("Could not find property '"
                             + bindingName + "' on " + _bindingContext.GetType());
                        var targetPropertyInfo = GetType().GetProperty(bindingPair.Key);
                        if (targetPropertyInfo == null) throw new ApplicationException("Could not find property '"
                            + bindingPair.Key + "' on " + GetType());
                        _bindingActions.Add(() =>
                        {
                            object newValue = sourcePropertyInfo.GetValue(_bindingContext);
                            targetPropertyInfo.SetValue(this, newValue);
                        });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("WARNING: Binding object is NULL");
                    }
                }

            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ReadBindings - Force the bindings to be read
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void ReadBindings(bool recurse = false)
        {
            if (recurse)
            {
                foreach (var child in children)
                {
                    child.ReadBindings(true);
                }
            }

            foreach (var bindingAction in _bindingActions)
            {
                bindingAction();
            }
        }

    }
}