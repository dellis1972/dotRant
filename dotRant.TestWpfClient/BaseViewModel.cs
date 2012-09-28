using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace dotRant.TestWpfClient
{
    class BaseViewModel
    {
        internal protected Dispatcher _dispatcher;

        public BaseViewModel(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            string name = ((MemberExpression)property.Body).Member.Name;
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        protected void DispatchPropertyChanged<T>(Expression<Func<T>> property)
        {
            _dispatcher.BeginInvoke((Action)delegate
            {
                OnPropertyChanged(property);
            });
        }
        #endregion
    }
}
