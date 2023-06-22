using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PokerFrontend.ViewModel;

public class BaseViewModel
    : ObservableObject, INotifyPropertyChanged
        , INotifyDataErrorInfo
        , IDataErrorInfo
{
    private readonly Dictionary<string, IList<string>> _validationErrors = new();

    public string this[string propertyName]
    {
        get
        {
            if (string.IsNullOrEmpty(propertyName))
                return Error;

            if (_validationErrors.ContainsKey(propertyName))
                return string.Join(Environment.NewLine, _validationErrors[propertyName]);

            return string.Empty;
        }
    }

    public string Error => string.Join(Environment.NewLine, GetAllErrors());

    public bool HasErrors => _validationErrors.Any();

    public IEnumerable GetErrors(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _validationErrors.SelectMany(kvp => kvp.Value);

        return _validationErrors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<object>();
    }

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    private IEnumerable<string> GetAllErrors()
    {
        return _validationErrors.SelectMany(kvp => kvp.Value).Where(e => !string.IsNullOrEmpty(e));
    }

    public void AddValidationError(string propertyName, string errorMessage)
    {
        if (!_validationErrors.ContainsKey(propertyName))
            _validationErrors.Add(propertyName, new List<string>());

        _validationErrors[propertyName].Add(errorMessage);
    }

    public void ClearValidationErrors(string propertyName)
    {
        if (_validationErrors.ContainsKey(propertyName))
            _validationErrors.Remove(propertyName);
    }
}