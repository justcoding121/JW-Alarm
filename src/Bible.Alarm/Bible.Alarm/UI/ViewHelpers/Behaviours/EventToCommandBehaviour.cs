﻿using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bible.Alarm.UI.Views.Behaviours
{
    /// <summary>
    /// This behavior allow you to bind any event on any View to an <see cref="ICommand"/>.
    /// Typically, this element is used in XAML to connect the attached element
    /// to a command located in a ViewModel. This trigger can only be attached
    /// to a View or a class deriving from View.
    /// https://github.com/asimmon/Pillar/blob/master/src/Pillar/Behaviors/EventToCommandBehavior.cs
    /// </summary>
    public class EventToCommandBehavior : BindableBehavior<View>
    {
        public static readonly BindableProperty EventNameProperty = BindableProperty.Create("EventName", typeof(string), typeof(EventToCommandBehavior));
        public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(EventToCommandBehavior));
        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(EventToCommandBehavior));
        public static readonly BindableProperty EventArgsConverterProperty = BindableProperty.Create("EventArgsConverter", typeof(IValueConverter), typeof(EventToCommandBehavior));
        public static readonly BindableProperty EventArgsConverterParameterProperty = BindableProperty.Create("EventArgsConverterParameter", typeof(object), typeof(EventToCommandBehavior));

        private Delegate handler;
        private EventInfo eventInfo;

        /// <summary>
        /// The name of the View event to bind.
        /// </summary>
        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }

        /// <summary>
        /// The command to execute.
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// The command parameter.
        /// </summary>
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        /// Gets or sets a converter used to convert the EventArgs.
        /// By default, the EventArgs are passed to the command if CommandParameter
        /// if not null or EventArgs.Empty.
        /// </summary>
        public IValueConverter EventArgsConverter
        {
            get { return (IValueConverter)GetValue(EventArgsConverterProperty); }
            set { SetValue(EventArgsConverterProperty, value); }
        }

        /// <summary>
        /// Gets or sets a parameters for the converter used to convert the EventArgs.
        /// </summary>
        public object EventArgsConverterParameter
        {
            get { return GetValue(EventArgsConverterParameterProperty); }
            set { SetValue(EventArgsConverterParameterProperty, value); }
        }

        protected override void OnAttachedTo(View visualElement)
        {
            base.OnAttachedTo(visualElement);

            if (string.IsNullOrWhiteSpace(EventName))
                throw new ArgumentException("EventToCommand: EventName must be specified");

            var events = AssociatedObject.GetType().GetRuntimeEvents().ToArray();
            if (events.Any())
            {
                eventInfo = events.FirstOrDefault(e => e.Name == EventName);
                if (eventInfo == null)
                    throw new ArgumentException($"EventToCommand: Cannot find any event named '{EventName}' on attached type");

                AddEventHandler(eventInfo, AssociatedObject, OnFired);
            }
        }

        protected override void OnDetachingFrom(View view)
        {
            if (handler != null)
                eventInfo.RemoveEventHandler(AssociatedObject, handler);

            base.OnDetachingFrom(view);
        }

        private void AddEventHandler(EventInfo eventInfo, object item, Action<object, EventArgs> action)
        {
            var eventParameters = eventInfo.EventHandlerType
                .GetRuntimeMethods().First(m => m.Name == "Invoke")
                .GetParameters()
                .Select(p => Expression.Parameter(p.ParameterType))
                .ToArray();

            var actionInvoke = action.GetType()
                .GetRuntimeMethods().First(m => m.Name == "Invoke");

            handler = Expression.Lambda(
                eventInfo.EventHandlerType,
                Expression.Call(Expression.Constant(action), actionInvoke, eventParameters[0], eventParameters[1]),
                eventParameters
            )
            .Compile();

            eventInfo.AddEventHandler(item, handler);
        }

        public void OnFired(object sender, EventArgs eventArgs)
        {
            if (Command == null)
                return;

            var parameter = CommandParameter;

            if (parameter == null && eventArgs != null && eventArgs != EventArgs.Empty)
            {
                parameter = eventArgs;

                if (EventArgsConverter != null)
                {
                    parameter = EventArgsConverter.Convert(eventArgs, typeof(object), EventArgsConverterParameter, CultureInfo.CurrentUICulture);
                }
            }

            if (Command.CanExecute(parameter))
            {
                Command.Execute(parameter);
            }
        }
    }
}
