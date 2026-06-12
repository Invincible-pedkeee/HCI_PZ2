using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using NetworkService.Model;

namespace NetworkService.Controls
{
    public class ConnectionLinesCanvas : Canvas
    {
        private bool redrawScheduled;

        public static readonly DependencyProperty DisplaySlotsItemsControlProperty =
            DependencyProperty.Register(
                "DisplaySlotsItemsControl",
                typeof(ItemsControl),
                typeof(ConnectionLinesCanvas),
                new PropertyMetadata(null, OnDisplaySlotsItemsControlChanged));

        public ItemsControl DisplaySlotsItemsControl
        {
            get
            {
                return (ItemsControl)GetValue(DisplaySlotsItemsControlProperty);
            }
            set
            {
                SetValue(DisplaySlotsItemsControlProperty, value);
            }
        }

        public static readonly DependencyProperty ConnectionsProperty =
            DependencyProperty.Register(
                "Connections",
                typeof(ObservableCollection<ConnectionLine>),
                typeof(ConnectionLinesCanvas),
                new PropertyMetadata(null, OnConnectionsChanged));

        public ObservableCollection<ConnectionLine> Connections
        {
            get
            {
                return (ObservableCollection<ConnectionLine>)GetValue(ConnectionsProperty);
            }
            set
            {
                SetValue(ConnectionsProperty, value);
            }
        }

        public static readonly DependencyProperty DisplaySlotsProperty =
            DependencyProperty.Register(
                "DisplaySlots",
                typeof(ObservableCollection<DisplaySlot>),
                typeof(ConnectionLinesCanvas),
                new PropertyMetadata(null, OnDisplaySlotsChanged));

        public ObservableCollection<DisplaySlot> DisplaySlots
        {
            get
            {
                return (ObservableCollection<DisplaySlot>)GetValue(DisplaySlotsProperty);
            }
            set
            {
                SetValue(DisplaySlotsProperty, value);
            }
        }

        public ConnectionLinesCanvas()
        {
            Loaded += ConnectionLinesCanvas_Loaded;
            SizeChanged += ConnectionLinesCanvas_SizeChanged;
            IsHitTestVisible = false;
        }

        private void ConnectionLinesCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            ScheduleRedraw();
        }

        private void ConnectionLinesCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScheduleRedraw();
        }

        private static void OnDisplaySlotsItemsControlChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            ConnectionLinesCanvas canvas = dependencyObject as ConnectionLinesCanvas;

            if (canvas == null)
            {
                return;
            }

            ItemsControl oldItemsControl = e.OldValue as ItemsControl;
            ItemsControl newItemsControl = e.NewValue as ItemsControl;

            if (oldItemsControl != null)
            {
                oldItemsControl.ItemContainerGenerator.StatusChanged -= canvas.ItemContainerGenerator_StatusChanged;
            }

            if (newItemsControl != null)
            {
                newItemsControl.ItemContainerGenerator.StatusChanged += canvas.ItemContainerGenerator_StatusChanged;
            }

            canvas.ScheduleRedraw();
        }

        private static void OnConnectionsChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            ConnectionLinesCanvas canvas = dependencyObject as ConnectionLinesCanvas;

            if (canvas == null)
            {
                return;
            }

            INotifyCollectionChanged oldCollection = e.OldValue as INotifyCollectionChanged;
            INotifyCollectionChanged newCollection = e.NewValue as INotifyCollectionChanged;

            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= canvas.Connections_CollectionChanged;
            }

            if (newCollection != null)
            {
                newCollection.CollectionChanged += canvas.Connections_CollectionChanged;
            }

            canvas.ScheduleRedraw();
        }

        private static void OnDisplaySlotsChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            ConnectionLinesCanvas canvas = dependencyObject as ConnectionLinesCanvas;

            if (canvas == null)
            {
                return;
            }

            ObservableCollection<DisplaySlot> oldSlots = e.OldValue as ObservableCollection<DisplaySlot>;
            ObservableCollection<DisplaySlot> newSlots = e.NewValue as ObservableCollection<DisplaySlot>;

            canvas.DetachDisplaySlots(oldSlots);
            canvas.AttachDisplaySlots(newSlots);

            canvas.ScheduleRedraw();
        }

        private void Connections_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ScheduleRedraw();
        }

        private void DisplaySlots_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (DisplaySlot slot in e.OldItems)
                {
                    slot.PropertyChanged -= DisplaySlot_PropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (DisplaySlot slot in e.NewItems)
                {
                    slot.PropertyChanged += DisplaySlot_PropertyChanged;
                }
            }

            ScheduleRedraw();
        }

        private void DisplaySlot_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "OccupiedEntity" ||
                e.PropertyName == "IsOccupied" ||
                e.PropertyName == "IsValueInvalid")
            {
                ScheduleRedraw();
            }
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            ItemContainerGenerator generator = sender as ItemContainerGenerator;

            if (generator != null && generator.Status == GeneratorStatus.ContainersGenerated)
            {
                ScheduleRedraw();
            }
        }

        private void AttachDisplaySlots(ObservableCollection<DisplaySlot> slots)
        {
            if (slots == null)
            {
                return;
            }

            slots.CollectionChanged += DisplaySlots_CollectionChanged;

            foreach (DisplaySlot slot in slots)
            {
                slot.PropertyChanged += DisplaySlot_PropertyChanged;
            }
        }

        private void DetachDisplaySlots(ObservableCollection<DisplaySlot> slots)
        {
            if (slots == null)
            {
                return;
            }

            slots.CollectionChanged -= DisplaySlots_CollectionChanged;

            foreach (DisplaySlot slot in slots)
            {
                slot.PropertyChanged -= DisplaySlot_PropertyChanged;
            }
        }

        private void ScheduleRedraw()
        {
            if (redrawScheduled)
            {
                return;
            }

            redrawScheduled = true;

            Dispatcher.BeginInvoke(
                new Action(
                    delegate
                    {
                        redrawScheduled = false;
                        RedrawConnectionLines();
                    }),
                DispatcherPriority.Loaded);
        }

        private void RedrawConnectionLines()
        {
            Children.Clear();

            if (Connections == null ||
                DisplaySlots == null ||
                DisplaySlotsItemsControl == null)
            {
                return;
            }

            foreach (ConnectionLine connection in Connections)
            {
                DisplaySlot firstSlot = FindSlotForEntity(connection.FirstEntity);
                DisplaySlot secondSlot = FindSlotForEntity(connection.SecondEntity);

                if (firstSlot == null || secondSlot == null)
                {
                    continue;
                }

                Point? firstPoint = GetSlotCenter(firstSlot);
                Point? secondPoint = GetSlotCenter(secondSlot);

                if (!firstPoint.HasValue || !secondPoint.HasValue)
                {
                    continue;
                }

                Line line = new Line
                {
                    X1 = firstPoint.Value.X,
                    Y1 = firstPoint.Value.Y,
                    X2 = secondPoint.Value.X,
                    Y2 = secondPoint.Value.Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                Children.Add(line);
            }
        }

        private DisplaySlot FindSlotForEntity(NetworkEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return DisplaySlots.FirstOrDefault(slot => slot.OccupiedEntity == entity);
        }

        private Point? GetSlotCenter(DisplaySlot slot)
        {
            FrameworkElement container =
                DisplaySlotsItemsControl.ItemContainerGenerator.ContainerFromItem(slot) as FrameworkElement;

            if (container == null)
            {
                return null;
            }

            try
            {
                Point topLeft = container.TransformToVisual(this).Transform(new Point(0, 0));

                return new Point(
                    topLeft.X + container.ActualWidth / 2,
                    topLeft.Y + container.ActualHeight / 2);
            }
            catch
            {
                return null;
            }
        }
    }
}