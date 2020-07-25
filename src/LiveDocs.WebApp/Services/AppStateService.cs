// SOURCE: http://www.engstromjimmy.se/blazor/2020/03/11/ChangeTitle.html
// AUTHOR: Jimmy Engström
// DATE: 2020-03-11

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LiveDocs.WebApp.Services
{
    public class AppStateService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string title = "LiveDocs";

        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }
    }
}
