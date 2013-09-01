﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

using NuGet;

namespace ICSharpCode.PackageManagement
{
	public class RegisteredPackageSourcesViewModel : ViewModelBase<RegisteredPackageSourcesViewModel>
	{
		ObservableCollection<PackageSourceViewModel> packageSourceViewModels = 
			new ObservableCollection<PackageSourceViewModel>();
		RegisteredPackageSources packageSources;
		IFolderBrowser folderBrowser;
		
		DelegateCommand addPackageSourceCommmand;
		DelegateCommand removePackageSourceCommand;
		DelegateCommand movePackageSourceUpCommand;
		DelegateCommand movePackageSourceDownCommand;
		DelegateCommand browsePackageFolderCommand;
		
		RegisteredPackageSource newPackageSource = new RegisteredPackageSource();
		PackageSourceViewModel selectedPackageSourceViewModel;
		
		public RegisteredPackageSourcesViewModel(
			RegisteredPackageSources packageSources)
			: this(packageSources, new FolderBrowser())
		{
		}
		
		public RegisteredPackageSourcesViewModel(
			RegisteredPackageSources packageSources,
			IFolderBrowser folderBrowser)
		{
			this.packageSources = packageSources;
			this.folderBrowser = folderBrowser;
			CreateCommands();
		}
		
		void CreateCommands()
		{
			addPackageSourceCommmand =
				new DelegateCommand(param => AddPackageSource(),
					param => CanAddPackageSource);
			
			removePackageSourceCommand =
				new DelegateCommand(param => RemovePackageSource(),
					param => CanRemovePackageSource);
			
			movePackageSourceUpCommand =
				new DelegateCommand(param => MovePackageSourceUp(),
					param => CanMovePackageSourceUp);
			
			movePackageSourceDownCommand =
				new DelegateCommand(param => MovePackageSourceDown(),
					param => CanMovePackageSourceDown);
			
			browsePackageFolderCommand =
				new DelegateCommand(param => BrowsePackageFolder());
		}
		
		public ICommand AddPackageSourceCommand {
			get { return addPackageSourceCommmand; }
		}
		
		public ICommand RemovePackageSourceCommand {
			get { return removePackageSourceCommand; }
		}
		
		public ICommand MovePackageSourceUpCommand {
			get { return movePackageSourceUpCommand; }
		}
		
		public ICommand MovePackageSourceDownCommand {
			get { return movePackageSourceDownCommand; }
		}
		
		public ICommand BrowsePackageFolderCommand {
			get { return browsePackageFolderCommand; }
		}
		
		public ObservableCollection<PackageSourceViewModel> PackageSourceViewModels {
			get { return packageSourceViewModels; }
		}
		
		public void Load()
		{
			foreach (PackageSource packageSource in packageSources) {
				AddPackageSourceToViewModel(packageSource);
			}
		}
		
		void AddPackageSourceToViewModel(PackageSource packageSource)
		{
			var packageSourceViewModel = new PackageSourceViewModel(packageSource);
			packageSourceViewModels.Add(packageSourceViewModel);
		}
		
		public void Save()
		{
			packageSources.Clear();
			foreach (PackageSourceViewModel packageSourceViewModel in packageSourceViewModels) {
				PackageSource source = packageSourceViewModel.GetPackageSource();
				packageSources.Add(source);
			}
		}
		
		public string NewPackageSourceName {
			get { return newPackageSource.Name; }
			set {
				newPackageSource.Name = value;
				OnPropertyChanged(viewModel => viewModel.NewPackageSourceName);
			}
		}
		
		public string NewPackageSourceUrl {
			get { return newPackageSource.Source; }
			set {
				newPackageSource.Source = value;
				OnPropertyChanged(viewModel => viewModel.NewPackageSourceUrl);
			}
		}
		
		public PackageSourceViewModel SelectedPackageSourceViewModel {
			get { return selectedPackageSourceViewModel; }
			set {
				selectedPackageSourceViewModel = value;
				OnPropertyChanged(viewModel => viewModel.SelectedPackageSourceViewModel);
				OnPropertyChanged(viewModel => viewModel.CanAddPackageSource);
			}
		}
		
		public void AddPackageSource()
		{
			AddNewPackageSourceToViewModel();
			SelectLastPackageSourceViewModel();
		}
		
		void AddNewPackageSourceToViewModel()
		{
			var packageSource = newPackageSource.ToPackageSource();
			AddPackageSourceToViewModel(packageSource);
		}
		
		void SelectLastPackageSourceViewModel()
		{
			SelectedPackageSourceViewModel = GetLastPackageSourceViewModel();
		}
		
		public bool CanAddPackageSource {
			get { return NewPackageSourceHasUrl && NewPackageSourceHasName; }
		}
		
		bool NewPackageSourceHasUrl {
			get { return !String.IsNullOrEmpty(NewPackageSourceUrl); }
		}
		
		bool NewPackageSourceHasName {
			get { return !String.IsNullOrEmpty(NewPackageSourceName); }
		}
		
		public void RemovePackageSource()
		{
			RemoveSelectedPackageSourceViewModel();
		}
		
		public bool CanRemovePackageSource {
			get { return IsPackageSourceSelected(); }
		}
		
		void RemoveSelectedPackageSourceViewModel()
		{
			packageSourceViewModels.Remove(selectedPackageSourceViewModel);
		}
		
		public void MovePackageSourceUp()
		{
			int selectedPackageSourceIndex = GetSelectedPackageSourceViewModelIndex();
			int destinationPackageSourceIndex = selectedPackageSourceIndex--;
			packageSourceViewModels.Move(selectedPackageSourceIndex, destinationPackageSourceIndex);
		}
		
		int GetSelectedPackageSourceViewModelIndex()
		{
			return packageSourceViewModels.IndexOf(selectedPackageSourceViewModel);
		}
		
		public bool CanMovePackageSourceUp {
			get {
				return HasAtLeastTwoPackageSources() &&
					IsPackageSourceSelected() &&
					!IsFirstPackageSourceSelected();
			}
		}
		
		bool IsPackageSourceSelected()
		{
			return selectedPackageSourceViewModel != null;
		}
		
		bool IsFirstPackageSourceSelected()
		{
			return selectedPackageSourceViewModel == packageSourceViewModels[0];
		}
		
		public void MovePackageSourceDown()
		{
			int selectedPackageSourceIndex = GetSelectedPackageSourceViewModelIndex();
			int destinationPackageSourceIndex = selectedPackageSourceIndex++;
			packageSourceViewModels.Move(selectedPackageSourceIndex, destinationPackageSourceIndex);			
		}
		
		public bool CanMovePackageSourceDown {
			get {
				return HasAtLeastTwoPackageSources() &&
					IsPackageSourceSelected() &&
					!IsLastPackageSourceSelected();
			}
		}
		
		bool HasAtLeastTwoPackageSources()
		{
			return packageSourceViewModels.Count >= 2;
		}
		
		bool IsLastPackageSourceSelected()
		{
			PackageSourceViewModel lastViewModel = GetLastPackageSourceViewModel();
			return lastViewModel == selectedPackageSourceViewModel;
		}
		
		PackageSourceViewModel GetLastPackageSourceViewModel()
		{
			return packageSourceViewModels.Last();
		}
		
		public void BrowsePackageFolder()
		{
			string folder = folderBrowser.SelectFolder();
			if (folder != null) {
				UpdateNewPackageSourceUsingSelectedFolder(folder);
			}
		}
		
		void UpdateNewPackageSourceUsingSelectedFolder(string folder)
		{
			NewPackageSourceUrl = folder;
			NewPackageSourceName = GetPackageSourceNameFromFolder(folder);
		}
		
		string GetPackageSourceNameFromFolder(string folder)
		{
			return Path.GetFileName(folder);
		}
	}
}
