﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VendaFlex.ViewModels.Commands
{
    public class AsyncCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private readonly Action<bool>? _onStateChanged;
        private bool _isExecuting;

        public AsyncCommand(Func<Task> execute, Func<bool>? canExecute = null, Action<bool>? onStateChanged = null)
        {
            _execute = execute;
            _canExecute = canExecute;
            _onStateChanged = onStateChanged;
        }

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;
            _isExecuting = true;
            _onStateChanged?.Invoke(true);
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            try { await _execute(); }
            finally { _isExecuting = false; _onStateChanged?.Invoke(false); CanExecuteChanged?.Invoke(this, EventArgs.Empty); }
        }

        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
