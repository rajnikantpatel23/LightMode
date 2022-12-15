using System.Windows.Input;
using Xamarin.Forms;

namespace ProdKart.Behaviours
{
    public class ItemTappedBehavior : BehaviorBase<ListView>
    {        
        public ICommand Command { get; set; }
         
        protected override void OnAttachedTo(ListView bindable)
        {
            base.OnAttachedTo(bindable);
            AssociatedObject.ItemTapped += OnItemTapped;
        }

        protected override void OnDetachingFrom(ListView bindable)
        {
            base.OnDetachingFrom(bindable);
            AssociatedObject.ItemTapped -= OnItemTapped;
        }

        void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (Command == null || e.Item == null) return;

            if (Command.CanExecute(e.Item))
                Command.Execute(e.Item);
        }
    }
}
