using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.model
{
    public class BindableStringBuilderWrapper : INotifyPropertyChanged
    {

        private StringBuilder _stringBuilder = new StringBuilder();

        private EventHandler<EventArgs> TextChanged;

        public string BuilderText
        {
            get { return _stringBuilder.ToString(); }
        }



        public void AppendNewLine(string text) {
            _stringBuilder.AppendLine(text);
            if (TextChanged != null)
                TextChanged(this, null);
            OnPropertyChanged(() => BuilderText);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                return;
            }

            var handler = PropertyChanged;

            if (handler != null)
            {
                var body = propertyExpression.Body as MemberExpression;
                if (body != null)
                    handler(this, new PropertyChangedEventArgs(body.Member.Name));
            }
        }

    }
}
