using System.ComponentModel;

namespace Chapter9.Interface
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Used to raise property changed event. Typically notifies UI elements
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed</param>
        protected void RaisePropertyChangedEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}