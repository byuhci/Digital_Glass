﻿using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DigitalGlass.Commands;
using DigitalGlass.Model;
using Point = System.Windows.Point;

namespace DigitalGlass.ViewModel
{
    /// <summary>
    /// The model representing the drawing layer of the canvas. This binds to several models and is responsible for updating the canvas when the model updates.
    /// </summary>
    public class CanvasHostViewModel : ViewModelBase
    {
        public static CanvasHostViewModel self;
        private ImageSource _imageSource;
        private Bitmap _image;
        private CanvasHostMode _canvasMode;

        internal int currentFrame { get; private set; }
        internal Animation animation;
        /// <summary>
        /// Scales the 
        /// </summary>
        public double imageToOutputScale { get; private set; } = 1;

        internal void moveToFrame(int frameNum)
        {
            if (currentFrame == frameNum) return;

            currentFrame = frameNum;
            //TODO: Need a way to tell the view to change to show new colors for each cell



            ObservableCollection<Cell> tempCells = new ObservableCollection<Cell>();
            foreach(Cell c in Cells)
            {
                tempCells.Add(c);
            }

           // Cells = new ObservableCollection<Cell>();
            foreach (Cell c in tempCells)
            {
                Cells.Remove(c);
            }

            foreach (Cell c in tempCells)
            {
                Cells.Add(c);
            }            

            OnPropertyChanged();

        }


        /// <summary>
        /// Constructs a new model from an specific image source.
        /// </summary>
        /// <param name="uri">The file path to the image to load into the background layer</param>
        public CanvasHostViewModel(Uri uri)
        {
            self = this;
            animation = Animation.newInstance();

            Cells = new ObservableCollection<Cell>();
            Leds = new ObservableCollection<Led>();
            //Frames = new ObservableCollection<Frame>();
            TouchRegions = new ObservableCollection<TouchRegion>();

            currentFrame = 0;
            animation.addFrame(new Frame());

            ImageSource = new BitmapImage(uri);
            _image = new Bitmap(uri.LocalPath);
            Tolerance = 30;
            _canvasMode = CanvasHostMode.None;
            BoardHeight = 150;

        }

        /// <summary>
        /// An in-memory representation of the bitmap image background. Used to draw the image in the window. For lower-level image operations and calculates, <see cref="Image"/>
        /// </summary>
        public ImageSource ImageSource
        {
            get { return _imageSource; }
            set
            {
                if (Equals(value, _imageSource)) return;
                _imageSource = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// The tolerance used by the magic wand tool for finding cells. The higher the tolerance, the more forgiving the tool is when searching for boundaries.
        /// </summary>
        public uint Tolerance
        {
            get { return _tolerance; }
            set
            {
                if (value == _tolerance) return;
                _tolerance = value;
                OnPropertyChanged();
            }
        }

        public int ImageWidth
        {
            get { return _image.Width; }
        }

        public int ImageHeight
        {
            get { return _image.Height; }
        }

        private uint _tolerance;
        private double _boardWidth;
        private double _boardHeight;

        /// <summary>
        /// General event command
        /// </summary>
        /// <param name="startClick">The point where the mouse down event occurred</param>
        /// <param name="endClick">The point where the mouse up event occurred</param>
        public void Act(Point startClick, Point endClick)
        {
            var command = CanvasHostCommandFactory.Create(this, _canvasMode);
            if (command == null) return;
            Processing = true;
            command.Execute(startClick,endClick);
            Processing = false;
        }

        private bool _processing;

        /// <summary>
        /// True when a command is executing (asynchronously)
        /// </summary>
        public bool Processing
        {
            get { return _processing; }
            set
            {
                if (value.Equals(_processing)) return;
                _processing = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// A second, more raw representation of an image. Used for running lower-level image processing. For the higher level wrapper used to paint the image, <see cref="ImageSource"/>
        /// </summary>
        public Bitmap Image
        {
            get
            {
                return _image;
            }
        }

        /// <summary>
        /// Represents the boundaries of the cells
        /// </summary>
        public ObservableCollection<Cell> Cells { get; private set; }

//        internal System.Windows.Media.Color getCellColor(Cell c)
//        {
//            ColorCell colorCell = currentFrame.getCellColor(animation.getCellNumber(c));
//            return System.Windows.Media.Color.FromArgb(colorCell.color.A, colorCell.color.R, colorCell.color.G, colorCell.color.B);
//
 //       }


        /// <summary>
        /// Represents the location of all LEDS
        /// </summary>
        public ObservableCollection<Led> Leds { get; private set; }

        /// <summary>
        /// Represents the location of all LEDS
        /// </summary>
        public ObservableCollection<TouchRegion> TouchRegions { get; private set; }

        /// <summary>
        /// Represents the location of all the Frames
        /// </summary>
       // public ObservableCollection<Frame> Frames { get; private set; }

        /// <summary>
        /// Which tool is currently being used. Identifies how to respond to clicks.
        /// </summary>
        public CanvasHostMode CanvasMode
        {
            get { return _canvasMode; }
            set
            {
                if (value.Equals(_canvasMode)) return;
                _canvasMode = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// The width of the board. Allows the board to scale
        /// </summary>
        public uint BoardWidth
        {
            get {
                //TODO scale the board to custom size
               if(_boardWidth == 0)
                    _boardWidth = this.ImageWidth;

                return (uint) _boardWidth; 
            }
            set
            {
                if (value - _boardWidth < 2 && value - _boardWidth > -2)
                {
                    OnPropertyChanged();
                    return;
                }

                imageToOutputScale = (double)(value) / (double)(this.ImageWidth);
                 
                _boardWidth = value;
                double ratio = ((double)(this.ImageHeight) / (double)(this.ImageWidth));
                BoardHeight = (uint) (ratio * value);
                //BoardHeight = (uint)(BoardHeight * imageToOutputScale);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The height of the board. Allows the board to scale
        /// </summary>
        public uint BoardHeight
        {
            get {
                //TODO scale the board to custom size

                if (_boardHeight == 0)
                    _boardHeight = this.ImageHeight;
               
                return (uint) _boardHeight;
            }
            set
            {
                if (value - _boardHeight < 2 && value - _boardHeight > -2)
                {
                    OnPropertyChanged();
                    return;
                }

                imageToOutputScale = (double)(value) / (double)(this.ImageHeight);

                _boardHeight = value;
                double ratio = ((double)(this.ImageWidth) / (double)(this.ImageHeight));
                BoardWidth = (uint) (ratio * value);
                OnPropertyChanged();
            }
        }

    }

    public enum CanvasHostMode
    {
        None,
        ColorFill,
        PlaceLED,
        CreateTouchRegion,
        PlaceLEDWithoutCell,
    }
}