using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace CoffeeJelly.tempa.Behaviors
{

    //http://stackoverflow.com/questions/36326949/how-to-properly-execute-a-command-upon-animation-is-completed
    public class CommandFakeAnimation : AnimationTimeline
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(CommandFakeAnimation), new UIPropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(CommandFakeAnimation), new PropertyMetadata(null));

        public CommandFakeAnimation()
        {
            Completed += new EventHandler(CommandAnimation_Completed);
        }

        public ICommand Command
        {
            get
            {
                return (ICommand)GetValue(CommandProperty);
            }
            set
            {
                SetValue(CommandProperty, value);
            }
        }

        //public object CommandParameter
        //{
        //    get
        //    {
        //        return GetValue(CommandParameterProperty);
        //    }
        //    set
        //    {
        //        SetValue(CommandParameterProperty, value);
        //    }
        //}

        private void CommandAnimation_Completed(object sender, EventArgs e)
        {
            if (Command != null && Command.CanExecute(true))
            {
                Command.Execute(true);
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new CommandFakeAnimation();
        }

        public override Type TargetPropertyType
        {
            get
            {
                return typeof(Object);
            }
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            return defaultOriginValue;
        }
    }
}
