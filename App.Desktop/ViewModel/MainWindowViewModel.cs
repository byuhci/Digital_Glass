using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using DigitalGlass.Annotations;
using DigitalGlass.Eagle;
using DigitalGlass.Model;
using DigitalGlass.Commands;
using System.ComponentModel;
using System.Windows.Data;
using System.IO;

namespace DigitalGlass.ViewModel
{
    /// <summary>
    /// Controls the main window of the application.
    /// <seealso cref="MainWindow.xaml"/>
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IReadOnlyCollection<CommandViewModel> _commands;
        private IReadOnlyCollection<CommandViewModel> _toolbarCommands;
        private Collection<Frame> _frames;
       // private IReadOnlyCollection<Frame> _frames;

        private string _coordinates;
        private CanvasHostViewModel _canvasHost;


        public MainWindowViewModel()
        {
            _commands = new ReadOnlyCollection<CommandViewModel>(CreateCommands());
            _toolbarCommands = new ReadOnlyCollection<CommandViewModel>(CreateToolbarCommands());
            CreateAddFrameCommand();
            _frames = new Collection<Frame>();

        }


        public event Action RequestClose;

        /// <summary>
        /// Controls the center of the application. The drawing layer.
        /// </summary>
        public CanvasHostViewModel CanvasHost
        {
            get { return _canvasHost; }
            set
            {
                if (Equals(value, _canvasHost)) return;
                _canvasHost = value;
                OnPropertyChanged();
            }
        }


        protected virtual void OnRequestClose()
        {
            var handler = RequestClose;
            if (handler != null) handler();
        }

        /// <summary>
        /// Command on the left sidebar of the application
        /// </summary>
        [UsedImplicitly]
        public IReadOnlyCollection<CommandViewModel> Commands
        {
            get { return _commands; }
        }

        /// <summary>
        /// 
        /// </summary>
        //[UsedImplicitly]
        public Collection<Frame> Frames { get { return _frames; } }
        // public ObservableCollection<Frame> Frames = new ObservableCollection<Frame>() { new Frame()};


        [UsedImplicitly]
        public ICommand AddFrameCommand
        {
            get;
            internal set;
        }

        private bool CanExecuteAddFrameCommand(object obj)
        {
            return _canvasHost != null;
        }

        private void CreateAddFrameCommand()
        {
            AddFrameCommand = new RelayCommand(AddFrameCommandExecute, CanExecuteAddFrameCommand);
        }

        public void AddFrameCommandExecute(object obj)
        {
            //Code To Create new Frame
            Frame f = _canvasHost.animation.addCopyOfFrame(_canvasHost.currentFrame);
            _frames.Add(f);
            //Move to Frame
            _canvasHost.moveToFrame(_canvasHost.animation.numFrames() - 1);
            //Update View
            ICollectionView view = CollectionViewSource.GetDefaultView(Frames);
            view.Refresh();
            view.MoveCurrentToFirst();

        }


        /// <summary>
        /// Commands in the "File" menu
        /// </summary>
        [UsedImplicitly]
        public IReadOnlyCollection<CommandViewModel> ToolbarCommands
        {
            get { return _toolbarCommands; }
        }

        private IList<CommandViewModel> CreateCommands()
        {
            return new List<CommandViewModel>()
            {
                new CommandViewModel("Open...", "Resources/folder_Open_32xLG.png",
                    new RelayCommand(param => this.OpenFile())),
                new CommandViewModel("Export to Gerber...", null,
                    new RelayCommand(param => this.ExportGerber(), param => this.ImageLoaded)),
                new CommandViewModel("Export Animation File...", null,
                    new RelayCommand(param => this.ExportAnimatiion(), param => this.ImageLoaded)),
                new CommandViewModel("Export Only to Eagle BRD...", null,                 //TODO: This is for Testing and Eventually will be removed from menu options
                    new RelayCommand(param => this.ExportEagle(), param => this.ImageLoaded)),
                new CommandViewModel("Close...", new RelayCommand(param => this.CloseFile(), param => this.CanClose)),
                new CommandViewModel("Exit...", "Resources/Close_16xLG.png", new RelayCommand(param => this.CloseApp())),
            };
        }

        /// <summary>
        /// Creates an in memory representation of an LED PCB using the data from the canvashost model.
        /// </summary>
        /// <returns></returns>
        private LedBoardBuilder CreateBoardFromModel()
        {
            var ledBoard = new LedBoardBuilder(this.CanvasHost.BoardWidth, this.CanvasHost.BoardHeight);
            Animation a = Animation.getInstance();
            foreach (var led in a.getLeds())
            {
                ledBoard.AddLedAtPoint(led.X * _canvasHost.imageToOutputScale, (this._canvasHost.ImageHeight - led.Y) * _canvasHost.imageToOutputScale); // transform to board space
            }
            ledBoard.attachToOutputPin();

            foreach(var touchRegion in a.touchRegions)
            {
                ledBoard.AddTouchPadAtPoint(touchRegion.X * _canvasHost.imageToOutputScale, (this._canvasHost.ImageHeight - touchRegion.Y) * _canvasHost.imageToOutputScale);
            }
      
            return ledBoard;
        }

        /// <summary>
        /// Runs the export to Gerber and saves all the files in a zip. <seealso cref="README.md"/>
        /// </summary>
        private void ExportGerber()
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Zip File|*.zip";
            var result = dialog.ShowDialog();
            if (result != true) return;

            var exporter = new GerberExporter(dialog.FileName, CreateBoardFromModel());
            exporter.Export();
        }

        /// <summary>
        /// Exports only the Eagle board file. This is useful for inspecting manually what the application is generating.
        /// </summary>
        private void ExportEagle()
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Eagle Board File|*.brd";
            var result = dialog.ShowDialog();
            if (result != true) return;
            var exporter = new EagleExporter(dialog.FileName, CreateBoardFromModel());
            exporter.Export();
        }


        /// <summary>
        /// Exports a .config file that the arduino uses to run the animation
        /// </summary>
        private void ExportAnimatiion()
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Animation File|*.h";
            var result = dialog.ShowDialog();
            if (result != true) return;

            // Create a file to write to. 
            string createText = Animation.getInstance().ToString();
            //File.Create(dialog.FileName);
            File.WriteAllText(dialog.FileName, createText);

        }

        private IList<CommandViewModel> CreateToolbarCommands()
        {
            return new List<CommandViewModel>
            {
                new CommandViewModel("Place LED", "Resources/LightBulb_32xMD.png",
                    new RelayCommand(param => this.CanvasHost.CanvasMode = CanvasHostMode.PlaceLED,
                        param => this.ImageLoaded)),

                new CommandViewModel("Choose Colors", "Resources/PaintBucket.ico",
                    new RelayCommand(param => this.CanvasHost.CanvasMode = CanvasHostMode.ColorFill,
                        param => this.ImageLoaded)),
            
                new CommandViewModel("Create Touch Region", "Resources/fingerTouch.png",
                    new RelayCommand(param => this.CanvasHost.CanvasMode = CanvasHostMode.CreateTouchRegion,
                        param => this.ImageLoaded)),
            };
        }

        private void CloseApp()
        {
            this.OnRequestClose();
        }

        private void CloseFile()
        {
            ImageLoaded = false;
            CanvasHost = null;
            CanClose = false;
        }

        public bool CanClose { get; set; }

        /// <summary>
        /// Loads the image file into a new CanvasHost model.
        /// </summary>
        private void OpenFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Images|*.jpg;*.png;*.gif";
            var result = dialog.ShowDialog();
            if (result == true)
            {
                CanvasHost = new CanvasHostViewModel(new Uri(dialog.FileName));
                ImageLoaded = true;
                CanClose = true;
                _frames.Add(new Frame());
            }
        }

        public bool ImageLoaded { get; set; }

        public void UpdateCoordinates(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(sender as IInputElement);
            this.Coordinates = String.Format("{0:F0},{1:F0}", point.X, point.Y);
        }

        public string Coordinates
        {
            get { return _coordinates; }
            set
            {
                if (value == _coordinates) return;
                _coordinates = value;
                OnPropertyChanged();
            }
        }
    }
}
