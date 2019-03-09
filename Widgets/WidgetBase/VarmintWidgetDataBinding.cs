using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MonoVarmint.Widgets
{
    public partial class VarmintWidget
    {
        private List<Action> _bindingReadActions = new List<Action>();
        private Dictionary<string, Action> _bindingPostActions = new Dictionary<string, Action>();

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// If your custom control changes a property that could be bound and you want that 
        /// change to go back to the bound object, you will need to call this from 
        /// the Property setter.
        /// </summary>
        //--------------------------------------------------------------------------------------
        protected void PushValueToBinding(string propertyName)
        {
            if (!_bindingPostActions.ContainsKey(propertyName)) return;
            _bindingPostActions[propertyName]();
        }

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

            BindingContext = newContext;

            _bindingReadActions = new List<Action>();
            _bindingPostActions = new Dictionary<string, Action>();
            foreach (var bindingPair in _bindingTemplates)
            {
                if (bindingPair.Key == BindingContextPropertyName) continue;
                var bindingName = bindingPair.Value.Trim('{', '}', ' ');



                var targetEventInfo = GetType().GetEvent(bindingPair.Key, _publicInstance);
                if (targetEventInfo != null) // Is the type of this binding an event?
                {
                    // Events are handled differently from value bindings.  
                    // Try to get the method from the binding context, then from the bound class (if any)
                    MethodInfo sourceMethodInfo = null;
                    object sourceMethodTarget = null;
                    if (BindingContext != null)
                    {
                        sourceMethodInfo = BindingContext.GetType().GetMethod(bindingName, _publicInstance);
                        sourceMethodTarget = BindingContext;
                    }
                    if(sourceMethodInfo == null)
                    {
                        sourceMethodInfo = ControlHandler.GetType().GetMethod(bindingName, _publicInstance);
                        sourceMethodTarget = ControlHandler;
                    }
                    if (sourceMethodInfo == null)
                    {
                        throw new ApplicationException($"Could not find method '{bindingName}' on types {BindingContext?.GetType()} or {ControlHandler?.GetType()}");
                    }

                    var handler = Delegate.CreateDelegate(targetEventInfo.EventHandlerType, sourceMethodTarget, sourceMethodInfo);
                    targetEventInfo.AddEventHandler(this, handler);
                }
                else // This is a value binding
                {
                    if (BindingContext != null)
                    {
                        var sourcePropertyInfo = BindingContext.GetType().GetProperty(bindingName, _publicInstance);
                        var targetPropertyName = bindingPair.Key;
                        if (sourcePropertyInfo == null) throw new ApplicationException("Could not find property '"
                             + bindingName + "' on " + BindingContext.GetType());
                        var targetPropertyInfo = GetType().GetProperty(targetPropertyName);
                        if (targetPropertyInfo == null) throw new ApplicationException("Could not find property '"
                            + targetPropertyName + "' on " + GetType());
                        _bindingReadActions.Add(() =>
                        {
                            var newValue = sourcePropertyInfo.GetValue(BindingContext);
                            if (targetPropertyInfo.Name == "Size")
                            {
                                var size = (Vector2)newValue;
                                SpecifiedSize = new Tuple<float?, float?>(size.X, size.Y);
                            }

                            targetPropertyInfo.SetValue(this, newValue);
                        });

                        _bindingPostActions.Add(targetPropertyName, () =>
                        {
                            var newValue = targetPropertyInfo.GetValue(this);
                            sourcePropertyInfo.SetValue(BindingContext, newValue);
                        });
                    }
                    else
                    {
                        throw new ApplicationException($"BindingContext is NULL on {this.GetType().Name} named {Name}");
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
                foreach (var child in Children)
                {
                    child.ReadBindings(true);
                }
            }

            foreach (var bindingAction in _bindingReadActions)
            {
                bindingAction();
            }
        }

    }
}