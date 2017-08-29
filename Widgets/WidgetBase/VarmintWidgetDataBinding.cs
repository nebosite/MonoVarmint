using System;
using System.Collections.Generic;

namespace MonoVarmint.Widgets
{
    public partial class VarmintWidget
    {
        private List<Action> _bindingReadActions = new List<Action>();
        private readonly Dictionary<string, Action> _bindingPostActions = new Dictionary<string, Action>();

        protected void PostBackProperty(string propertyName)
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
            foreach (var bindingPair in _bindingTemplates)
            {
                if (bindingPair.Key == BindingContextPropertyName) continue;
                var bindingName = bindingPair.Value.Trim('{', '}', ' ');

                // Events are handled differently from value bindings
                var targetEventInfo = GetType().GetEvent(bindingPair.Key, _publicInstance);
                if (targetEventInfo != null)
                {
                    var sourceMethodInfo = EventBindingContext.GetType().GetMethod(bindingName, _publicInstance);
                    if (sourceMethodInfo == null)
                    {
                        throw new ApplicationException("Could not find method '" + bindingName
                            + "' on type " + EventBindingContext.GetType());
                    }

                    var handler =
                         Delegate.CreateDelegate(targetEventInfo.EventHandlerType,
                                                 EventBindingContext,
                                                 sourceMethodInfo);
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