using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using NetworkService.Model;
using NetworkService.ViewModel;

namespace NetworkService.Views
{
    public partial class NetworkDisplayView : UserControl
    {
        private bool isDragging;
        private NetworkEntity draggedEntity;
        private DisplaySlot sourceSlot;
        private NetworkDisplayViewModel subscribedViewModel;

        public NetworkDisplayView()
        {
            InitializeComponent();

            Loaded += NetworkDisplayView_Loaded;
            SizeChanged += NetworkDisplayView_SizeChanged;
        }

        private void NetworkDisplayView_Loaded(object sender, RoutedEventArgs e)
        {
            AttachConnectionsCollection();
            RedrawConnectionLines();
        }

        private void NetworkDisplayView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawConnectionLines();
        }

        private void EntitiesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (isDragging)
            {
                return;
            }

            if (EntitiesTreeView.SelectedItem is NetworkEntity selectedEntity)
            {
                isDragging = true;
                draggedEntity = selectedEntity;
                sourceSlot = null;

                DragDrop.DoDragDrop(this, draggedEntity, DragDropEffects.Move);

                ResetDragState();
            }
        }

        private void EntitiesTreeView_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ResetDragState();
        }

        private void DisplaySlot_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                return;
            }

            if (IsOriginalSourceInsideButton(e.OriginalSource as DependencyObject))
            {
                return;
            }

            DisplaySlot slot = GetDisplaySlotFromSender(sender);

            if (slot == null || !slot.IsOccupied)
            {
                return;
            }

            isDragging = true;
            sourceSlot = slot;
            draggedEntity = slot.OccupiedEntity;

            DragDrop.DoDragDrop(this, draggedEntity, DragDropEffects.Move);

            ResetDragState();
        }

        private void DisplaySlot_DragOver(object sender, DragEventArgs e)
        {
            DisplaySlot targetSlot = GetDisplaySlotFromSender(sender);
            NetworkEntity entity = GetDraggedEntityFromEvent(e);

            if (entity != null && targetSlot != null && !targetSlot.IsOccupied)
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void DisplaySlot_Drop(object sender, DragEventArgs e)
        {
            DisplaySlot targetSlot = GetDisplaySlotFromSender(sender);
            NetworkEntity entity = GetDraggedEntityFromEvent(e);

            if (entity != null && targetSlot != null)
            {
                NetworkDisplayViewModel viewModel = DataContext as NetworkDisplayViewModel;

                if (viewModel != null)
                {
                    if (sourceSlot != null)
                    {
                        viewModel.MoveEntityBetweenSlots(sourceSlot, targetSlot);
                    }
                    else
                    {
                        viewModel.DropEntityToSlot(entity, targetSlot);
                    }
                }
            }

            ResetDragState();
            RedrawConnectionLines();

            e.Handled = true;
        }

        private void ConnectSlotButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            DisplaySlot slot = button.DataContext as DisplaySlot;

            if (slot == null)
            {
                return;
            }

            NetworkDisplayViewModel viewModel = DataContext as NetworkDisplayViewModel;

            if (viewModel != null)
            {
                viewModel.StartOrCompleteConnection(slot);
                RedrawConnectionLines();
            }
        }

        private void RemoveSlotButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            DisplaySlot slot = button.DataContext as DisplaySlot;

            if (slot == null)
            {
                return;
            }

            NetworkDisplayViewModel viewModel = DataContext as NetworkDisplayViewModel;

            if (viewModel != null)
            {
                viewModel.RemoveEntityFromSlot(slot);
                RedrawConnectionLines();
            }
        }

        private void AttachConnectionsCollection()
        {
            NetworkDisplayViewModel viewModel = DataContext as NetworkDisplayViewModel;

            if (subscribedViewModel == viewModel)
            {
                return;
            }

            if (subscribedViewModel != null)
            {
                subscribedViewModel.Connections.CollectionChanged -= Connections_CollectionChanged;
            }

            subscribedViewModel = viewModel;

            if (subscribedViewModel != null)
            {
                subscribedViewModel.Connections.CollectionChanged += Connections_CollectionChanged;
            }
        }

        private void Connections_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RedrawConnectionLines();
        }

        private void RedrawConnectionLines()
        {
            if (ConnectionsCanvas == null)
            {
                return;
            }

            ConnectionsCanvas.Children.Clear();

            NetworkDisplayViewModel viewModel = DataContext as NetworkDisplayViewModel;

            if (viewModel == null)
            {
                return;
            }

            foreach (ConnectionLine connection in viewModel.Connections)
            {
                DisplaySlot firstSlot = FindSlotForEntity(viewModel, connection.FirstEntity);
                DisplaySlot secondSlot = FindSlotForEntity(viewModel, connection.SecondEntity);

                if (firstSlot == null || secondSlot == null)
                {
                    continue;
                }

                Point firstPoint = GetSlotCenter(firstSlot);
                Point secondPoint = GetSlotCenter(secondSlot);

                Line line = new Line
                {
                    X1 = firstPoint.X,
                    Y1 = firstPoint.Y,
                    X2 = secondPoint.X,
                    Y2 = secondPoint.Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                ConnectionsCanvas.Children.Add(line);
            }
        }

        private DisplaySlot FindSlotForEntity(NetworkDisplayViewModel viewModel, NetworkEntity entity)
        {
            return viewModel.DisplaySlots.FirstOrDefault(slot => slot.OccupiedEntity == entity);
        }

        private Point GetSlotCenter(DisplaySlot slot)
        {
            FrameworkElement container =
                DisplaySlotsItemsControl.ItemContainerGenerator.ContainerFromItem(slot) as FrameworkElement;

            if (container == null)
            {
                return new Point(0, 0);
            }

            Point topLeft = container.TransformToVisual(ConnectionsCanvas).Transform(new Point(0, 0));

            return new Point(
                topLeft.X + container.ActualWidth / 2,
                topLeft.Y + container.ActualHeight / 2);
        }

        private DisplaySlot GetDisplaySlotFromSender(object sender)
        {
            Border border = sender as Border;

            if (border == null)
            {
                return null;
            }

            return border.DataContext as DisplaySlot;
        }

        private NetworkEntity GetDraggedEntityFromEvent(DragEventArgs e)
        {
            if (draggedEntity != null)
            {
                return draggedEntity;
            }

            if (e.Data.GetDataPresent(typeof(NetworkEntity)))
            {
                return e.Data.GetData(typeof(NetworkEntity)) as NetworkEntity;
            }

            return null;
        }

        private bool IsOriginalSourceInsideButton(DependencyObject source)
        {
            while (source != null)
            {
                if (source is Button)
                {
                    return true;
                }

                source = VisualTreeHelper.GetParent(source);
            }

            return false;
        }

        private void ResetDragState()
        {
            isDragging = false;
            draggedEntity = null;
            sourceSlot = null;
        }
    }
}