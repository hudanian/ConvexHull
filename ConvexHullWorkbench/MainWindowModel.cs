﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
//using System.Threading;
using System.Threading.Tasks;
using ConvexHullHelper;
using OxyPlot;
using OxyPlot.Series;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using System.Timers;
using System.Windows.Controls;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Mathematic;
using MarkerType = OxyPlot.MarkerType;

namespace ConvexHullWorkbench
{
	public class MainWindowModel : NotifyPropertyChangeBase
	{
		// ******************************************************************
		private string _quadrant;
		private int _iteration;
		private PlotModel _plotModel;
		private bool _isCountOfPointSpecific = true;
		private bool _isCountOfPointRandom = false;
		private int _countOfPoint = 1000;
		private int _countOfPointMin = 1000;
		private int _countOfPointMax = 1000000;
		private PointGenerator _pointGeneratorSelected = ConvexHullHelper.PointGeneratorManager.Instance.Generators[0];
		private Random _rnd = new Random();

		// ******************************************************************
		public AlgorithmManager AlgorithmManager
		{
			get { return AlgorithmManager.Instance; }
		}

		// ******************************************************************
		public PointGeneratorManager PointGeneratorManager
		{
			get { return PointGeneratorManager.Instance; }
		}

		// ******************************************************************
		public string Quadrant
		{
			get { return _quadrant; }
			set
			{
				if (value == _quadrant) return;
				_quadrant = value;
				RaisePropertyChanged();
			}
		}

		// ******************************************************************
		public int Iteration
		{
			get { return _iteration; }
			set
			{
				if (value == _iteration) return;
				_iteration = value;
				RaisePropertyChanged();
			}
		}

		public Global Global => Global.Instance;

		// ******************************************************************
		/// <summary>
		/// Gets the plot model.
		/// </summary>
		public PlotModel PlotModel
		{
			get { return _plotModel; }
			private set
			{
				if (Equals(value, _plotModel)) return;
				_plotModel = value;
				RaisePropertyChanged();
			}
		}

		// ******************************************************************
		public MainWindowModel()
		{
			////// Create the plot model
			////var tmp = new PlotModel {Title = "OxyPlot", Subtitle = "Sample lines"};

			////// Create two line series (markers are hidden by default)
			////var series1 = new LineSeries { Title = "Series 1", MarkerType = MarkerType.Circle };
			////series1.Points.Add(new DataPoint(0, 0));
			////series1.Points.Add(new DataPoint(10, 18));
			////series1.Points.Add(new DataPoint(20, 12));
			////series1.Points.Add(new DataPoint(30, 8));
			////series1.Points.Add(new DataPoint(40, 15));

			////var series2 = new LineSeries { Title = "Series 2", MarkerType = MarkerType.Square };
			////series2.Points.Add(new DataPoint(0, 4));
			////series2.Points.Add(new DataPoint(10, 12));
			////series2.Points.Add(new DataPoint(20, 16));
			////series2.Points.Add(new DataPoint(30, 25));
			////series2.Points.Add(new DataPoint(40, 5));

			////// Add the series to the plot model
			////tmp.Series.Add(series1);
			////tmp.Series.Add(series2);

			// Axes are created automatically if they are not defined


			GeneratePoints();
			ShowPoints();
			GenerateConvexHulls(new List<Algorithm> { AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullSingleThread] });

			// Set the Model property, the INotifyPropertyChanged event will make the WPF Plot control update its content
			Application.Current.Dispatcher.BeginInvoke(new Action(() =>
			{
				RaisePropertyChanged(nameof(PlotModel));
				// this.PlotModel = tmp;
			}), DispatcherPriority.ContextIdle);

			Global.Instance.PropertyChanged += InstanceOnPropertyChanged;

			IsCountOfPointRandom = true;

			AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexChan].IsSelected = true;
			AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexLiuAndChen].IsSelected = true;

			AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullSingleThread].IsSelected = true;
			AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullSingleThreadArray].IsSelected = true;
			AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullSingleThreadArrayMemCpy].IsSelected = true;
			AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullSingleThreadArrayImmu].IsSelected = true;
			//AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullSingleThreadArrayMemCpyNoIndirect].IsSelected = true;

			AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullAvl].IsSelected = true;
			AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullAvl2].IsSelected = true;

			AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHull4Threads].IsSelected = true;
			AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullMultiThreads].IsSelected = true;

			AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullCpp].IsSelected = true;
		}

		// ******************************************************************
		public string SelectedGeneratorDescription
		{
			get
			{
				int min = IsCountOfPointSpecific ? CountOfPoint : CountOfPointMin;
				int max = IsCountOfPointSpecific ? CountOfPoint : CountOfPointMax;

				return $"{PointGeneratorSelected} : {min} - {max}";
			}
		}

		// ******************************************************************
		private void InstanceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Iteration")
			{
				Iteration = Global.Iteration;
			}
		}

		// ******************************************************************
		public bool IsCountOfPointSpecific
		{
			get { return _isCountOfPointSpecific; }
			set
			{
				if (value == _isCountOfPointSpecific) return;
				_isCountOfPointSpecific = value;
				RaisePropertyChanged();
			}
		}

		// ******************************************************************
		public bool IsCountOfPointRandom
		{
			get { return _isCountOfPointRandom; }
			set
			{
				if (value == _isCountOfPointRandom) return;
				_isCountOfPointRandom = value;
				RaisePropertyChanged();
			}
		}

		// ******************************************************************
		public int CountOfPoint
		{
			get { return _countOfPoint; }
			set
			{
				if (value == _countOfPoint) return;
				_countOfPoint = value;
				RaisePropertyChanged();
			}
		}

		// ******************************************************************
		public int CountOfPointMin
		{
			get { return _countOfPointMin; }
			set
			{
				if (value == _countOfPointMin) return;
				_countOfPointMin = value;
				RaisePropertyChanged();
			}
		}

		// ******************************************************************
		public int CountOfPointMax
		{
			get { return _countOfPointMax; }
			set
			{
				if (value == _countOfPointMax) return;
				_countOfPointMax = value;
				RaisePropertyChanged();
			}
		}

		// ******************************************************************
		public int CountOfTest
		{
			get { return _countOfTest; }
			set
			{
				if (value == _countOfTest) return;
				_countOfTest = value;
				RaisePropertyChanged();
			}
		}

		// ******************************************************************
		public int CountOfSourcePoints
		{
			get { return _points.Count(); }
		}

		// ******************************************************************
		public PointGenerator PointGeneratorSelected
		{
			get { return _pointGeneratorSelected; }
			set
			{
				if (Equals(value, _pointGeneratorSelected)) return;
				_pointGeneratorSelected = value;
				RaisePropertyChanged();
			}
		}

		private Point[] _points;
		private int _countOfTest = 100;

		// ******************************************************************
		public void GeneratePoints()
		{
			int qty = CountOfPoint;

			if (IsCountOfPointRandom)
			{
				qty = (int)((_rnd.NextDouble() * (CountOfPointMax - CountOfPointMin)) + CountOfPointMin);
			}

			_points = PointGeneratorSelected.GeneratorFunc(qty);
		}

		// ******************************************************************
		public void ShowPoints()
		{
			AddMessage($"Adding {_points.Length} points in OxyPlot started.");

			var series = new ScatterSeries()
			{
				Title = PointGeneratorSelected.Name + " point generator",
				MarkerType = MarkerType.Circle,
				MarkerSize = 2
			};

			for (int ptIndex = 0; ptIndex < _points.Length; ptIndex++)
			{
				series.Points.Add(new ScatterPoint(_points[ptIndex].X, _points[ptIndex].Y)); // new DataPoint(0, 0));
			}

			var tmp = new PlotModel { Title = "Convex Hull Test Bench", Subtitle = $"'Sample test' with {series.Points.Count} points " };

			SetOxyPlotDefaultColorPalette(tmp);

			tmp.Series.Add(series);
			this.PlotModel = tmp;

			AddMessage($"Adding {_points.Length} points in OxyPlot ended.");
		}

		// ******************************************************************
		public void GenerateConvexHulls(List<Algorithm> algorithms)
		{
			while (PlotModel.Series.Count > 1)
			{
				PlotModel.Series.RemoveAt(0);
			}

			foreach (Algorithm algo in algorithms)
			{
				var series = new LineSeries { Title = algo.Name, MarkerType = MarkerType.Square, MarkerFill = algo.Color };

				var points = algo.GetHull(_points, null);

				AddMessage($"Adding {points.Length} convex hull points in OxyPlot started.");

				foreach (Point pt in points)
				{
					series.Points.Add(new DataPoint(pt.X, pt.Y));
				}

				this.PlotModel.Series.Insert(0, series);
				this.PlotModel.PlotView?.InvalidatePlot();

				AddMessage($"Adding {points.Length} convex hull points in OxyPlot ended.");
			}
		}

		// ******************************************************************
		private void SetOxyPlotDefaultColorPalette(PlotModel plotModel)
		{
			plotModel.DefaultColors = new List<OxyColor>
			{
				OxyColors.Red,
				OxyColors.Green,
				OxyColors.Blue,
				OxyColors.LightSkyBlue,
				OxyColors.Purple,
				OxyColors.HotPink,
				OxyColors.Olive,
				OxyColors.SaddleBrown,
				OxyColors.Indigo,
				OxyColors.YellowGreen,
				OxyColors.Thistle,
				OxyColors.SlateBlue,
				OxyColors.Orange,
				OxyColors.Khaki,
			};
		}

		// ******************************************************************
		public void SpeedTest(List<Algorithm> algorithms)
		{
#if DEBUG
			if (
				MessageBox.Show("You are running in debug mode. Results would not reflect the reality. Do you want to continue?",
					"Confirmation", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
			{
				return;
			}
#endif

			Global.Instance.ResetCancel();

			Dictionary<Algorithm, ScatterSeries> algoToSeries = new Dictionary<Algorithm, ScatterSeries>();

			var tmp = new PlotModel { Title = "Convex Hull Test Bench", Subtitle = $"Speed Test for {SelectedGeneratorDescription}" };

			SetOxyPlotDefaultColorPalette(tmp);

			tmp.LegendPosition = LegendPosition.LeftTop;

			foreach (var algo in algorithms)
			{
				var series = new ScatterSeries() { Title = algo.Name, MarkerType = MarkerType.Circle, MarkerSize = 2, MarkerFill = algo.Color };

				algoToSeries.Add(algo, series);
				tmp.Series.Add(series);
			}

			this.PlotModel = tmp;

			tmp.DefaultXAxis.Title = "Points";
			tmp.DefaultYAxis.Title = "Millisecs";

			Task.Run(() =>
			{
				AddMessage("Speed test started. You can look the iteration in the status bar to know the status of the test.");

				for (Iteration = 0; Iteration < CountOfTest; Iteration++)
				{
					GeneratePoints();
					foreach (var algo in algorithms)
					{
						if (Global.IsCancel)
						{
							AddMessage("Speed test canceled");
							Global.ResetCancel();
							return;
						}

						AlgorithmStat stat = new AlgorithmStat();
						Stopwatch stopwatch = Stopwatch.StartNew();
						var result = algo.GetHull(_points, stat);
						stopwatch.Stop();

						if (Global.IsCancel)
						{
							AddMessage("Speed test canceled");
							Global.ResetCancel();
							return;
						}

						stat.TimeSpanCSharp = stopwatch.Elapsed; //  TimeSpan.FromTicks(stopwatch.ElapsedTicks);
						stat.PointCount = _points.Length;
						stat.ResultCount = result.Length;

						Application.Current.Dispatcher.BeginInvoke(new Action(() =>
						{
							ScatterPoint sp = new ScatterPoint(_points.Length, stat.BestTimeSpan.TotalMilliseconds);
							algoToSeries[algo].Points.Add(sp);
						}));
					}

					Application.Current.Dispatcher.BeginInvoke(new Action(() =>
					{
						this.PlotModel.PlotView.InvalidatePlot();
					}));
				}

				Application.Current.Dispatcher.BeginInvoke(new Action(() =>
				{
					AddLinearRegression();
				}));

				AddMessage("Speed test ended.");
			});
		}

		// ******************************************************************
		private void AddLinearRegression()
		{
			if (this.IsCountOfPointRandom)
			{
				List<ScatterSeries> allSeries = new List<ScatterSeries>(PlotModel.Series.Cast<ScatterSeries>());

				foreach (ScatterSeries series in allSeries)
				{
					List<Point> points = new List<Point>(series.Points.Count);

					series.Points.ForEach(scatterPoint => points.Add(new Point(scatterPoint.X, scatterPoint.Y)));

					double rsSquared;
					double yIntercept;
					double slope;

					LinearRegression.Calc(points, 0, points.Count, out rsSquared, out yIntercept, out slope);

					Point[] pts = new Point[2];
					pts[0].X = this.CountOfPointMin;
					pts[0].Y = slope * pts[0].X + yIntercept;

					pts[1].X = this.CountOfPointMax;
					pts[1].Y = slope * pts[1].X + yIntercept;

					AddSeriesLines(pts, PlotModel, series.Title + " (Linear Regression)", MarkerType.Circle, 1, 1, series.ActualMarkerFillColor);
				}

				Application.Current.Dispatcher.BeginInvoke(new Action(() =>
				{
					this.PlotModel?.PlotView.InvalidatePlot();
				}));
			}
		}

		// ******************************************************************
		private void AddSeriesLines(IReadOnlyList<Point> points, PlotModel plotModel, string title, MarkerType markerType = MarkerType.Circle, int markerSize = 2, int strokeTickness = 2, OxyColor color = default(OxyColor))
		{
			if (points != null && points.Count > 0)
			{

				var series = new LineSeries() { Title = title, MarkerType = markerType, MarkerSize = markerSize, StrokeThickness = strokeTickness };
				for (int ptIndex = 0; ptIndex < points.Count; ptIndex++)
				{
					series.Points.Add(new DataPoint(points[ptIndex].X, points[ptIndex].Y));
				}

				if (color != default(OxyColor))
				{
					series.Color = color;
				}

				plotModel.Series.Add(series);
			}
		}

		// ******************************************************************
		public ObservableCollection<LogEntry> Messages { get; } = new ObservableCollection<LogEntry>();

		// ******************************************************************
		public void AddMessage(string message)
		{
			if (Application.Current.Dispatcher.CheckAccess())
			{
				Messages.Add(new LogEntry(Messages.Count, message));
			}
			else
			{
				Application.Current.Dispatcher.BeginInvoke(new Action(() =>
				{
					Messages.Add(new LogEntry(Messages.Count, message));
				}));
			}
		}

		// ******************************************************************
		public void AlgorithmTests(List<Algorithm> algorithms)
		{
			Global.Instance.ResetCancel();

			Func<DifferencesInPath, ExecutionState> funcShouldStopTesting = (diffs) =>
			{
				if (diffs.HasErrors)
				{
					AddMessage(diffs.Description + "\r\n");

					if (diffs.Exception == null)
					{
						var tmp = new PlotModel { Title = "Convex Hull Test Bench", Subtitle = "Tests results" };

						var series = new LineSeries()
						{
							Title = "Ref",
							MarkerType = MarkerType.Circle,
							MarkerSize = 3
						};

						for (int ptIndex = 0; ptIndex < diffs.PointsRef.Count; ptIndex++)
						{
							series.Points.Add(new DataPoint(diffs.PointsRef[ptIndex].X, diffs.PointsRef[ptIndex].Y));
							// new DataPoint(0, 0));
						}

						tmp.Series.Add(series);

						series = new LineSeries()
						{
							Title = "Points",
							MarkerType = MarkerType.Circle,
							MarkerSize = 3
						};

						for (int ptIndex = 0; ptIndex < diffs.Points.Count; ptIndex++)
						{
							series.Points.Add(new DataPoint(diffs.Points[ptIndex].X, diffs.Points[ptIndex].Y)); // new DataPoint(0, 0));
						}

						tmp.Series.Add(series);

						ScatterSeries scatterSeries = new ScatterSeries()
						{
							Title = "Unwanted points",
							MarkerType = MarkerType.Square,
							MarkerSize = 5
						};

						for (int ptIndex = 0; ptIndex < diffs.UnwantedPoints.Count; ptIndex++)
						{
							scatterSeries.Points.Add(new ScatterPoint(diffs.UnwantedPoints[ptIndex].X, diffs.UnwantedPoints[ptIndex].Y));
							// new DataPoint(0, 0));
						}

						tmp.Series.Add(scatterSeries);

						scatterSeries = new ScatterSeries()
						{
							Title = "Missing points",
							MarkerType = MarkerType.Square,
							MarkerSize = 5
						};

						for (int ptIndex = 0; ptIndex < diffs.MissingPoints.Count; ptIndex++)
						{
							scatterSeries.Points.Add(new ScatterPoint(diffs.MissingPoints[ptIndex].X, diffs.MissingPoints[ptIndex].Y));
							// new DataPoint(0, 0));
						}

						tmp.Series.Add(scatterSeries);

						this.PlotModel = tmp;
					}

					if (MessageBox.Show("Error Found! Continue ?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.OK) == MessageBoxResult.OK)
					{
						return ExecutionState.Continue;
					}

					return ExecutionState.Stop;
				}

				return ExecutionState.Continue;
			};

			Task.Run(new Action(() =>
			{
				AddMessage("Testing started: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

				try
				{
					foreach (var algo in algorithms.Where(a => a.AlgorithmType == AlgorithmType.ConvexHull && a.IsSelected).ToList())
					{
						AddMessage("Testing algorithm: " + algo.Name);

						Func<Point[], Point[]> funcConvexHull = (points) => algo.GetHull(points, null);

						ConvexHullTests tests = new ConvexHullTests(algo.Name, funcConvexHull, funcShouldStopTesting);
						tests.TestSpecialCases();
						tests.ExtensiveTests();
					}
				}
				catch (Exception)
				{
				}

				AddMessage("Testing ended: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			}));
		}

		// ******************************************************************
		public void ValidateAgainstOuelletSharpSingleThread(List<Algorithm> algorithms)
		{
			Global.Instance.Iteration = 0;

			Algorithm algoRef = AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullSingleThread];

			if (algorithms.Count == 0)
			{
				MessageBox.Show("You should at least select on algorithm that is not 'Ouellet Single Thead'.");
				return;
			}

			algorithms.RemoveAll(algo => algo == algoRef);

			Task.Run(new Action(() =>
			{
				for (Iteration = 0; Iteration < CountOfTest; Iteration++)
				{
					Global.Instance.Iteration++;

					GeneratePoints();

					var refPoints = algoRef.GetHull(_points, null);

					foreach (var algo in algorithms.Where(a => a.AlgorithmType == AlgorithmType.ConvexHull && a.IsSelected).ToList())
					{
						if (Global.Instance.IsCancel)
						{
							MessageBox.Show("Side by side test cancelled"); // GUI in model... i'm lazy, sorry.
							Global.Instance.ResetCancel();
							return;
						}

						DifferencesInPath diffs = ConvexHullUtil.GetPathDifferences(algo.Name, _points, refPoints, algo.GetHull(_points, null));

						if (diffs.HasErrors)
						{
							diffs.PointsRef.ForEach(pt => Debug.Print($"Pt: {pt}"));

							var tmp = new PlotModel { Title = "Convex Hull differences", Subtitle = "using OxyPlot" };

							AddSeriesPoints(diffs.UnwantedPoints, tmp, "Unwanted points", MarkerType.Square, 9);
							AddSeriesPoints(diffs.MissingPoints, tmp, "Missing points", MarkerType.Square, 7);

							AddSeriesLines(refPoints, tmp, "Ref lines", MarkerType.Circle, 4, 3);
							AddSeriesLines(diffs.Points, tmp, algo.Name, MarkerType.Circle, 2, 1);

							this.PlotModel = tmp;

							Application.Current.Dispatcher.BeginInvoke(new Action(() =>
							{
								this.PlotModel.PlotView.InvalidatePlot();
							}));

							var result = MessageBox.Show("Diffs found, do you want to continue to search for diffs?", "Continue?", MessageBoxButton.YesNo);
							if (result == MessageBoxResult.No)
							{
								return;
							}
						}
					}
				}

				Application.Current.Dispatcher.BeginInvoke(new Action(() =>
				{
					MessageBox.Show("Algo diffs ended.");
				}));
			}));
		}

		// ******************************************************************
		private void AddSeriesPoints(IReadOnlyList<Point> points, PlotModel plotModel, string title, MarkerType markerType = MarkerType.Circle, int markerSize = 2)
		{
			if (points != null && points.Count > 0)
			{
				var series = new ScatterSeries { Title = title, MarkerType = markerType, MarkerSize = markerSize };
				for (int ptIndex = 0; ptIndex < points.Count; ptIndex++)
				{
					series.Points.Add(new ScatterPoint(points[ptIndex].X, points[ptIndex].Y));
				}
				plotModel.Series.Add(series);
			}
		}

		// ******************************************************************
		private void AddSeriesLines(IReadOnlyList<Point> points, PlotModel plotModel, string title, MarkerType markerType = MarkerType.Circle, int markerSize = 2, int strokeTickness = 2)
		{
			if (points != null && points.Count > 0)
			{

				var series = new LineSeries() { Title = title, MarkerType = markerType, MarkerSize = markerSize, StrokeThickness = strokeTickness };
				for (int ptIndex = 0; ptIndex < points.Count; ptIndex++)
				{
					series.Points.Add(new DataPoint(points[ptIndex].X, points[ptIndex].Y));
				}
				plotModel.Series.Add(series);
			}
		}

		// ******************************************************************
		public int[] CountOfInputPoints { get; set; } = new int[] { 10, 100, 1000, 10000, 100000, 1000000, 10000000, 50000000 };

		private int _countIndexSelected = 5;
		public int CountIndexSelected
		{
			get { return _countIndexSelected; }
			set
			{
				if (_countIndexSelected == value) return;

				_countIndexSelected = value;
				RaisePropertyChanged();
			}
		}

		// ******************************************************************
		public void TestForArticle(string path)
		{
			var wb = new XLWorkbook();
			var ws = wb.Worksheets.Add("Algo stats");

			// int[] countOfInputPoints = new int[] { 10, 100, 1000, 10000, 100000 };
			// int[] countOfInputPoints = new int[] { 10, 100, 1000, 10000, 100000, 1000000, 10000000, 50000000 };

			int countOfSimulationWhereTheAverageIsDone = 10; // more representative average result

			var pointGenerators = PointGeneratorManager.Generators.Where(g => g.IsSelected).ToList();
			
			//pointGenerators.Clear();
			//pointGenerators.Add(PointGeneratorManager.Generators[4]);

			var algos = GetSelectedAlgorithms();

			//algos.Clear();
			//algos.Add(AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexHeap]);
			//algos.Add(AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexMiConvexHull]);

			//algos.Add(AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexChan]);
			//algos.Add(AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexLiuAndChen]);
			//algos.Add(AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullSingleThread]);
			//algos.Add(AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullAvl]);
			//algos.Add(AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHull4Threads]);
			//algos.Add(AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullMultiThreads]);
			//algos.Add(AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullCpp]);

			//algos.Add(AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexChan]);
			//algos.Add(AlgorithmManager.Algorithms[AlgorithmManager.AlgoIndexOuelletConvexHullAvl]);

			int dataStartCol = 2;

			int algoIndex = 0;
			int pointGeneratorIndex = 0;

			double[,,] stats = new double[algos.Count, pointGenerators.Count, CountOfInputPoints.Length];

			// Data
			for (int countOfInputPointsIndex = 0; countOfInputPointsIndex <= _countIndexSelected; countOfInputPointsIndex++)
			{
				for (int countOfSimulationWhereTheAverageIsDoneIndex = 0; countOfSimulationWhereTheAverageIsDoneIndex < countOfSimulationWhereTheAverageIsDone; countOfSimulationWhereTheAverageIsDoneIndex++)
				{
					pointGeneratorIndex = 0;

					foreach (var pointGenerator in pointGenerators)
					{
						Point[] points = pointGenerator.GeneratorFunc(CountOfInputPoints[countOfInputPointsIndex]);

						algoIndex = 0;

						foreach (var algo in algos)
						{
							AddMessage($"Input points: {CountOfInputPoints[countOfInputPointsIndex]}, Test: {countOfSimulationWhereTheAverageIsDoneIndex + 1}, Algo: {algo.Name}, Generator: {pointGenerator.Name}");

							var algoStat = new AlgorithmStat();
							Stopwatch stopWatch = new Stopwatch();
							stopWatch.Start();

							Point[] result = algo.GetHull(points, algoStat);

							stopWatch.Stop();
							algoStat.TimeSpanCSharp = stopWatch.Elapsed;

							stats[algoIndex, pointGeneratorIndex, countOfInputPointsIndex] += algoStat.BestTimeSpan.TotalMilliseconds;

							algoIndex++;
						}

						pointGeneratorIndex++;
					}
				}
			}

			int row = 3;

			var listOfMinTimePerLine = new double[CountOfInputPoints.Length];

			for (pointGeneratorIndex = 0; pointGeneratorIndex < CountIndexSelected; pointGeneratorIndex++)
			{
				ws.Cell(row++, dataStartCol - 1).Value = $"{pointGenerators[pointGeneratorIndex].Name} generator";

				// Start: Header and also Ensure everything is properly loaded in memory
				ws.Cell(row, dataStartCol - 1).Value = "Points";
				algoIndex = 0;
				foreach (var algo in algos)
				{
					ws.Cell(row, dataStartCol + algoIndex).Value = algo.Name;
					algoIndex++;
				}
				row++;
				// End: Header and also Ensure everything is properly loaded in memory


				for (int countOfInputPointsIndex = 0; countOfInputPointsIndex < CountOfInputPoints.Length; countOfInputPointsIndex++)
				{
					double minTimePerLine = 0;

					ws.Cell(row, dataStartCol - 1).Value = CountOfInputPoints[countOfInputPointsIndex];

					for (algoIndex = 0; algoIndex < algos.Count; algoIndex++)
					{
						double time = stats[algoIndex, pointGeneratorIndex, countOfInputPointsIndex] / countOfSimulationWhereTheAverageIsDone;

						ws.Cell(row, dataStartCol + algoIndex).Value = time;

						if (minTimePerLine <= 0)
						{
							minTimePerLine = time;
						}
						else
						{
							minTimePerLine = Math.Min(minTimePerLine, time);
						}
					}

					listOfMinTimePerLine[countOfInputPointsIndex] = minTimePerLine;
					row++;
				}

				ws.Cell(row, dataStartCol - 1).Value = $"{pointGenerators[pointGeneratorIndex].Name} generator ratio";
				row++;

				// Start: Header and also Ensure everything is properly loaded in memory
				ws.Cell(row, dataStartCol - 1).Value = "Points";
				algoIndex = 0;
				foreach (var algo in algos)
				{
					ws.Cell(row, dataStartCol + algoIndex).Value = algo.Name;
					algoIndex++;
				}
				row++;
				// End: Header and also Ensure everything is properly loaded in memory

				for (int countOfInputPointsIndex = 0; countOfInputPointsIndex < CountOfInputPoints.Length; countOfInputPointsIndex++)
				{
					ws.Cell(row, dataStartCol - 1).Value = CountOfInputPoints[countOfInputPointsIndex];

					for (algoIndex = 0; algoIndex < algos.Count; algoIndex++)
					{
						ws.Cell(row, dataStartCol + algoIndex).Value = stats[algoIndex, pointGeneratorIndex, countOfInputPointsIndex] /
												  countOfSimulationWhereTheAverageIsDone / listOfMinTimePerLine[countOfInputPointsIndex];
					}

					row++;
				}

				row++;
			}

			// End: Print the ratio in regards to the best one


			wb.SaveAs(path);
			Process.Start(path);
		}

		// ******************************************************************
		public List<Algorithm> GetSelectedAlgorithms()
		{
			List<Algorithm> algorithms = new List<Algorithm>();

			foreach (Algorithm algo in AlgorithmManager.Algorithms)
			{
				if (algo.IsSelected)
				{
					algorithms.Add(algo as Algorithm);
				}
			}

			return algorithms;
		}

		// ******************************************************************
		


	}
}