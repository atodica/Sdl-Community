﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sdl.Community.DsiViewer
{
	public class ModelBase:INotifyPropertyChanged
	{

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
