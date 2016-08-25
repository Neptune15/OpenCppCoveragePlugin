﻿// OpenCppCoverage is an open source code coverage for C++.
// Copyright (C) 2016 OpenCppCoverage
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using GalaSoft.MvvmLight.Command;
using OpenCppCoverage.VSPackage.Helper;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace OpenCppCoverage.VSPackage.Settings.UI
{
    //-------------------------------------------------------------------------
    // String cannot be used directly inside a datagrid.
    class Pattern
    {
        public string Value { get; set; }
    }

    //-------------------------------------------------------------------------
    class UnifiedDiffs : PropertyChangedNotifier
    {
        string unifiedDiffPath;
        string optionalRootFolder;

        //---------------------------------------------------------------------
        public string UnifiedDiffPath
        {
            get { return this.unifiedDiffPath; }
            set { SetField(ref this.unifiedDiffPath, value); }
        }

        //---------------------------------------------------------------------
        public string OptionalRootFolder
        {
            get { return this.optionalRootFolder; }
            set { SetField(ref this.optionalRootFolder, value); }
        }
    }

    //-------------------------------------------------------------------------
    class FilterSettingController
    {
        readonly IFileSystemDialog fileSystemDialog;

        //---------------------------------------------------------------------
        public FilterSettingController(IFileSystemDialog fileSystemDialog)
        {
            this.fileSystemDialog = fileSystemDialog;
            this.SourcePatterns = new ObservableCollection<Pattern>();
            this.ExcludedSourcePatterns = new ObservableCollection<Pattern>();
            this.ModulePatterns = new ObservableCollection<Pattern>();
            this.ExcludedModulePatterns = new ObservableCollection<Pattern>();
            this.UnifiedDiffs = new ObservableCollection<UnifiedDiffs>();            
            this.UnifiedDiffCommand = new RelayCommand(OnUnifiedDiffCommand);
        }

        //-----------------------------------------------------------------
        static void CellClickHandle<T>(
            DataGridCellInfo cellInfo,             
            ObservableCollection<T> collection,
            Func<T, string, bool> action) where T: class, new()
        {            
            if (cellInfo.IsValid)
            {
                var item = cellInfo.Item as T;
                bool newItemCreated = false;

                if (item == null && cellInfo.Item == CollectionView.NewItemPlaceholder)
                {
                    item = new T();
                    newItemCreated = true;
                }

                if (item == null)
                    throw new InvalidOperationException("Error in CellClickHandle");

                var column = (DataGridBoundColumn)cellInfo.Column;
                var binding = (Binding)column.Binding;
                var propertyPath = binding.Path;

                if (action(item, propertyPath.Path) && newItemCreated)
                    collection.Add(item);
            }
        }

        //---------------------------------------------------------------------
        void OnUnifiedDiffCommand()
        {
            CellClickHandle(this.CurrentUnifiedDiffCellInfo, this.UnifiedDiffs,
                (item, bindingPath) =>
                {
                    switch (bindingPath)
                    {
                        case nameof(item.UnifiedDiffPath):
                            return fileSystemDialog.SelectFile(
                                "Diff Files (.diff)|*.diff|All Files (*.*)|*.*",
                                path => item.UnifiedDiffPath = path);
                        case nameof(item.OptionalRootFolder):
                            return fileSystemDialog.SelectFolder(path => item.OptionalRootFolder = path);
                        default:
                            throw new InvalidOperationException("Invalid Value for DisplayIndex");
                    };
                });
        }

        public DataGridCellInfo CurrentUnifiedDiffCellInfo { get; set; }
        public ICommand UnifiedDiffCommand { get; private set; }
        public ObservableCollection<Pattern> SourcePatterns { get; private set; }
        public ObservableCollection<Pattern> ExcludedSourcePatterns { get; private set; }
        public ObservableCollection<Pattern> ModulePatterns { get; private set; }
        public ObservableCollection<Pattern> ExcludedModulePatterns { get; private set; }
        public ObservableCollection<UnifiedDiffs> UnifiedDiffs { get; private set; }
    }
}